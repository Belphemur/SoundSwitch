// SoundSwitch PATH manipulation operations
// Copyright © 2010-2025 SoundSwitch

[Code]
procedure ModifyPath(const ValueToAdd: string; PathType: Integer);
var
  OldPath: string;
  NewPath: string;
  RegPathKey: string;
begin
  // PathType: 0 = user path, 1 = system path
  if PathType = 0 then
    RegPathKey := 'HKCU\Environment'
  else
    RegPathKey := 'HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment';
  
  // Get the old path value
  if not RegQueryStringValue(RegPathKey, 'Path', OldPath) then
    OldPath := '';
    
  // Check if it already exists in the path
  if Pos(UpperCase(ValueToAdd), UpperCase(OldPath)) > 0 then
    Exit; // Already in the path
    
  // Append to the path, ensuring proper separation with semicolons
  if Copy(OldPath, Length(OldPath), 1) <> ';' then
    NewPath := OldPath + ';' + ValueToAdd
  else
    NewPath := OldPath + ValueToAdd;
    
  // Write the new path back to registry
  if RegWriteStringValue(RegPathKey, 'Path', NewPath) then
  begin
    // Notify Windows about the environment change
    if PathType = 1 then
      // Use SendMessageTimeout for system-wide environment change
      SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 0, 'Environment', SMTO_ABORTIFHUNG, 5000, 0);
  end;
end;

procedure RemoveFromPath(const ValueToRemove: string; PathType: Integer);
var
  OldPath: string;
  NewPath: string;
  RegPathKey: string;
  P: Integer;
  PathLen: Integer;
begin
  // PathType: 0 = user path, 1 = system path
  if PathType = 0 then
    RegPathKey := 'HKCU\Environment'
  else
    RegPathKey := 'HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment';
  
  // Get the old path value
  if not RegQueryStringValue(RegPathKey, 'Path', OldPath) then
    Exit; // No path to modify
    
  // Find the position of the value in the path
  P := Pos(UpperCase(ValueToRemove), UpperCase(OldPath));
  if P <= 0 then
    Exit; // Not in the path
    
  PathLen := Length(ValueToRemove);
  // Remove the value from the path
  if (P > 1) and (OldPath[P-1] = ';') then
  begin
    // If preceded by a semicolon, remove that too
    Delete(OldPath, P-1, PathLen+1);
  end
  else if (P + PathLen <= Length(OldPath)) and (OldPath[P + PathLen] = ';') then
  begin
    // If followed by a semicolon, remove that too
    Delete(OldPath, P, PathLen+1);
  end
  else
  begin
    // Just remove the value itself
    Delete(OldPath, P, PathLen);
  end;
  
  // Clean up any double semicolons
  OldPath := StringReplace(OldPath, ';;', ';', [rfReplaceAll]);
  
  // If path ends with semicolon, remove it
  if (Length(OldPath) > 0) and (OldPath[Length(OldPath)] = ';') then
    OldPath := Copy(OldPath, 1, Length(OldPath)-1);
  
  // Write the new path back to registry
  if RegWriteStringValue(RegPathKey, 'Path', OldPath) then
  begin
    // Notify Windows about the environment change
    if PathType = 1 then
      SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 0, 'Environment', SMTO_ABORTIFHUNG, 5000, 0);
  end;
end;