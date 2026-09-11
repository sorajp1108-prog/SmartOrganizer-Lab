namespace SmartOrganizer.WinUI.Operations;
public interface IOperationService
{
 Task<OperationPlan> PreviewAsync(OperationKind kind,string sourcePath,string destinationFolder,CancellationToken token);
 Task<OperationResult> ExecuteAsync(OperationPlan plan,IProgress<OperationProgress>? progress,CancellationToken token);
 Task<OperationResult> UndoAsync(Guid operationId,CancellationToken token);
 Task<IReadOnlyList<OperationRecord>> HistoryAsync(CancellationToken token);
 Task<IReadOnlyList<OperationRecord>> RecoveryCandidatesAsync(CancellationToken token);
 Task<IReadOnlyList<RecoveryCandidate>> InspectRecoveryAsync(CancellationToken token);
}
