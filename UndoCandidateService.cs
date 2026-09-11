namespace SmartOrganizer.WinUI.Operations;
public sealed class UndoCandidateService
{
 readonly IOperationService operations;public UndoCandidateService(IOperationService operations)=>this.operations=operations;
 public async Task<OperationRecord?> LatestAsync(CancellationToken token)=>(await operations.HistoryAsync(token)).Where(x=>x.State==OperationState.Completed).OrderByDescending(x=>x.CompletedAt).FirstOrDefault();
 public static string Describe(OperationRecord r)=>$"{r.Kind}: {Path.GetFileName(r.DestinationPath)}\n実行日時: {r.CompletedAt?.ToLocalTime():g}\n元: {r.SourcePath}\n先: {r.DestinationPath}";
}
