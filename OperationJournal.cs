using System.Text.Json;
namespace SmartOrganizer.WinUI.Operations;
public sealed class OperationJournal
{
 readonly SemaphoreSlim gate=new(1,1);readonly string path;readonly string backupPath;public string? LastRecoveryNotice{get;private set;}
 static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.General){WriteIndented=true};
 public OperationJournal(string? baseFolder=null){baseFolder??=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SmartOrganizer");path=Path.Combine(baseFolder,"operation-journal.json");backupPath=path+".bak";}
 public async Task<IReadOnlyList<OperationRecord>> ReadAsync(CancellationToken token){await gate.WaitAsync(token);try{return await ReadWithRecoveryUnlocked(token);}finally{gate.Release();}}
 public async Task UpsertAsync(OperationRecord record,CancellationToken token){await gate.WaitAsync(token);try{var all=(await ReadWithRecoveryUnlocked(token)).ToList();var i=all.FindIndex(x=>x.Id==record.Id);if(i<0)all.Add(record);else all[i]=record;await WriteAtomicAsync(all,token);}finally{gate.Release();}}
 async Task<IReadOnlyList<OperationRecord>> ReadWithRecoveryUnlocked(CancellationToken token)
 {
  var primary=await TryReadAsync(path,token);if(primary is not null)return primary;var backup=await TryReadAsync(backupPath,token);if(backup is not null){LastRecoveryNotice="操作履歴の本体が破損していたためバックアップを読み込みました";await RestorePrimaryFromBackupAsync(token);return backup;}if(File.Exists(path)||File.Exists(backupPath))LastRecoveryNotice="操作履歴とバックアップを読み込めませんでした。ファイルは削除していません";return Array.Empty<OperationRecord>();
 }
 static async Task<IReadOnlyList<OperationRecord>?> TryReadAsync(string candidate,CancellationToken token){if(!File.Exists(candidate))return Array.Empty<OperationRecord>();try{await using var s=File.OpenRead(candidate);return await JsonSerializer.DeserializeAsync<List<OperationRecord>>(s,Json,token)??[];}catch(JsonException){return null;}catch(IOException){return null;}}
 async Task RestorePrimaryFromBackupAsync(CancellationToken token){Directory.CreateDirectory(Path.GetDirectoryName(path)!);var temp=path+".restore.tmp";await using(var input=File.OpenRead(backupPath)){await using var output=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None,65536,true);await input.CopyToAsync(output,token);await output.FlushAsync(token);}if(File.Exists(path))File.Move(path,path+".corrupt-"+DateTime.UtcNow.ToString("yyyyMMddHHmmss"),false);File.Move(temp,path,false);}
 async Task WriteAtomicAsync(List<OperationRecord> records,CancellationToken token){Directory.CreateDirectory(Path.GetDirectoryName(path)!);var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{await using(var s=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None,65536,FileOptions.Asynchronous|FileOptions.WriteThrough)){await JsonSerializer.SerializeAsync(s,records,Json,token);await s.FlushAsync(token);}if(File.Exists(path))File.Replace(temp,path,backupPath,true);else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}}
}
