using System.Text;
namespace SmartOrganizer.WinUI.Services;
public sealed record FileOrigin(string Label,int? ZoneId,string? HostUrl,string? ReferrerUrl,bool IsConfirmed);
public sealed class FileOriginService
{
 public FileOrigin Detect(string path)
 {
  try
  {
   var lines=File.ReadAllLines(path+":Zone.Identifier",Encoding.UTF8);int? zone=null;string? host=null,referrer=null;
   foreach(var raw in lines){var line=raw.Trim();if(line.StartsWith("ZoneId=",StringComparison.OrdinalIgnoreCase)&&int.TryParse(line[7..],out var z))zone=z;else if(line.StartsWith("HostUrl=",StringComparison.OrdinalIgnoreCase))host=line[8..];else if(line.StartsWith("ReferrerUrl=",StringComparison.OrdinalIgnoreCase))referrer=line[12..];}
   var uri=TryUri(host)??TryUri(referrer);var label=BrowserOrHost(uri?.Host,zone);return new(label,zone,host,referrer,true);
  }catch(FileNotFoundException){return new("ローカルまたは取得元不明",null,null,null,false);}catch(IOException){return new("取得元を読み取れません",null,null,null,false);}catch(UnauthorizedAccessException){return new("取得元を読み取れません",null,null,null,false);}
 }
 static Uri? TryUri(string? value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)?uri:null;
 static string BrowserOrHost(string? host,int? zone)
 {
  if(string.IsNullOrWhiteSpace(host))return zone is 3 or 4?"Webから取得":"ローカルまたは取得元不明";
  host=host.ToLowerInvariant();if(host.Contains("microsoft.com")||host.Contains("office.com")||host.Contains("sharepoint.com"))return "Microsoftサービス";if(host.Contains("google.com")||host.Contains("googleusercontent.com"))return "Googleサービス";if(host.Contains("dropbox.com"))return "Dropbox";if(host.Contains("github.com")||host.Contains("githubusercontent.com"))return "GitHub";return host;
 }
}
