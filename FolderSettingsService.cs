namespace SmartOrganizer.WinUI.Services;
public sealed class FolderSettingsService
{
 readonly SettingsService settings;public FolderSettingsService(SettingsService settings)=>this.settings=settings;
 public string GetRoot(){var fallback=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");var value=settings.Get("folder.root",fallback);return Directory.Exists(value)?value:fallback;}
 public void SetRoot(string path){if(!Directory.Exists(path))throw new DirectoryNotFoundException(path);settings.Set("folder.root",Path.GetFullPath(path));}
 public bool IncludeSubfolders{get=>settings.Get("folder.includeSubfolders",false);set=>settings.Set("folder.includeSubfolders",value);}
 public long LargeThresholdBytes{get=>Math.Max(1024*1024,settings.Get("views.largeThresholdBytes",500L*1024*1024));set=>settings.Set("views.largeThresholdBytes",Math.Max(1024*1024,value));}
 public int RecentDays{get=>Math.Clamp(settings.Get("views.recentDays",7),1,365);set=>settings.Set("views.recentDays",value);}
}
