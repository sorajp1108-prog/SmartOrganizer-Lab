namespace SmartOrganizer.WinUI.Models;
public enum StructureBlockKind { PortableApplication,DevelopmentProject,UnknownBundle,SystemArea }
public enum ProtectionState { None,BlockProtected,SystemProtected,ReviewRequired }
public enum DetectionConfidence { Confirmed,High,Medium,Low,Unknown }
public enum EntryPresentationKind { SingleFile,StructureBlock,SystemArea,UnknownBlock }
public enum FileOperationMode { Standard,Advanced }
public enum ProtectedOperationConfirmation { Always,OncePerBlock,Never }
public sealed record DetectionReason(string Code,string Description,int Weight);
public sealed record FileEntry(string FullPath,string Name,string Extension,long Length,DateTime CreatedAt,DateTime ModifiedAt,FileAttributes Attributes,string RelativeFolder);
public sealed record StructureBlock(string Id,string RootPath,string DisplayName,StructureBlockKind Kind,ProtectionState Protection,DetectionConfidence Confidence,IReadOnlyList<DetectionReason> Reasons,IReadOnlyList<FileEntry> Members,long TotalLength,DateTime ModifiedAt);
public sealed record StructureAnalysis(IReadOnlyList<FileEntry> StandaloneFiles,IReadOnlyList<StructureBlock> Blocks);
public sealed record ProtectionDecision(bool Allowed,ProtectionState State,string Message,string? BlockRoot,string? BlockId,StructureBlockKind? BlockKind);
public sealed record ProtectedOperationRequest(string SourcePath,string BlockId,string BlockRoot,StructureBlockKind BlockKind,string Message);
