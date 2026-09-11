param([Parameter(Mandatory=$true)][string]$PackagePath,[Parameter(Mandatory=$true)][string]$PackageName)
$ErrorActionPreference='Stop'
# Development package must already be signed and its test certificate trusted on this managed test PC.
Add-AppxPackage -Path $PackagePath
$installed=Get-AppxPackage -Name $PackageName
if(!$installed){throw 'Install verification failed'}
Write-Host "PASS install: $($installed.PackageFullName)"
Remove-AppxPackage -Package $installed.PackageFullName
if(Get-AppxPackage -Name $PackageName){throw 'Uninstall verification failed'}
Write-Host 'PASS uninstall'
