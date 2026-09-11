using Windows.Storage;
namespace SmartOrganizer.WinUI.Services;
public sealed class SettingsService
{
    private readonly ApplicationDataContainer values = ApplicationData.Current.LocalSettings;
    public T Get<T>(string key, T fallback)
    {
        if (!values.Values.TryGetValue(key, out var raw) || raw is null) return fallback;
        try { return (T)Convert.ChangeType(raw, typeof(T)); } catch { return fallback; }
    }
    public void Set<T>(string key, T value) => values.Values[key] = value;
}
