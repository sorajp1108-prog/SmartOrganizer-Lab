using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class LazyOriginLoader
{
 readonly FileOriginService origins=new();
 public Task<FileRow> LoadAsync(FileRow row,CancellationToken token)=>Task.Run(()=>{token.ThrowIfCancellationRequested();if(row.PresentationKind!=EntryPresentationKind.SingleFile)return row;var o=origins.Detect(row.FullPath);return row with{Source=o.Label};},token);
}
