using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public interface IAccessProvider
    {
        Task<bool> FolderExistsAsync(string folderPath);

        Task<string[]> GetFolderPathsAsync(string folderPath, string searchPattern = null);

        Task<string> EnsureFolderExistsAsync(params string[] paths);

        Task<string> DeleteFolderContentsAsync(string folderPath);

        Task<string[]> GetFilePathsAsync(string folderPath, string searchPattern = null);

        Task<bool> IsDifferentCopyRequiredAsync(string sourcePath, string destinationPath);

        Task<bool> IsNewerCopyRequiredAsync(string sourcePath, string destinationPath);

        Task<int> CompareFileCopiesAsync(string file1Path, string file2Path);

        Task<bool> FileExistsAsync(string filePath);

        Task MoveFileAsync(string sourcePath, string destinationPath);

        Task CopyFileAsync(string sourcePath, string destinationPath);

        Task DeleteFileAsync(string filePath);

        Task<string> ReadAllTextAsync(string filePath);

        Task<byte[]> ReadAllBytesAsync(string filePath);

        Task WriteAllTextAsync(string filePath, string text);

        Task WriteAllBytesAsync(string filePath, byte[] bytes);
    }
}
