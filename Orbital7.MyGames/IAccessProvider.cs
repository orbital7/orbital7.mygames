using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    public interface IAccessProvider
    {
        bool FolderExists(string folderPath);

        string[] GetFolderPaths(string folderPath, string searchPattern = null);

        string EnsureFolderExists(params string[] paths);

        string DeleteFolderContents(string folderPath);

        string[] GetFilePaths(string folderPath, string searchPattern = null);

        bool IsDifferentCopyRequired(string sourcePath, string destinationPath);

        bool IsNewerCopyRequired(string sourcePath, string destinationPath);

        bool FileExists(string filePath);

        void MoveFile(string sourcePath, string destinationPath);

        void CopyFile(string sourcePath, string destinationPath);

        void DeleteFile(string filePath);

        string ReadAllText(string filePath);

        byte[] ReadAllBytes(string filePath);

        void WriteAllText(string filePath, string text);

        void WriteAllBytes(string filePath, byte[] bytes);
    }
}
