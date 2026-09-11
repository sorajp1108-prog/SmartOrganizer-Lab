using System.Security.Cryptography;using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class DuplicateCandidateService
{
 const FileAttributes RecallOnDataAccess=(FileAttributes)0x00400000;sealed record Snapshot(long Length,DateTime LastWriteUtc);
 public async Task<IReadOnlyDictionary<string,string>> FindAsync(IEnumerable<FileRow> files,IProgress<CatalogProgress>? progress,CancellationToken token)
 {
  var candidates=files.Where(x=>x.IsIndividuallyOperable&&x.PresentationKind==EntryPresentationKind.SingleFile).Where(IsLocalContent).Where(x=>x.Length>0).GroupBy(x=>x.Length).Where(g=>g.Count()>1).SelectMany(x=>x).ToArray();var result=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);var quick=new Dictionary<(long,string),List<FileRow>>();var done=0;
  foreach(var f in candidates){token.ThrowIfCancellationRequested();var before=TakeSnapshot(f.FullPath);var h=await QuickHashAsync(f.FullPath,token);if(Unchanged(f.FullPath,before)){var key=(f.Length,h);if(!quick.TryGetValue(key,out var list))quick[key]=list=[];list.Add(f);}progress?.Report(new("QUICK_HASH",++done,candidates.Length,$"簡易検査 {done}/{candidates.Length}"));}
  var fullCandidates=quick.Values.Where(x=>x.Count>1).SelectMany(x=>x).ToArray();done=0;var full=new Dictionary<(long,string),List<FileRow>>();foreach(var f in fullCandidates){token.ThrowIfCancellationRequested();var before=TakeSnapshot(f.FullPath);var h=await FullHashAsync(f.FullPath,token);if(Unchanged(f.FullPath,before)){var key=(f.Length,h);if(!full.TryGetValue(key,out var list))full[key]=list=[];list.Add(f);}progress?.Report(new("FULL_HASH",++done,fullCandidates.Length,$"全体検査 {done}/{fullCandidates.Length}"));}
  foreach(var pair in full.Where(x=>x.Value.Count>1)){var id=$"{pair.Key.Item1}:{pair.Key.Item2}";foreach(var f in pair.Value)result[f.FullPath]=id;}return result;
 }
 static bool IsLocalContent(FileRow f){try{var a=File.GetAttributes(f.FullPath);return (a&(FileAttributes.Offline|RecallOnDataAccess))==0;}catch{return false;}}static Snapshot TakeSnapshot(string p){var f=new FileInfo(p);return new(f.Length,f.LastWriteTimeUtc);}static bool Unchanged(string p,Snapshot s){try{var f=new FileInfo(p);return f.Exists&&f.Length==s.Length&&f.LastWriteTimeUtc==s.LastWriteUtc;}catch{return false;}}
 static async Task<string> QuickHashAsync(string path,CancellationToken token){await using var s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,65536,true);using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);var b=new byte[65536];var n=await s.ReadAsync(b,token);h.AppendData(b,0,n);if(s.Length>65536){s.Seek(Math.Max(0,s.Length-65536),SeekOrigin.Begin);n=await s.ReadAsync(b,token);h.AppendData(b,0,n);}return Convert.ToHexString(h.GetHashAndReset());}
 static async Task<string> FullHashAsync(string path,CancellationToken token){await using var s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,1024*1024,true);using var sha=SHA256.Create();return Convert.ToHexString(await sha.ComputeHashAsync(s,token));}
}
