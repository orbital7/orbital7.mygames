using Orbital7.Extensions.Windows;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Orbital7.MyGames.Local
{
    public class LocalAccessProvider : IAccessProvider
    {
        public async Task<bool> FolderExistsAsync(string folderPath)
        {
            return await Task<bool>.Run(() => Directory.Exists(folderPath));
        }

        public async Task<string[]> GetFolderPathsAsync(string folderPath, string searchPattern = null)
        {
            if (!String.IsNullOrEmpty(searchPattern))
                return await Task<string[]>.Run(() => Directory.GetDirectories(folderPath, searchPattern));
            else
                return await Task<string[]>.Run(() => Directory.GetDirectories(folderPath));
        }

        public async Task<string> EnsureFolderExistsAsync(params string[] paths)
        {
            return await Task<string>.Run(() => FileSystemHelper.EnsureFolderExists(paths));
        }

        public async Task<string> DeleteFolderContentsAsync(string folderPath)
        {
            return await Task<string>.Run(() => FileSystemHelper.EnsureFolderExists(folderPath));
        }

        public async Task<string[]> GetFilePathsAsync(string folderPath, string searchPattern = null)
        {
            if (!String.IsNullOrEmpty(searchPattern))
                return await Task<string[]>.Run(() => Directory.GetFiles(folderPath, searchPattern));
            else
                return await Task<string[]>.Run(() => Directory.GetFiles(folderPath));
        }

        public async Task<bool> IsDifferentCopyRequiredAsync(string sourcePath, string destinationPath)
        {
            return await Task<bool>.Run(() =>
            {
                if (File.Exists(destinationPath))
                {
                    var sourceProperties = new FileInfo(sourcePath);
                    var destProperties = new FileInfo(destinationPath);
                    return sourceProperties.LastWriteTimeUtc != destProperties.LastWriteTimeUtc ||
                           sourceProperties.Length != destProperties.Length;
                }
                else
                {
                    return true;
                }
            });
        }

        public async Task<bool> IsNewerCopyRequiredAsync(string sourcePath, string destinationPath)
        {
            return await Task<bool>.Run(() =>
            {
                if (File.Exists(destinationPath))
                {
                    var sourceProperties = new FileInfo(sourcePath);
                    var destProperties = new FileInfo(destinationPath);
                    return sourceProperties.LastWriteTimeUtc > destProperties.LastWriteTimeUtc;
                }
                else
                {
                    return true;
                }
            });
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            return await Task<bool>.Run(() => File.Exists(filePath));
        }

        public async Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            await Task.Run(() => File.Move(sourcePath, destinationPath));
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            await Task.Run(() => File.Copy(sourcePath, destinationPath, true));
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await Task.Run(() => File.Delete(filePath));
        }

        public async Task<string> ReadAllTextAsync(string filePath)
        {
            return await Task<bool>.Run(() => File.ReadAllText(filePath));
        }

        public async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            return await Task<bool>.Run(() => File.ReadAllBytes(filePath));
        }

        public async Task WriteAllTextAsync(string filePath, string text)
        {
            await Task.Run(() => File.WriteAllText(filePath, text));
        }

        public async Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            await Task.Run(() => File.WriteAllBytes(filePath, bytes));
        }
    }
}
