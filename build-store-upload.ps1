$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'validate-store-readiness.ps1')
$ev=Join-Path $root 'evidence';New-Item -ItemType Directory -Force $ev|Out-Null
Start-Transcript -Path (Join-Path $ev 'store-build-transcript.txt') -Force
try {
 dotnet restore (Join-Path $root 'SmartOrganizer.sln') -r win-x64
 dotnet run --project (Join-Path $root 'tests\SmartOrganizer.OperationTests\SmartOrganizer.OperationTests.csproj') -c Release | Tee-Object (Join-Path $ev 'store-core-tests.txt')
 dotnet build (Join-Path $root 'SmartOrganizer.sln') -c Release -p:Platform=x64 -p:AppxBundle=Never -p:UapAppxPackageBuildMode=StoreUpload -p:AppxPackageSigningEnabled=false -bl:(Join-Path $ev 'store-build.binlog') | Tee-Object (Join-Path $ev 'store-build.txt')
 $packages=Get-ChildItem $root -Recurse -Include *.msixupload,*.appxupload,*.msix
 if(!$packages){throw 'No Store upload package was generated'}
 $packages|ForEach-Object{Get-FileHash $_.FullName -Algorithm SHA256}|Format-Table -Auto|Out-File (Join-Path $ev 'store-package-SHA256.txt')
 Write-Host 'PASS Store upload candidate generated. Do not distribute this file directly to users.'
} finally {Stop-Transcript}
