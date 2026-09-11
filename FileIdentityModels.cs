namespace SmartOrganizer.WinUI.Models;
public enum FileIdentityKind { WindowsFileId,PathFallback,ContentHashLink }
public sealed record FileIdentity(string StableKey,FileIdentityKind Kind,string? VolumeId,string? FileId,string FileSystem,string CurrentPath,bool IsReliable,string? FailureReason);
public sealed record IdentityObservation(string Path,FileIdentity Identity,long Length,DateTime LastWriteUtc,string? ContentHash);
public sealed record FirstSeenResolution(DateTime FirstSeenAt,string StableKey,FileIdentityKind IdentityKind,string? PreviousPath,bool WasRenamedOrMoved,bool WasCrossVolumeLinked);
public sealed record IdentityLedgerEntry(string StableKey,DateTime FirstSeenAt,string CurrentPath,string? PreviousPath,string? VolumeId,string? FileId,string FileSystem,long Length,DateTime LastWriteUtc,string? ContentHash,DateTime LastObservedAt);
