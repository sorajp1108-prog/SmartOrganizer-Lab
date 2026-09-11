using System.ComponentModel;using System.Runtime.InteropServices;using Microsoft.Win32.SafeHandles;using SmartOrganizer.WinUI.Models;
namespace SmartOrganizer.WinUI.Services;
public sealed class WindowsFileIdentityProvider
{
 const int FileIdInfo=18;
 [StructLayout(LayoutKind.Sequential)]struct FILE_ID_128{[MarshalAs(UnmanagedType.ByValArray,SizeConst=16)]public byte[] Identifier;}
 [StructLayout(LayoutKind.Sequential)]struct FILE_ID_INFO{public ulong VolumeSerialNumber;public FILE_ID_128 FileId;}
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool GetFileInformationByHandleEx(SafeFileHandle hFile,int infoClass,out FILE_ID_INFO info,uint size);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool GetVolumeInformation(string root,string? volumeName,uint volumeNameSize,out uint serial,out uint maxComponentLength,out uint flags,System.Text.StringBuilder fileSystemName,uint fileSystemNameSize);
 public FileIdentity Get(string path)
 {
  var full=Path.GetFullPath(path);try{var root=Path.GetPathRoot(full)??"";var fs=new System.Text.StringBuilder(32);uint serial,max,flags;if(!GetVolumeInformation(root,null,0,out serial,out max,out flags,fs,(uint)fs.Capacity))throw new Win32Exception(Marshal.GetLastWin32Error());using var handle=File.OpenHandle(full,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete,FileOptions.None);if(!GetFileInformationByHandleEx(handle,FileIdInfo,out var info,(uint)Marshal.SizeOf<FILE_ID_INFO>()))throw new Win32Exception(Marshal.GetLastWin32Error());var id=Convert.ToHexString(info.FileId.Identifier??[]);if(id.Length==0||id.All(c=>c=='0'))throw new InvalidDataException("File ID unavailable");var volume=info.VolumeSerialNumber.ToString("X16");return new($"FID:{volume}:{id}",FileIdentityKind.WindowsFileId,volume,id,fs.ToString(),full,true,null);}catch(Exception e)when(e is IOException or UnauthorizedAccessException or Win32Exception or InvalidDataException or NotSupportedException){var root=Path.GetPathRoot(full)??"";return new($"PATH:{root.ToUpperInvariant()}:{full.ToUpperInvariant()}",FileIdentityKind.PathFallback,null,null,"Unknown",full,false,e.Message);}
 }
}
