from pathlib import Path
import json,sys,xml.etree.ElementTree as ET
r=Path(__file__).resolve().parents[1];m=(r/'src/SmartOrganizer.WinUI/Package.appxmanifest').read_text();w=(r/'.github/workflows/windows-store-candidate.yml').read_text();b=(r/'eng/build-store-upload.ps1').read_text();a=(r/'eng/apply-store-identity.ps1').read_text()
ET.parse(r/'src/SmartOrganizer.WinUI/Package.appxmanifest')
checks={'store identity template':(r/'store/store-identity.template.json').exists(),'placeholder guarded':'REPLACE_' in m and 'REPLACE_' in a,'identity apply script':'Package.appxmanifest' in a and 'PublisherDisplayName' in a,'store readiness validation':'validate-store-readiness.ps1' in b,'store upload build':'UapAppxPackageBuildMode=StoreUpload' in b,'production signing disabled locally':'AppxPackageSigningEnabled=false' in b,'protected CI variables':'vars.STORE_IDENTITY_NAME' in w and 'vars.STORE_PUBLISHER' in w,'no pfx committed':not any(r.rglob('*.pfx')),'listing drafts':all((r/'store/listing-ja'/f).exists() for f in ['短い説明.txt','説明.txt','機能.txt','キーワード.txt','リリースノート.txt']),'privacy draft':(r/'store/privacy-policy-draft.md').exists(),'support draft':(r/'store/support-page-draft.md').exists(),'partner checklist':(r/'store/partner-center-checklist.md').exists(),'single user entry':'Microsoft Store' in (r/'docs/Phase5実行状況.md').read_text() and 'CMD、PowerShell、Visual Studioを利用者へ要求しない' in (r/'docs/Phase5実行状況.md').read_text(),'phase4 retained':(r/'eng/phase4-windows.ps1').exists()}
(r/'evidence/phase5-static.json').write_text(json.dumps(checks,ensure_ascii=False,indent=2))
for k,v in checks.items():print(('PASS' if v else 'FAIL'),k)
if not all(checks.values()):sys.exit(1)
print('PASS',len(checks),'checks')
