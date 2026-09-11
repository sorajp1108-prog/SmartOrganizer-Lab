$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$ev=Join-Path $root 'evidence'
New-Item -ItemType Directory -Force $ev | Out-Null
Start-Transcript -Path (Join-Path $ev 'phase4-transcript.txt') -Force
try {
 dotnet --info | Out-File (Join-Path $ev 'dotnet-info.txt')
 dotnet restore (Join-Path $root 'SmartOrganizer.sln') -r win-x64
 dotnet build (Join-Path $root 'tests/SmartOrganizer.OperationTests/SmartOrganizer.OperationTests.csproj') -c Release
 dotnet run --project (Join-Path $root 'tests/SmartOrganizer.OperationTests/SmartOrganizer.OperationTests.csproj') -c Release | Tee-Object (Join-Path $ev 'core-tests.txt')
 dotnet build (Join-Path $root 'SmartOrganizer.sln') -c Release -p:Platform=x64 -p:AppxBundle=Never -p:UapAppxPackageBuildMode=SideloadOnly -bl:(Join-Path $ev 'winui-build.binlog') | Tee-Object (Join-Path $ev 'winui-build.txt')
 Get-ChildItem $root -Recurse -Include *.msix,*.msixbundle,*.appxupload | ForEach-Object { Get-FileHash $_.FullName -Algorithm SHA256 } | Format-Table -Auto | Out-File (Join-Path $ev 'SHA256SUMS.txt')
} finally { Stop-Transcript }
