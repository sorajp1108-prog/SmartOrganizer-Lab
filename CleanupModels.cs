namespace SmartOrganizer.WinUI.Models;
public enum CleanupCandidateKind { TemporaryFile,IncompleteDownload,LogOrCache,OldVersion,OrphanFile,Installer }
public enum CleanupConfidence { High,Medium,Low }
public sealed record CleanupReason(string Code,string Description,int Weight);
public sealed record CleanupCandidate(string FullPath,CleanupCandidateKind Kind,CleanupConfidence Confidence,IReadOnlyList<CleanupReason> Reasons,string GroupKey,bool ReadOnlySuggestion=true);
