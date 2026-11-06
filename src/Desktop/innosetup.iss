; Windows installer for Bricks Breaker (Inno Setup 6).
; 1. dotnet publish src/Desktop/Breakout.Desktop.csproj -p:PublishProfile=FolderProfile
; 2. compile this script, paths are relative to this file.

#define MyAppName "Bricks Breaker"
#define MyAppVersion "1.3"
#define MyAppPublisher "AppoMobi"
#define MyAppURL "https://github.com/taublast/DrawnUi.Breakout"
#define MyAppExeName "BricksBreaker.exe"

[Setup]
AppId={{5B3C2E5D-8F0A-4C61-9E7B-2D4A9C1F7E30}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=icon.ico
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=bin\Release\installer
OutputBaseFilename=BricksBreakerSetup
SolidCompression=yes
WizardStyle=modern dynamic

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net10.0\publish\win-x64\*.*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
