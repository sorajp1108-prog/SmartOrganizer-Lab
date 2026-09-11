using System.Diagnostics;using System.Text.RegularExpressions;using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class CleanupCandidateEngine
{
 static readonly HashSet<string> TempExt=new(StringComparer.OrdinalIgnoreCase){".tmp",".temp",".bak",".old",".dmp"};
 static readonly HashSet<string> PartialExt=new(StringComparer.OrdinalIgnoreCase){".part",".partial",".crdownload",".download",".opdownload"};
 static readonly HashSet<string> LogExt=new(StringComparer.OrdinalIgnoreCase){".log",".trace",".etl"};
 static readonly HashSet<string> InstallerExt=new(StringComparer.OrdinalIgnoreCase){".msi",".msix",".msixbundle",".appx",".appxbundle"};
 static readonly HashSet<string> OrphanExt=new(StringComparer.OrdinalIgnoreCase){".lnk",".url"};
 static readonly Regex VersionName=new(@"^(?<base>.+?)[-_ .]?(?:v|ver|version)?(?<version>\d+(?:[._-]\d+){1,3})(?<tail>[^\\/]*)$",RegexOptions.IgnoreCase|RegexOptions.Compiled);
 public IReadOnlyDictionary<string,CleanupCandidate> Analyze(IEnumerable<FileRow> rows,IReadOnlyList<StructureBlock> blocks,DateTime utcNow)
 {
  var standalone=rows.Where(x=>x.PresentationKind==EntryPresentationKind.SingleFile&&x.IsIndividuallyOperable&&x.BlockId is null).ToArray();var result=new Dictionary<string,CleanupCandidate>(StringComparer.OrdinalIgnoreCase);foreach(var f in standalone){var age=utcNow-f.ModifiedAt.ToUniversalTime();var ext=Path.GetExtension(f.FullPath);var reasons=new List<CleanupReason>();CleanupCandidate? c=null;
   if(PartialExt.Contains(ext)&&age>=TimeSpan.FromHours(24)){reasons.Add(new("INCOMPLETE_EXTENSION","未完了ダウンロードで使われる拡張子です",50));reasons.Add(new("STALE_24H","24時間以上更新されていません",30));if(!IsLocked(f.FullPath))reasons.Add(new("NOT_LOCKED","現在使用中ではありません",20));c=Make(f,CleanupCandidateKind.IncompleteDownload,reasons,"DOWNLOAD");}
   else if(TempExt.Contains(ext)&&age>=TimeSpan.FromDays(7)){reasons.Add(new("TEMP_EXTENSION","一時・退避用途で使われる拡張子です",45));reasons.Add(new("STALE_7D","7日以上更新されていません",30));if(!IsLocked(f.FullPath))reasons.Add(new("NOT_LOCKED","現在使用中ではありません",20));c=Make(f,CleanupCandidateKind.TemporaryFile,reasons,"TEMP");}
   else if((LogExt.Contains(ext)||PathParts(f.FullPath).Any(x=>x.Equals("cache",StringComparison.OrdinalIgnoreCase)||x.Equals("logs",StringComparison.OrdinalIgnoreCase)))&&age>=TimeSpan.FromDays(30)){reasons.Add(new("LOG_OR_CACHE","ログまたはキャッシュらしい場所・拡張子です",45));reasons.Add(new("STALE_30D","30日以上更新されていません",35));c=Make(f,CleanupCandidateKind.LogOrCache,reasons,"LOGCACHE");}
   else if((InstallerExt.Contains(ext)||ext.Equals(".exe",StringComparison.OrdinalIgnoreCase))&&age>=TimeSpan.FromDays(30)&&LooksLikeInstaller(f.FullPath)){reasons.Add(new("INSTALLER_METADATA","名前または製品情報がインストーラーを示します",55));reasons.Add(new("STALE_30D","30日以上更新されていません",25));c=Make(f,CleanupCandidateKind.Installer,reasons,"INSTALLER");}
   else if(OrphanExt.Contains(ext)&&age>=TimeSpan.FromDays(30)&&IsBrokenLinkNameOnly(f.FullPath)){reasons.Add(new("LINK_TARGET_UNRESOLVED","参照先を確認できないリンク候補です",45));reasons.Add(new("STALE_30D","30日以上更新されていません",0));c=Make(f,CleanupCandidateKind.OrphanFile,reasons,"ORPHAN");}
   if(c is not null)result[f.FullPath]=c;
  }
  foreach(var group in standalone.Select(f=>(f,match:VersionName.Match(Path.GetFileNameWithoutExtension(f.Name)))).Where(x=>x.match.Success).GroupBy(x=>(x.match.Groups["base"].Value.ToUpperInvariant(),x.f.Extension.ToUpperInvariant())).Where(g=>g.Count()>1)){var ordered=group.OrderByDescending(x=>ParseVersion(x.match.Groups["version"].Value)).ThenByDescending(x=>x.f.ModifiedAt).ToArray();foreach(var old in ordered.Skip(1)){if(result.ContainsKey(old.f.FullPath))continue;var reasons=new[]{new CleanupReason("VERSION_SIBLINGS","同じ名前系列の新しい版候補があります",45),new CleanupReason("OLDER_VERSION","版番号または更新日時が古い候補です",25)};result[old.f.FullPath]=Make(old.f,CleanupCandidateKind.OldVersion,reasons,"VERSION:"+group.Key.Item1);}}
  return result;
 }
 static CleanupCandidate Make(FileRow f,CleanupCandidateKind kind,IEnumerable<CleanupReason> source,string group){var reasons=source.ToArray();var score=reasons.Sum(x=>x.Weight);var confidence=score>=80?CleanupConfidence.High:score>=55?CleanupConfidence.Medium:CleanupConfidence.Low;return new(f.FullPath,kind,confidence,reasons,group,true);}
 static bool IsLocked(string path){try{using var s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None);return false;}catch{return true;}}
 static bool LooksLikeInstaller(string path){var n=Path.GetFileNameWithoutExtension(path);if(n.Contains("setup",StringComparison.OrdinalIgnoreCase)||n.Contains("installer",StringComparison.OrdinalIgnoreCase)||n.Contains("install",StringComparison.OrdinalIgnoreCase))return true;try{var v=FileVersionInfo.GetVersionInfo(path);return (v.FileDescription?.Contains("install",StringComparison.OrdinalIgnoreCase)??false)||(v.ProductName?.Contains("setup",StringComparison.OrdinalIgnoreCase)??false);}catch{return false;}}
 static bool IsBrokenLinkNameOnly(string path)=>Path.GetExtension(path).Equals(".lnk",StringComparison.OrdinalIgnoreCase)&&new FileInfo(path).Length<1024*1024;
 static IEnumerable<string> PathParts(string p)=>p.Split(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);static Version ParseVersion(string v){var nums=v.Split('.','_','-').Select(x=>int.TryParse(x,out var n)?n:0).Take(4).ToArray();Array.Resize(ref nums,4);return new(nums[0],nums[1],nums[2],nums[3]);}
}
