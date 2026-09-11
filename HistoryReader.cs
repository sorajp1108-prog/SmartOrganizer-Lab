using System.Text.Json;using SmartOrganizer.WinUI.Models;using Windows.Storage;
namespace SmartOrganizer.WinUI.Services;
public sealed class HistoryReader
{
 public async Task<IReadOnlyList<HistoryRow>> ReadAsync()
 {
  var path=Path.Combine(ApplicationData.Current.LocalFolder.Path,"operation-history.json");if(!File.Exists(path))return Array.Empty<HistoryRow>();
  try{await using var s=File.OpenRead(path);return await JsonSerializer.DeserializeAsync<List<HistoryRow>>(s)??[];}catch(JsonException){return Array.Empty<HistoryRow>();}catch(IOException){return Array.Empty<HistoryRow>();}
 }
}
