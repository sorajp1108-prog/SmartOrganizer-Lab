from pathlib import Path
import sys,xml.etree.ElementTree as ET
r=Path(__file__).resolve().parents[1];x=(r/'src/SmartOrganizer.WinUI/MainWindow.xaml').read_text();c=(r/'src/SmartOrganizer.WinUI/MainWindow.xaml.cs').read_text();cat=(r/'src/SmartOrganizer.WinUI/Services/ReadOnlyFileCatalog.cs').read_text();vm=(r/'src/SmartOrganizer.WinUI/ViewModels/MainViewModel.cs').read_text();watch=(r/'src/SmartOrganizer.WinUI/Services/FileCatalogWatcher.cs').read_text()
for f in ['App.xaml','MainWindow.xaml']:ET.parse(r/'src/SmartOrganizer.WinUI'/f)
checks={'read-only catalog':'IReadOnlyFileCatalog' in cat and ('EnumerateFiles' in cat or 'SafeFileEnumerator' in cat),'no write operations':all(q not in cat for q in ['File.Delete','File.Move','File.Copy','WriteAll']),'categories':'CategoryOf' in cat and 'Categories' in vm,'search debounce':'300' in c and 'SearchBox_TextChanged' in x,'filters':all(q in x for q in ['KindFilter','SourceFilter','ExtensionFilter']),'sort':'SortFilter' in x and 'SortMode' in c,'watcher':'FileSystemWatcher' in watch and '450' in watch,'cancellation':'CancellationTokenSource' in vm and 'OperationCanceledException' in vm,'open default':'UseShellExecute=true' in c,'history available':('HistoryReader' in c or 'operations.HistoryAsync' in c) and (r/'src/SmartOrganizer.WinUI/Operations/OperationJournal.cs').exists(),'phase1 horizontal':'HorizontalScrollMode="Enabled"' in x,'phase1 columns':'ColumnSplitter_DragDelta' in x}
for k,v in checks.items():print(('PASS' if v else 'FAIL'),k)
if not all(checks.values()):sys.exit(1)
print('PASS',len(checks),'checks')
