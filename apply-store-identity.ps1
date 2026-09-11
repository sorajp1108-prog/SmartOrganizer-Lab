param([string]$IdentityFile = "$PSScriptRoot\..\store\store-identity.json")
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(!(Test-Path $IdentityFile)){throw "Store identity file not found: $IdentityFile. Copy store-identity.template.json to store-identity.json and paste the exact Partner Center values."}
$id=Get-Content $IdentityFile -Raw | ConvertFrom-Json
$required=@('identityName','publisher','publisherDisplayName','displayName','version')
foreach($name in $required){if([string]::IsNullOrWhiteSpace($id.$name) -or $id.$name -match 'REPLACE_'){throw "Invalid Store identity value: $name"}}
$manifest=Join-Path $root 'src\SmartOrganizer.WinUI\Package.appxmanifest'
[xml]$xml=Get-Content $manifest -Raw
$ns=New-Object System.Xml.XmlNamespaceManager($xml.NameTable);$ns.AddNamespace('m','http://schemas.microsoft.com/appx/manifest/foundation/windows10')
$identity=$xml.SelectSingleNode('/m:Package/m:Identity',$ns);$identity.Name=$id.identityName;$identity.Publisher=$id.publisher;$identity.Version=$id.version
$props=$xml.SelectSingleNode('/m:Package/m:Properties',$ns);$props.DisplayName=$id.displayName;$props.PublisherDisplayName=$id.publisherDisplayName
$visual=$xml.SelectSingleNode('/m:Package/m:Applications/m:Application/*[local-name()="VisualElements"]',$ns);$visual.DisplayName=$id.displayName
$xml.Save($manifest)
Write-Host 'PASS Store identity applied to Package.appxmanifest'
