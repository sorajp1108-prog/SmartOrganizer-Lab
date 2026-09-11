namespace SmartOrganizer.WinUI.Services;
public sealed record EnumerationReport(IReadOnlyList<string> Files,IReadOnlyList<string> SkippedFolders,int OfflineFilesSkipped);
public sealed class SafeFileEnumerator
{
 const FileAttributes RecallOnDataAccess=(FileAttributes)0x00400000;const int MaxDepth=64;
 public EnumerationReport Enumerate(string root,bool includeSubfolders,CancellationToken token)
 {
  var files=new List<string>();var skipped=new List<string>();var offline=0;var rootFull=Path.GetFullPath(root);var pending=new Stack<(string Path,int Depth)>();var visited=new HashSet<string>(StringComparer.OrdinalIgnoreCase);pending.Push((rootFull,0));
  while(pending.Count>0){token.ThrowIfCancellationRequested();var item=pending.Pop();if(item.Depth>MaxDepth){skipped.Add(item.Path+" [最大階層超過]");continue;}string canonical;try{canonical=Path.GetFullPath(item.Path);}catch{skipped.Add(item.Path+" [無効なパス]");continue;}if(!visited.Add(canonical))continue;
   try
   {
    var rootInfo=new DirectoryInfo(canonical);if((rootInfo.Attributes&FileAttributes.ReparsePoint)!=0&&item.Depth>0){skipped.Add(canonical+" [ReparsePoint]");continue;}
    foreach(var entry in rootInfo.EnumerateFileSystemInfos("*",new EnumerationOptions{RecurseSubdirectories=false,IgnoreInaccessible=true,ReturnSpecialDirectories=false,AttributesToSkip=FileAttributes.None}))
    {token.ThrowIfCancellationRequested();try{var attrs=entry.Attributes;if((attrs&FileAttributes.Directory)!=0){if(includeSubfolders){if((attrs&FileAttributes.ReparsePoint)!=0)skipped.Add(entry.FullName+" [ReparsePoint]");else pending.Push((entry.FullName,item.Depth+1));}continue;}if((attrs&(FileAttributes.Offline|RecallOnDataAccess))!=0){offline++;continue;}files.Add(entry.FullName);}catch(UnauthorizedAccessException){skipped.Add(entry.FullName+" [アクセス拒否]");}catch(IOException){skipped.Add(entry.FullName+" [I/Oエラー]");}}
   }catch(UnauthorizedAccessException){skipped.Add(canonical+" [アクセス拒否]");}catch(IOException){skipped.Add(canonical+" [I/Oエラー]");}
  }
  return new(files,skipped,offline);
 }
}
