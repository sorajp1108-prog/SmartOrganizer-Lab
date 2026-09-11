using System.Security.Cryptography;using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class FileIdentityService
{
 const long HashLimit=64L*1024*1024;readonly WindowsFileIdentityProvider windows=new();
 public async Task<IdentityObservation> ObserveAsync(string path,bool includeHash,CancellationToken token){var identity=OperatingSystem.IsWindows()?windows.Get(path):Fallback(path,"Windows API unavailable");var f=new FileInfo(path);string? hash=null;if(includeHash&&f.Length<=HashLimit){try{await using var s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,1024*1024,true);hash=Convert.ToHexString(await SHA256.HashDataAsync(s,token));}catch(IOException){}catch(UnauthorizedAccessException){}}return new(path,identity,f.Length,f.LastWriteTimeUtc,hash);}
 static FileIdentity Fallback(string path,string reason){var full=Path.GetFullPath(path);return new($"PATH:{full.ToUpperInvariant()}",FileIdentityKind.PathFallback,null,null,"Unknown",full,false,reason);}
}
