using Windows.Storage;using Windows.Storage.AccessCache;
namespace SmartOrganizer.WinUI.Services;
public sealed class FutureFolderAccessService
{
 const string Token="SmartOrganizer.WatchedFolder";
 public string Remember(StorageFolder folder){var list=StorageApplicationPermissions.FutureAccessList;list.AddOrReplace(Token,folder,folder.Path);return folder.Path;}
 public async Task<StorageFolder?> RestoreAsync(){var list=StorageApplicationPermissions.FutureAccessList;if(!list.ContainsItem(Token))return null;try{return await list.GetFolderAsync(Token);}catch{return null;}}
 public void Forget(){var list=StorageApplicationPermissions.FutureAccessList;if(list.ContainsItem(Token))list.Remove(Token);}
}
