using System.Diagnostics;using System.Text.Json;using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class ChangeTrackingCoordinator
{
 readonly UsnJournalReader usn=new();readonly ChangeTrackingCheckpointStore checkpoints=new();readonly string metricsPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SmartOrganizer","change-tracking-metrics.jsonl");
 public TimeSpan PeriodicFullScanInterval{get;}=TimeSpan.FromMinutes(30);
 public async Task<DeltaScanResult> CheckAsync(string root,CancellationToken token)
 {
  var sw=Stopwatch.StartNew();var snapshot=usn.Probe(root);if(!snapshot.Supported)return await Finish(new(ChangeTrackingMode.WatcherWithPeriodicScan,DeltaRefreshDisposition.FullRescan,[],snapshot.UnavailableReason??"USNを利用できないため定期再走査",sw.ElapsedMilliseconds,false),token);
  var saved=await checkpoints.LoadAsync(snapshot.VolumeRoot,token);var disposition=JournalContinuityEvaluator.Evaluate(saved,snapshot,out var continuityReason);if(disposition==DeltaRefreshDisposition.FullRescan){await checkpoints.SaveAsync(new(snapshot.VolumeRoot,snapshot.JournalId,snapshot.NextUsn,snapshot.FileSystem,DateTime.UtcNow),token);return await Finish(new(ChangeTrackingMode.UsnJournal,DeltaRefreshDisposition.FullRescan,[],continuityReason+"のため全件再走査",sw.ElapsedMilliseconds,false),token);}if(disposition==DeltaRefreshDisposition.NoChanges)return await Finish(new(ChangeTrackingMode.UsnJournal,DeltaRefreshDisposition.NoChanges,[],"変更なし",sw.ElapsedMilliseconds,false),token);
  try{var changes=await Task.Run(()=>{var c=usn.Read(snapshot.VolumeRoot,snapshot.JournalId,saved.NextUsn,out var next);return (c,next);},token);await checkpoints.SaveAsync(new(snapshot.VolumeRoot,snapshot.JournalId,changes.next,snapshot.FileSystem,DateTime.UtcNow),token);return await Finish(new(ChangeTrackingMode.UsnJournal,changes.c.Count==0?DeltaRefreshDisposition.NoChanges:DeltaRefreshDisposition.IncrementalRefresh,changes.c,"USN差分を取得",sw.ElapsedMilliseconds,false),token);}catch(Exception e){return await Finish(new(ChangeTrackingMode.WatcherWithPeriodicScan,DeltaRefreshDisposition.FullRescan,[],"USN読み取り失敗のため安全に全件再走査: "+e.Message,sw.ElapsedMilliseconds,false),token);}
 }
 async Task<DeltaScanResult> Finish(DeltaScanResult result,CancellationToken token){try{Directory.CreateDirectory(Path.GetDirectoryName(metricsPath)!);var metric=new ChangeTrackingMetric(DateTime.UtcNow,result.Mode.ToString(),result.ElapsedMilliseconds,result.Changes.Count,result.Reason);await File.AppendAllTextAsync(metricsPath,JsonSerializer.Serialize(metric)+Environment.NewLine,token);}catch{}return result;}
}
