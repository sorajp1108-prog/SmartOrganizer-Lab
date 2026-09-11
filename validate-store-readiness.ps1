$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
[xml]$m=Get-Content (Join-Path $root 'src\SmartOrganizer.WinUI\Package.appxmanifest') -Raw
$text=$m.OuterXml
if($text -match 'REPLACE_WITH|Placeholder'){throw 'Store identity placeholders remain in Package.appxmanifest'}
if(!(Test-Path (Join-Path $root 'store\assets\StoreLogo.png'))){throw 'StoreLogo.png is missing'}
if(!(Test-Path (Join-Path $root 'store\assets\Square44x44Logo.png'))){throw 'Square44x44Logo.png is missing'}
if(!(Test-Path (Join-Path $root 'store\assets\Square150x150Logo.png'))){throw 'Square150x150Logo.png is missing'}
Write-Host 'PASS Store readiness precheck'
