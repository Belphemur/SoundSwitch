#Requires -Version 5.0
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$KBArticleID,

    [Parameter(Mandatory = $true)]
    [ValidateSet("CheckInstalled", "TriggerInstall")]
    [string]$Action
)

function Write-LogOutput {
    param ([string]$Message)
    Write-Output "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') PS: $Message"
}

function Test-KBInstalled {
    param ([string]$KBID)
    Write-LogOutput "Checking if $KBID is installed."
    try {
        $hotfix = Get-HotFix -Id $KBID -ErrorAction SilentlyContinue
        if ($hotfix) {
            Write-LogOutput "$KBID is installed."
            Exit 0 # Installed
        }
        else {
            Write-LogOutput "$KBID is not installed."
            Exit 1 # Not Installed
        }
    }
    catch {
        Write-LogOutput "Error checking for $KBID using Get-HotFix: $($_.Exception.Message)"
        # Fallback to WMI check if Get-HotFix fails (e.g. on older systems or if service is off)
        try {
            Write-LogOutput "Attempting WMI check for $KBID."
            $wmiHotfix = Get-WmiObject -Class Win32_QuickFixEngineering -Filter "HotFixID = '$KBID'" -ErrorAction SilentlyContinue
            if ($wmiHotfix) {
                Write-LogOutput "$KBID found via WMI."
                Exit 0 # Installed
            }
            else {
                Write-LogOutput "$KBID not found via WMI."
                Exit 1 # Not Installed
            }
        }
        catch {
            Write-LogOutput "Error checking for $KBID using WMI: $($_.Exception.Message)"
            Exit 2 # Error during check
        }
    }
}

function Invoke-KBInstall {
    param ([string]$KBID)
    Write-LogOutput "Attempting to trigger installation for $KBID via Windows Update service."

    try {
        Write-LogOutput "Creating Update Session..."
        $updateSession = New-Object -ComObject Microsoft.Update.Session
        $updateSearcher = $updateSession.CreateUpdateSearcher()
        
        Write-LogOutput "Searching for update $KBID..."
        # Search criteria for a specific KB article.
        # May need refinement if the KBID isn't directly searchable this way,
        # or if it's part of a cumulative update.
        # Using a general search for the KBID in the title or description.
        # Use a more targeted search to reduce the number of updates returned.
        # Try to match the KBID in the title, which is usually present for individual updates.
        $searchCriteria = "IsInstalled=0 and Type='Software' and Title like '%$KBID%'"
        # Note: The Windows Update API does not support direct search by KBArticleID in the query string.
        # If $KBID is in the form "KB123456", this will match updates with that KB in the title.
        # It's often better to search for "KBxxxxxxx" within the title or description if a direct ID search fails.
        # $searchCriteria = "IsInstalled=0 and Type='Software' and Title like '%$KBID%'"
        # For more specific targeting, one might need to know if it's a security update, critical update etc.
        
        $searchResult = $updateSearcher.Search($searchCriteria)

        if ($searchResult.Updates.Count -eq 0) {
            Write-LogOutput "No applicable updates found for $KBID with current criteria. It might be already installed, superseded, or not applicable."
            # Check if it's already installed as a fallback, as search criteria might miss it.
            $hotfixCheck = Get-HotFix -Id $KBID -ErrorAction SilentlyContinue
            if ($hotfixCheck) {
                Write-LogOutput "$KBID appears to be already installed (found by Get-HotFix during trigger)."
                Exit 0 # Already installed
            }
            Exit 3 # Update not found or not applicable
        }

        $updatesToDownload = New-Object -ComObject Microsoft.Update.UpdateColl
        $updateFound = $false

        foreach ($update in $searchResult.Updates) {
            # Iterate through KBNames if available, or check title/description
            $kbMatch = $false
            if ($update.KBArticleIDs) {
                foreach ($kb in $update.KBArticleIDs) {
                    if ($kb -eq $KBID.Replace("KB", "")) { $kbMatch = $true; break }
                }
            }
            if (-not $kbMatch -and ($update.Title -like "*$KBID*" -or ($update.Description -and $update.Description -like "*$KBID*"))) {
                $kbMatch = $true
            }

            if ($kbMatch) {
                Write-LogOutput "Found update: $($update.Title)"
                if ($update.IsDownloaded) {
                    Write-LogOutput "Update '$($update.Title)' is already downloaded."
                }
                else {
                    Write-LogOutput "Adding update '$($update.Title)' to download list."
                }
                $updatesToDownload.Add($update) | Out-Null
                $updateFound = $true
                # Typically, we'd install one specific update if found.
                # If multiple updates match a loose criteria, this logic might need adjustment.
                # For a specific KB, we usually expect one primary match.
                break 
            }
        }

        if (-not $updateFound) {
            Write-LogOutput "Specific update $KBID not found in search results, though other updates were listed."
            Exit 3 # Update not found
        }

        # Sanity check - if we reach here, we should have at least one update
        if ($updatesToDownload.Count -eq 0) {
            Write-LogOutput "Unexpected state: updateFound was true but no updates in collection. This should not happen."
            Exit 5 # Unexpected error state
        }

        $needsDownload = $false
        foreach ($updateToInstall in $updatesToDownload) {
            if (-not $updateToInstall.IsDownloaded) {
                $needsDownload = $true
                break
            }
        }

        if ($needsDownload) {
            Write-LogOutput "Downloading updates..."
            $downloader = $updateSession.CreateUpdateDownloader()
            $downloader.Updates = $updatesToDownload
            $downloader.Download() | Out-Null
            Write-LogOutput "Download Result: $($downloader.GetProgress().ResultCode)" # Log download result
        }
        else {
            Write-LogOutput "All required updates are already downloaded."
        }
        
        Write-LogOutput "Installing updates..."
        $installer = $updateSession.CreateUpdateInstaller()
        $installer.Updates = $updatesToDownload
        
        # For non-interactive install, this should be fine.
        # If user interaction is somehow triggered, this might hang in a hidden window.
        # The ewNoWait in InnoSetup should help if this call blocks for too long.
        $installationResult = $installer.Install()
        
        Write-LogOutput "Installation Result Code: $($installationResult.ResultCode)"
        Write-LogOutput "Reboot Required: $($installationResult.RebootRequired)"

        if ($installationResult.ResultCode -eq 2) {
            # or suSucceeded
            Write-LogOutput "Installation of $KBID appears successful."
            Exit 0 # Success
        }
        else {
            Write-LogOutput "Installation of $KBID may have failed or requires attention. ResultCode: $($installationResult.ResultCode)"
            Exit 4 # Installation failed or other issue
        }

    }
    catch {
        $exceptionMessage = $_.Exception.Message
        $scriptStackTraceFull = $_.ScriptStackTrace
        Write-LogOutput ("Error during Windows Update operations for {0}: {1}" -f $KBID, $exceptionMessage)
        Write-LogOutput ("ScriptStackTrace: {0}" -f $scriptStackTraceFull)
        Exit 5 # Error during update process
    }
}


# Main script logic
Write-LogOutput "ManageWindowsUpdate.ps1 called with Action: $Action, KBArticleID: $KBArticleID"

switch ($Action) {
    "CheckInstalled" {
        Test-KBInstalled -KBID $KBArticleID
    }
    "TriggerInstall" {
        Invoke-KBInstall -KBID $KBArticleID
    }
    default {
        Write-LogOutput "Invalid action: $Action"
        Exit 99 # Invalid action
    }
}