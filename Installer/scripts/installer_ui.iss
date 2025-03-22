// SoundSwitch installer UI customization
// Copyright © 2010-2025 SoundSwitch
#define installerUiIss

#ifndef installerUiIss
#define installerUiIss

#include "utility_functions.iss"

[Code]
// Create custom options page
procedure CreateCustomOptionPage();
var
  Page: TWizardPage;
  OptionsPage: TNewNotebookPage;
  DonatePanel: TPanel;
  DonateImage: TBitmapImage;
  DonateLabel: TNewStaticText;
begin
  // Create a custom page
  Page := CreateCustomPage(wpSelectTasks, 'Additional Options', 'Configure additional installation options');

  // Create options notebook page
  OptionsPage := TNewNotebookPage.Create(Page);
  OptionsPage.Notebook := Page.Notebook;
  OptionsPage.Parent := Page.Notebook;
  OptionsPage.Align := alClient;

  // Add clean settings checkbox
  CleanCheckBox := TNewCheckBox.Create(Page);
  CleanCheckBox.Parent := OptionsPage;
  CleanCheckBox.Left := 8;
  CleanCheckBox.Top := 8;
  CleanCheckBox.Width := Page.SurfaceWidth - 16;
  CleanCheckBox.Caption := ExpandConstant('{cm:ExistingSettings}');
  CleanCheckBox.Checked := False;

  // Add CLI to PATH checkbox
  AddToPathCheckBox := TNewCheckBox.Create(Page);
  AddToPathCheckBox.Parent := OptionsPage;
  AddToPathCheckBox.Left := 8;
  AddToPathCheckBox.Top := 32;
  AddToPathCheckBox.Width := Page.SurfaceWidth - 16;
  AddToPathCheckBox.Caption := ExpandConstant('{cm:AddToPath}');
  AddToPathCheckBox.Checked := False;

  // Create donation page if not suppressed via command line
  if ShowDonate() then
  begin
    // Create donate panel
    DonatePanel := TPanel.Create(Page);
    DonatePanel.Parent := OptionsPage;
    DonatePanel.Left := 0;
    DonatePanel.Top := 64;
    DonatePanel.Width := Page.SurfaceWidth;
    DonatePanel.Height := 120;
    DonatePanel.Anchors := [akLeft, akRight, akTop];

    // Add donation text
    DonateLabel := TNewStaticText.Create(Page);
    DonateLabel.Parent := DonatePanel;
    DonateLabel.Left := 8;
    DonateLabel.Top := 8;
    DonateLabel.Width := DonatePanel.Width - 16;
    DonateLabel.Height := 60;
    DonateLabel.WordWrap := True;
    DonateLabel.Caption := ExpandConstant('{cm:SupportTheProject}');
  end;
end;

// Called when the wizard initializes user interface
procedure InitializeWizard();
begin
  CreateCustomOptionPage();
end;

// Handle user selections
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpSelectTasks then
  begin
    // Store the checkbox state for use in CurStepChanged
    CleanSettings := CleanCheckBox.Checked;
  end;

  Result := True;
end;

#endif