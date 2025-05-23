# Plan: Integrate Windows Update Check (KB5053606) into SoundSwitch InnoSetup Installer

**Overall Goal:**
The installer will first check if the operating system is Windows 10 LTSC. If it is, it will then use a PowerShell script to check if Windows Update KB5053606 is installed. If the update is not installed, the same PowerShell script will be invoked to attempt to trigger its installation via the Windows Update service. All operations and their outcomes will be logged to the InnoSetup log file. The main SoundSwitch installation will proceed regardless of the outcome of these update checks or installation attempts (silent failure with logging).

**Detailed Plan:**

1.  **Helper Script (PowerShell):**

    - **Filename:** `ManageWindowsUpdate.ps1`
    - **Location:** To be placed in `Installer/scripts/` relative to `setup.iss`.
    - **Functionality:**
      - Accepts parameters:
        - `-KBArticleID <string>` (e.g., "KB5053606")
        - `-Action <string>` (e.g., "CheckInstalled" or "TriggerInstall")
      - **If `Action` is "CheckInstalled":**
        - Uses `Get-HotFix -Id $KBArticleID` or queries WMI (`Get-WmiObject -Class Win32_QuickFixEngineering -Filter "HotFixID='$KBArticleID'"`) to check if the specified KB is installed.
        - Exits with code `0` if found.
        - Exits with code `1` if not found.
        - Exits with other codes for errors (e.g., `2` if `Get-HotFix` fails or WMI query fails).
        - Logs its findings (e.g., using `Write-Output` for InnoSetup to capture, or to a dedicated log if more detail is needed from the PS script).
      - **If `Action` is "TriggerInstall":**
        - Uses the Windows Update Agent API (e.g., `Microsoft.Update.Session`, `Microsoft.Update.Searcher`, `IUpdateDownloader`, `IUpdateInstaller2`) to search for the specific KB, download it if necessary, and initiate the installation.
        - Logs its progress and outcome.
        - This action should be designed to run without blocking the InnoSetup installer for too long (e.g., by initiating the update and exiting).

2.  **Inno Setup Script (`Installer/setup.iss`) Modifications:**

    - **`[Files]` Section:**

      - Add an entry to include `ManageWindowsUpdate.ps1`. It will be extracted to `{tmp}` during setup and deleted after installation.

      ```iss
      [Files]
      ; ... existing files ...
      Source: "scripts\ManageWindowsUpdate.ps1"; DestDir: "{tmp}"; Flags: confirmoverwrite deleteafterinstall
      ```

    - **`[Code]` Section:**
      - **Constants:**
        ```pascal
        const
          KBID_TO_CHECK = 'KB5053606';
          LTSC_PRODUCT_NAME_1 = 'Windows 10 Enterprise LTSC';
          LTSC_PRODUCT_NAME_2 = 'Windows 10 Enterprise LTSB'; // Older naming
        ```
      - **Logging Function:**
        ```pascal
        procedure LogMsg(Msg: String);
        begin
          Log(FormatDateTime('yyyy-mm-dd hh:nn:ss', Now) + ': ' + Msg);
        end;
        ```
      - **`IsWin10LTSC(): Boolean;` Function:**
        - Reads `ProductName` from `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion`.
        - Checks if it contains `LTSC_PRODUCT_NAME_1` or `LTSC_PRODUCT_NAME_2`.
        - Logs the detected OS and the result of the LTSC check.
        - Returns `True` if LTSC, `False` otherwise. Logs an error and returns `False` if the registry key cannot be read.
      - **`IsKBInstalledPS(KBID: String): Boolean;` Function:**
        - Locates `powershell.exe`.
        - Constructs the command: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "{tmp}\ManageWindowsUpdate.ps1" -KBArticleID "<KBID>" -Action CheckInstalled`.
        - Executes this command hidden using `Exec` and waits for it to terminate.
        - Checks the `ExitCode`. `0` means installed (`True`). `1` means not installed (`False`). Any other code means an error occurred (treat as `False`, log the actual exit code).
        - Logs the command executed and the outcome.
      - **`AttemptKBInstallPS(KBID: String);` Procedure:**
        - Locates `powershell.exe`.
        - Constructs the command: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "{tmp}\ManageWindowsUpdate.ps1" -KBArticleID "<KBID>" -Action TriggerInstall`.
        - Executes this command hidden using `Exec` with the `ewNoWait` flag.
        - Logs the attempt to trigger the installation.
      - **`InitializeSetup(): Boolean;` Event Function:**
        - This function is called when setup is initializing.
        - Calls `LogMsg('Starting Windows Update prerequisite check.')`.
        - Calls `IsWin10LTSC()`.
        - If LTSC is detected:
          - Calls `IsKBInstalledPS(KBID_TO_CHECK)`.
          - If the KB is not installed, calls `AttemptKBInstallPS(KBID_TO_CHECK)`.
        - Else (not LTSC):
          - Calls `LogMsg('System is not Windows 10 LTSC. Skipping update check for ' + KBID_TO_CHECK + '.')`.
        - All results and error conditions encountered by these functions will be logged.
        - _Always_ returns `True` to allow the SoundSwitch installation to proceed.
        - Calls `LogMsg('Windows Update prerequisite check finished. Proceeding with main installation.')`.
    - Ensure `SetupLogging=yes` is present in the `[Setup]` section.

**Mermaid Diagram of the Logic:**

```mermaid
graph TD
    A[Installer Starts] --> LogInit[Log: Begin Update Check];
    LogInit --> B{Call IsWin10LTSC()};
    B -- True --> LogLTSC[Log: LTSC Detected];
    LogLTSC --> C{Call IsKBInstalledPS('KB5053606') via ManageWindowsUpdate.ps1 -Action CheckInstalled};
    B -- False --> LogNotLTSC[Log: Not LTSC / Skip Update Check];
    LogNotLTSC --> Z[Proceed with SoundSwitch Installation];
    C -- Exit Code 0 (Installed) --> LogKBFound[Log: KB5053606 Installed];
    LogKBFound --> Z;
    C -- Exit Code 1 (Not Installed) --> LogKBNotFound[Log: KB5053606 Not Installed];
    LogKBNotFound --> D[Call AttemptKBInstallPS('KB5053606') via ManageWindowsUpdate.ps1 -Action TriggerInstall];
    D --> LogAttempt[Log: Attempted to Trigger KB Install];
    LogAttempt --> Z;
    C -- Other Exit Codes (Error) --> LogKBCheckError[Log: Error checking KB5053606 (Exit Code: <val>)];
    LogKBCheckError --> Z;
    B -- Error During LTSC Check --> LogLTSCError[Log: Error Checking LTSC];
    LogLTSCError --> Z;
    Z --> LogFinal[Log: Update Check Finished. Proceeding with Main Install.]
```
