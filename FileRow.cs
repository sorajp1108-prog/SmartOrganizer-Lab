namespace SmartOrganizer.WinUI.Models;
public sealed record FileRow(string Name,string Kind,string Source,string Modified,string Size,string Extension,string FullPath,long Length,DateTime ModifiedAt,DateTime CreatedAt,string RelativeFolder,string? DuplicateGroup=null,DateTime? FirstSeenAt=null,bool OriginLoaded=false,EntryPresentationKind PresentationKind=EntryPresentationKind.SingleFile,string? BlockId=null,ProtectionState Protection=ProtectionState.None,DetectionConfidence Confidence=DetectionConfidence.Unknown,string? ProtectionReason=null,bool IsIndividuallyOperable=true,CleanupCandidateKind? CleanupKind=null,CleanupConfidence? CleanupConfidence=null,string? CleanupReason=null,bool IsCleanupSuggestion=false);
public sealed record CategoryRow(string Id,string Label,string Glyph,int Count);
public sealed record QuickAccessRow(string Id,string Label,string Glyph,string? Path,string ViewMode);
public sealed record FilterSnapshot(IReadOnlyList<string> Kinds,IReadOnlyList<string> Sources,IReadOnlyList<string> Extensions,IReadOnlyList<string> Subfolders);
public sealed record QueryOptions(string Category,string Search,string? Kind,string? Source,string? Extension,SortMode Sort,bool IncludeSubfolders,string? RelativeFolder,string SmartView,long LargeFileThresholdBytes,int RecentDays,bool FindDuplicates,IProgress<CatalogProgress>? Progress=null);
public enum SortMode { Newest,Oldest,NameAscending,NameDescending,SizeDescending,SizeAscending }
public sealed record QueryResult(IReadOnlyList<FileRow> Files,IReadOnlyList<CategoryRow> Categories,FilterSnapshot Filters,int TotalCount);
public sealed record HistoryRow(DateTime At,string Operation,string State,string Target);

public sealed record DuplicateGroupRow(string GroupId,long Length,IReadOnlyList<FileRow> Files);
public sealed record CatalogProgress(string Stage,int Processed,int Total,string Message);
