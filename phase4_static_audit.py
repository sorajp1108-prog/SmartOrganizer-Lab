from pathlib import Path
import re,sys,xml.etree.ElementTree as ET,json,hashlib
r=Path(__file__).resolve().parents[1]
for f in ['App.xaml','MainWindow.xaml']:ET.parse(r/'src/SmartOrganizer.WinUI'/f)
ET.parse(r/'src/SmartOrganizer.WinUI/SmartOrganizer.WinUI.csproj');ET.parse(r/'src/SmartOrganizer.WinUI/Package.appxmanifest')
x=(r/'src/SmartOrganizer.WinUI/MainWindow.xaml').read_text();c=(r/'src/SmartOrganizer.WinUI/MainWindow.xaml.cs').read_text();svc=(r/'src/SmartOrganizer.WinUI/Operations/SafeOperationService.cs').read_text()
events=set(re.findall(r'(?:Click|Invoked|TextChanged|SelectionChanged|ItemClick|DoubleTapped|DragDelta)="([A-Za-z0-9_]+)"',x));methods=set(re.findall(r'\b(?:void|Task)\s+([A-Za-z0-9_]+)\s*\(',c))
checks={'xaml handlers resolved':not(events-methods),'x64 only':'Any CPU' not in (r/'SmartOrganizer.sln').read_text(),'phase1 retained':'HorizontalScrollMode="Enabled"' in x and 'ColumnSplitter_DragDelta' in x,'phase2 retained':'SearchBox_TextChanged' in x and 'FileList_DoubleTapped' in x,'phase3 retained':'.partial' in svc and 'FixedTimeEquals' in svc,'core test project':(r/'tests/SmartOrganizer.OperationTests/Program.cs').exists(),'windows build script':(r/'eng/phase4-windows.ps1').exists(),'msix lifecycle script':(r/'eng/msix-install-test.ps1').exists(),'ui checklist':(r/'eng/ui-manual-checklist.md').exists(),'evidence workflow':(r/'.github/workflows/windows-phase4.yml').exists(),'no signing secret committed':not any(r.rglob('*.pfx'))}
(r/'evidence/static-audit.json').write_text(json.dumps({'checks':checks,'missing_handlers':sorted(events-methods)},ensure_ascii=False,indent=2))
for k,v in checks.items():print(('PASS' if v else 'FAIL'),k)
if not all(checks.values()):sys.exit(1)
for p in sorted(q for q in r.rglob('*') if q.is_file() and 'evidence' not in q.parts):
 h=hashlib.sha256(p.read_bytes()).hexdigest();print(h,' ',p.relative_to(r),sep='')
