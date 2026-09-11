from pathlib import Path
import sys,xml.etree.ElementTree as ET
r=Path(__file__).resolve().parents[1];svc=(r/'src/SmartOrganizer.WinUI/Operations/SafeOperationService.cs').read_text();j=(r/'src/SmartOrganizer.WinUI/Operations/OperationJournal.cs').read_text();x=(r/'src/SmartOrganizer.WinUI/MainWindow.xaml').read_text();c=(r/'src/SmartOrganizer.WinUI/MainWindow.xaml.cs').read_text()
ET.parse(r/'src/SmartOrganizer.WinUI/MainWindow.xaml')
checks={'preview required':'PreviewAsync' in svc and 'PreviewAndExecuteAsync' in c,'explicit confirmation':'ContentDialogResult.Primary' in c,'no overwrite':'同名ファイルが既にあります' in svc and 'File.Move(stage,plan.DestinationPath,false)' in svc,'staging':'.partial' in svc,'sha256':'SHA256' in svc and 'FixedTimeEquals' in svc,'move revalidation':'RevalidateSourceBeforeRemoval' in svc,'cancel cleanup':'OperationCanceledException' in svc and 'File.Delete(stage)' in svc,'copy undo guarded':'DestinationHash' in svc and 'record.Kind==OperationKind.Copy' in svc,'move undo collision guard':'File.Exists(record.SourcePath)' in svc,'atomic journal':('File.Replace(temp,path,backup,true)' in j or 'File.Replace(temp,path,backupPath,true)' in j),'recovery candidates':'OperationState.Running' in svc,'phase2 retained':'SearchBox_TextChanged' in x and 'FileList_DoubleTapped' in x,'phase1 retained':'HorizontalScrollMode="Enabled"' in x and 'ColumnSplitter_DragDelta' in x}
for k,v in checks.items():print(('PASS' if v else 'FAIL'),k)
if not all(checks.values()):sys.exit(1)
print('PASS',len(checks),'checks')
