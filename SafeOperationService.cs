using System.Security.Cryptography;
namespace SmartOrganizer.WinUI.Operations;
public sealed class SafeOperationService:IOperationService
{
 readonly OperationJournal journal=new();readonly SemaphoreSlim operationGate=new(1,1);
 public Task<OperationPlan> PreviewAsync(OperationKind kind,string sourcePath,string destinationFolder,CancellationToken token)=>Task.Run(()=>Preview(kind,sourcePath,destinationFolder,token),token);
 static OperationPlan Preview(OperationKind kind,string sourcePath,string destinationFolder,CancellationToken token)
 {
  token.ThrowIfCancellationRequested();var source=Path.GetFullPath(sourcePath);var folder=Path.GetFullPath(destinationFolder);
  string? block=null;if(!File.Exists(source))block="元ファイルがありません";else if(!Directory.Exists(folder))block="保存先フォルダーがありません";
  var destination=Path.Combine(folder,Path.GetFileName(source));if(string.Equals(source,destination,StringComparison.OrdinalIgnoreCase))block="元ファイルと保存先が同じです";
  var exists=File.Exists(destination);if(exists)block="同名ファイルが既にあります。上書きは行いません";
  var f=File.Exists(source)?new FileInfo(source):null;return new(Guid.NewGuid(),kind,source,destination,f?.Length??0,f?.LastWriteTimeUtc??DateTime.MinValue,exists,block);
 }
 public async Task<OperationResult> ExecuteAsync(OperationPlan plan,IProgress<OperationProgress>? progress,CancellationToken token)
 {
  await operationGate.WaitAsync(token);
  try{return await ExecuteCoreAsync(plan,progress,token);}
  finally{operationGate.Release();}
 }
 async Task<OperationResult> ExecuteCoreAsync(OperationPlan plan,IProgress<OperationProgress>? progress,CancellationToken token)
 {
  if(plan.BlockingReason is not null)throw new InvalidOperationException(plan.BlockingReason);var running=Record(plan,OperationState.Running);await journal.UpsertAsync(running,token);var stage=plan.DestinationPath+".smartorganizer-"+plan.Id.ToString("N")+".partial";var commitStarted=false;
  try
  {
   Revalidate(plan);Directory.CreateDirectory(Path.GetDirectoryName(plan.DestinationPath)!);var sourceHash=await CopyVerifiedAsync(plan.SourcePath,stage,plan.Id,progress,token);token.ThrowIfCancellationRequested();
   if(File.Exists(plan.DestinationPath))throw new IOException("実行中に同名ファイルが作成されました");progress?.Report(new(plan.Id,"COMMITTING",plan.SourceLength,plan.SourceLength,"正式ファイルへ確定しています",CancellationBoundary.CommitStarted));var committing=running with{State=OperationState.Committing,SourceHash=Convert.ToHexString(sourceHash)};await journal.UpsertAsync(committing,CancellationToken.None);commitStarted=true;File.Move(stage,plan.DestinationPath,false);var destinationHash=await HashAsync(plan.DestinationPath,CancellationToken.None);if(!CryptographicOperations.FixedTimeEquals(sourceHash,destinationHash))throw new IOException("公開後の検証に失敗しました");
   if(plan.Kind==OperationKind.Move){RevalidateSourceBeforeRemoval(plan,sourceHash);try{File.Delete(plan.SourcePath);}catch(Exception e){var remaining=running with{State=OperationState.CompletedWithSourceRemaining,SourceHash=Convert.ToHexString(sourceHash),DestinationHash=Convert.ToHexString(destinationHash),CompletedAt=DateTime.UtcNow,Error=e.Message};await journal.UpsertAsync(remaining,CancellationToken.None);return new(false,remaining,"保存先は完成しましたが、元ファイルを削除できませんでした。両方のファイルが残っています");}}
   var done=running with{State=OperationState.Completed,SourceHash=Convert.ToHexString(sourceHash),DestinationHash=Convert.ToHexString(destinationHash),CompletedAt=DateTime.UtcNow};await journal.UpsertAsync(done,CancellationToken.None);return new(true,done,"完了しました");
  }
  catch(OperationCanceledException){TryDeleteStage(stage);var cancelled=running with{State=OperationState.Cancelled,Error="利用者がキャンセルしました"};await journal.UpsertAsync(cancelled,CancellationToken.None);return new(false,cancelled,"キャンセルしました");}
  catch(Exception e){TryDeleteStage(stage);var failed=running with{State=commitStarted?OperationState.RecoveryRequired:OperationState.Failed,Error=e.Message};await journal.UpsertAsync(failed,CancellationToken.None);return new(false,failed,e.Message);}
 }
 public async Task<OperationResult> UndoAsync(Guid id,CancellationToken token)
 {
  await operationGate.WaitAsync(token);
  try{return await UndoCoreAsync(id,token);}
  finally{operationGate.Release();}
 }
 async Task<OperationResult> UndoCoreAsync(Guid id,CancellationToken token)
 {
  var record=(await journal.ReadAsync(token)).SingleOrDefault(x=>x.Id==id)??throw new InvalidOperationException("履歴がありません");if(record.State!=OperationState.Completed)throw new InvalidOperationException("Undoできる完了操作ではありません");
  try
  {
   if(!File.Exists(record.DestinationPath))throw new FileNotFoundException("保存先ファイルがありません");var current=Convert.ToHexString(await HashAsync(record.DestinationPath,token));if(!string.Equals(current,record.DestinationHash,StringComparison.OrdinalIgnoreCase))throw new IOException("保存先が変更されているためUndoしません");
   if(record.Kind==OperationKind.Copy)File.Delete(record.DestinationPath);else{if(File.Exists(record.SourcePath))throw new IOException("元の場所に同名ファイルがあるためUndoしません");File.Move(record.DestinationPath,record.SourcePath,false);}
   var undone=record with{State=OperationState.Undone,UndoneAt=DateTime.UtcNow};await journal.UpsertAsync(undone,CancellationToken.None);return new(true,undone,"Undoしました");
  }catch(Exception e){var failed=record with{State=OperationState.UndoFailed,Error=e.Message};await journal.UpsertAsync(failed,CancellationToken.None);return new(false,failed,e.Message);}
 }
 public async Task<IReadOnlyList<OperationRecord>> HistoryAsync(CancellationToken token)=>(await journal.ReadAsync(token)).OrderByDescending(x=>x.CreatedAt).ToArray();
 public async Task<IReadOnlyList<OperationRecord>> RecoveryCandidatesAsync(CancellationToken token)=>(await journal.ReadAsync(token)).Where(x=>x.State is OperationState.Running or OperationState.Committing or OperationState.RecoveryRequired).ToArray();
 public async Task<IReadOnlyList<RecoveryCandidate>> InspectRecoveryAsync(CancellationToken token){var records=await RecoveryCandidatesAsync(token);var list=new List<RecoveryCandidate>();foreach(var r in records){token.ThrowIfCancellationRequested();var partial=Directory.Exists(Path.GetDirectoryName(r.DestinationPath))&&Directory.EnumerateFiles(Path.GetDirectoryName(r.DestinationPath)!,Path.GetFileName(r.DestinationPath)+".smartorganizer-*.partial").Any();var src=File.Exists(r.SourcePath);var dst=File.Exists(r.DestinationPath);bool? same=null;if(src&&dst)try{same=CryptographicOperations.FixedTimeEquals(await HashAsync(r.SourcePath,token),await HashAsync(r.DestinationPath,token));}catch(IOException){}var recommendation=partial&&!dst?"一時ファイルを削除する候補":src&&dst&&same==true?"両方が一致しています。コピー完了として確定するか、移動を完了するか確認してください":!src&&dst?"保存先は存在します。操作完了として確定する候補":"自動変更せず詳細確認が必要";list.Add(new(r,src,dst,partial,same,recommendation));}return list;}

