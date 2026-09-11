namespace SmartOrganizer.WinUI.Models;
public enum ChangeTrackingMode { UsnJournal,WatcherWithPeriodicScan }
public enum DeltaRefreshDisposition { NoChanges,IncrementalRefresh,FullRescan }
public sealed record UsnJournalSnapshot(string VolumeRoot,ulong JournalId,long FirstUsn,long NextUsn,long LowestValidUsn,string FileSystem,bool Supported,string? UnavailableReason);
public sealed record UsnChange(ulong FileReferenceNumber,ulong ParentFileReferenceNumber,long Usn,uint Reason,string Name);
public sealed record ChangeTrackingCheckpoint(string VolumeRoot,ulong JournalId,long NextUsn,string FileSystem,DateTime SavedAtUtc);
public sealed record DeltaScanResult(ChangeTrackingMode Mode,DeltaRefreshDisposition Disposition,IReadOnlyList<UsnChange> Changes,string Reason,long ElapsedMilliseconds,bool UsedAdministratorPrivileges=false);
public sealed record ChangeTrackingMetric(DateTime AtUtc,string Operation,long ElapsedMilliseconds,int ChangeCount,string Result);
