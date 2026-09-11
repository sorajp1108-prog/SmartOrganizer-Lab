namespace SmartOrganizer.WinUI.Operations;
public sealed class BatchOperationService
{
 readonly IOperationService operations;
 public BatchOperationService(IOperationService operations)=>this.operations=operations;
 public async Task<BatchOperationPlan> PreviewAsync(OperationKind kind,IEnumerable<string> sources,string destinationFolder,NameConflictPolicy policy,CancellationToken token)
 {
  var reserved=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var items=new List<BatchPlanItem>();
  foreach(var source in sources.Distinct(StringComparer.OrdinalIgnoreCase)){token.ThrowIfCancellationRequested();var target=ChooseTarget(destinationFolder,Path.GetFileName(source),policy,reserved);if(target is null){items.Add(new(source,Path.Combine(destinationFolder,Path.GetFileName(source)),BatchItemState.Skipped,"同名ファイルがあるためスキップ",null));continue;}reserved.Add(target);var plan=await operations.PreviewAsync(kind,source,Path.GetDirectoryName(target)!,token);if(!string.Equals(plan.DestinationPath,target,StringComparison.OrdinalIgnoreCase))plan=plan with{DestinationPath=target,DestinationExists=File.Exists(target),BlockingReason=File.Exists(target)?"同名ファイルがあります":plan.BlockingReason};items.Add(new(source,target,plan.BlockingReason is null?BatchItemState.Ready:BatchItemState.Skipped,plan.BlockingReason,plan));}
  return new(Guid.NewGuid(),kind,policy,items);
 }
 public async Task<BatchOperationResult> ExecuteAsync(BatchOperationPlan plan,IProgress<OperationProgress>? progress,CancellationToken token)
 {
  var results=new List<BatchItemResult>();foreach(var item in plan.Items){if(item.State==BatchItemState.Skipped||item.OperationPlan is null){results.Add(new(item.SourcePath,item.DestinationPath,BatchItemState.Skipped,item.Reason??"スキップ",null));continue;}if(token.IsCancellationRequested){results.Add(new(item.SourcePath,item.DestinationPath,BatchItemState.Cancelled,"一括操作がキャンセルされました",null));continue;}var result=await operations.ExecuteAsync(item.OperationPlan,progress,token);var state=result.Record.State switch{OperationState.Completed=>BatchItemState.Completed,OperationState.Cancelled=>BatchItemState.Cancelled,OperationState.CompletedWithSourceRemaining=>BatchItemState.CompletedWithSourceRemaining,_=>BatchItemState.Failed};results.Add(new(item.SourcePath,item.DestinationPath,state,result.Message,result.Record.Id));if(state==BatchItemState.Cancelled)break;}return new(plan.Id,plan.Kind,results);
 }
 static string? ChooseTarget(string folder,string fileName,NameConflictPolicy policy,HashSet<string> reserved)
 {
  var target=Path.Combine(folder,fileName);if(!File.Exists(target)&&!reserved.Contains(target))return target;if(policy==NameConflictPolicy.Skip)return null;if(policy!=NameConflictPolicy.Rename)return null;var stem=Path.GetFileNameWithoutExtension(fileName);var ext=Path.GetExtension(fileName);for(var i=1;i<=9999;i++){target=Path.Combine(folder,$"{stem} ({i}){ext}");if(!File.Exists(target)&&!reserved.Contains(target))return target;}return null;
 }
}