 static void TryDeleteStage(string stage)
 {
  try{if(File.Exists(stage))File.Delete(stage);}
  catch(Exception e) when(e is IOException or UnauthorizedAccessException){/* 復旧検査へ残す */}
 }
 static OperationRecord Record(OperationPlan p,OperationState state)=>new(p.Id,p.Kind,state,p.SourcePath,p.DestinationPath,p.SourceLength,p.SourceModifiedAt,null,null,DateTime.UtcNow,null,null,null);
 static void Revalidate(OperationPlan p){var f=new FileInfo(p.SourcePath);if(!f.Exists||f.Length!=p.SourceLength||f.LastWriteTimeUtc!=p.SourceModifiedAt)throw new IOException("プレビュー後に元ファイルが変更されました");if(File.Exists(p.DestinationPath))throw new IOException("同名ファイルが存在します");}
 static void RevalidateSourceBeforeRemoval(OperationPlan p,byte[] expected){var f=new FileInfo(p.SourcePath);if(!f.Exists||f.Length!=p.SourceLength||f.LastWriteTimeUtc!=p.SourceModifiedAt)throw new IOException("コピー中に元ファイルが変更されたため移動を中止しました");using var s=File.OpenRead(p.SourcePath);var actual=SHA256.HashData(s);if(!CryptographicOperations.FixedTimeEquals(expected,actual))throw new IOException("元ファイルの再検証に失敗しました");}
 static async Task<byte[]> CopyVerifiedAsync(string source,string stage,Guid id,IProgress<OperationProgress>? progress,CancellationToken token){using var sha=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);await using var input=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.Read,1024*1024,FileOptions.Asynchronous|FileOptions.SequentialScan);await using var output=new FileStream(stage,FileMode.CreateNew,FileAccess.Write,FileShare.None,1024*1024,FileOptions.Asynchronous|FileOptions.WriteThrough);var buffer=new byte[1024*1024];long done=0;int n;while((n=await input.ReadAsync(buffer,token))>0){await output.WriteAsync(buffer.AsMemory(0,n),token);sha.AppendData(buffer,0,n);done+=n;progress?.Report(new(id,"COPYING",done,input.Length,$"{done:N0} / {input.Length:N0} bytes",CancellationBoundary.Allowed));}await output.FlushAsync(token);return sha.GetHashAndReset();}
 static async Task<byte[]> HashAsync(string path,CancellationToken token){await using var s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,1024*1024,FileOptions.Asynchronous|FileOptions.SequentialScan);using var sha=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);var b=new byte[1024*1024];int n;while((n=await s.ReadAsync(b,token))>0)sha.AppendData(b,0,n);return sha.GetHashAndReset();}
}
