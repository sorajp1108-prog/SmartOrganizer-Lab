namespace SmartOrganizer.WinUI.Operations;
public enum NameConflictPolicy { Skip,Rename }
public enum BatchItemState { Ready,Skipped,Completed,Failed,Cancelled,CompletedWithSourceRemaining }
public sealed record BatchPlanItem(string SourcePath,string DestinationPath,BatchItemState State,string? Reason,OperationPlan? OperationPlan);
public sealed record BatchOperationPlan(Guid Id,OperationKind Kind,NameConflictPolicy ConflictPolicy,IReadOnlyList<BatchPlanItem> Items);
public sealed record BatchItemResult(string SourcePath,string DestinationPath,BatchItemState State,string Message,Guid? OperationId);
public sealed record BatchOperationResult(Guid Id,OperationKind Kind,IReadOnlyList<BatchItemResult> Items)
{
 public int Completed=>Items.Count(x=>x.State==BatchItemState.Completed);
 public int Skipped=>Items.Count(x=>x.State==BatchItemState.Skipped);
 public int Failed=>Items.Count(x=>x.State is BatchItemState.Failed or BatchItemState.CompletedWithSourceRemaining);
 public int Cancelled=>Items.Count(x=>x.State==BatchItemState.Cancelled);
}
