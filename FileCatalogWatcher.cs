namespace SmartOrganizer.WinUI.Services;
public enum WatcherSignal { Changed,OverflowOrError,PeriodicRescan }
public sealed class FileCatalogWatcher:IDisposable
{
 FileSystemWatcher? watcher;readonly TimeSpan delay=TimeSpan.FromMilliseconds(450);readonly TimeSpan periodic=TimeSpan.FromMinutes(30);CancellationTokenSource? debounce,periodicCts;WatcherSignal pending;
 public event EventHandler<WatcherSignal>? Signaled;
 public void Start(string root,bool includeSubfolders=false){DisposeWatcher();if(!Directory.Exists(root))return;watcher=new(root){IncludeSubdirectories=includeSubfolders,NotifyFilter=NotifyFilters.FileName|NotifyFilters.DirectoryName|NotifyFilters.LastWrite|NotifyFilters.Size,InternalBufferSize=64*1024};watcher.Created+=OnChange;watcher.Changed+=OnChange;watcher.Deleted+=OnChange;watcher.Renamed+=OnRename;watcher.Error+=OnError;watcher.EnableRaisingEvents=true;var c=periodicCts=new();_ = Task.Run(async()=>{try{while(!c.IsCancellationRequested){await Task.Delay(periodic,c.Token);Signaled?.Invoke(this,WatcherSignal.PeriodicRescan);}}catch(OperationCanceledException){}});}
 void OnChange(object? s,FileSystemEventArgs e)=>Schedule(WatcherSignal.Changed);void OnRename(object? s,RenamedEventArgs e)=>Schedule(WatcherSignal.Changed);void OnError(object? s,ErrorEventArgs e)=>Schedule(WatcherSignal.OverflowOrError);
 void Schedule(WatcherSignal signal){if(signal==WatcherSignal.OverflowOrError)pending=signal;debounce?.Cancel();debounce?.Dispose();var c=debounce=new();_ = Task.Run(async()=>{try{await Task.Delay(delay,c.Token);var emit=pending==WatcherSignal.OverflowOrError?WatcherSignal.OverflowOrError:signal;pending=default;Signaled?.Invoke(this,emit);}catch(OperationCanceledException){}});}
 void DisposeWatcher(){debounce?.Cancel();debounce?.Dispose();debounce=null;periodicCts?.Cancel();periodicCts?.Dispose();periodicCts=null;if(watcher is not null){watcher.EnableRaisingEvents=false;watcher.Dispose();watcher=null;}}
 public void Dispose()=>DisposeWatcher();
}
