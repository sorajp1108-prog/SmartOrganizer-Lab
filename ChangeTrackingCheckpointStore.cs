using System.Text.Json;using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class ChangeTrackingCheckpointStore
{
 readonly string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SmartOrganizer","change-tracking-v1.json");readonly SemaphoreSlim gate=new(1,1);
 public async Task<ChangeTrackingCheckpoint?> LoadAsync(string root,CancellationToken token){await gate.WaitAsync(token);try{if(!File.Exists(path))return null;await using var s=File.OpenRead(path);var all=await JsonSerializer.DeserializeAsync<Dictionary<string,ChangeTrackingCheckpoint>>(s,cancellationToken:token);return all is not null&&all.TryGetValue(Key(root),out var c)?c:null;}catch{return null;}finally{gate.Release();}}
 public async Task SaveAsync(ChangeTrackingCheckpoint value,CancellationToken token){await gate.WaitAsync(token);try{Dictionary<string,ChangeTrackingCheckpoint> all=[];if(File.Exists(path))try{all=JsonSerializer.Deserialize<Dictionary<string,ChangeTrackingCheckpoint>>(await File.ReadAllTextAsync(path,token))??[];}catch{}all[Key(value.VolumeRoot)]=value;Directory.CreateDirectory(Path.GetDirectoryName(path)!);var tmp=path+".tmp";await File.WriteAllTextAsync(tmp,JsonSerializer.Serialize(all),token);File.Move(tmp,path,true);}finally{gate.Release();}}
 static string Key(string root)=>Path.GetFullPath(root).ToUpperInvariant();
}
