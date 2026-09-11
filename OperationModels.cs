namespace SmartOrganizer.WinUI.Operations;
public enum OperationKind { Copy,Move }
public enum OperationState { Planned,Running,Committing,Completed,CompletedWithSourceRemaining,Failed,Cancelled,Undone,UndoFailed,RecoveryRequired }
public enum CancellationBoundary { Allowed,CommitStarted,NotApplicable }
public sealed record OperationPlan(Guid Id,OperationKind Kind,string SourcePath,string DestinationPath,long SourceLength,DateTime SourceModifiedAt,bool DestinationExists,string? BlockingReason);
public sealed record OperationProgress(Guid Id,string Stage,long BytesCompleted,long TotalBytes,string Message,CancellationBoundary CancellationBoundary=CancellationBoundary.Allowed);
public sealed record OperationRecord(Guid Id,OperationKind Kind,OperationState State,string SourcePath,string DestinationPath,long SourceLength,DateTime SourceModifiedAt,string? SourceHash,string? DestinationHash,DateTime CreatedAt,DateTime? CompletedAt,DateTime? UndoneAt,string? Error);
public sealed record OperationResult(bool Success,OperationRecord Record,string Message);
public sealed record RecoveryCandidate(OperationRecord Record,bool SourceExists,bool DestinationExists,bool PartialExists,bool? HashesMatch,string Recommendation);
