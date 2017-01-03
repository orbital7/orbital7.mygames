﻿using Orbital7.Extensions.Windows;
using System;
using System.IO;

namespace Orbital7.MyGames.Local
{
    public class LocalAccessProvider : IAccessProvider
    {
        public bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        public string[] GetFolderPaths(string folderPath, string searchPattern = null)
        {
            if (!String.IsNullOrEmpty(searchPattern))
                return Directory.GetDirectories(folderPath, searchPattern);
            else
                return Directory.GetDirectories(folderPath);
        }

        public string EnsureFolderExists(params string[] paths)
        {
            return FileSystemHelper.EnsureFolderExists(paths);
        }

        public string DeleteFolderContents(string folderPath)
        {
            return FileSystemHelper.EnsureFolderExists(folderPath);
        }

        public string[] GetFilePaths(string folderPath, string searchPattern = null)
        {
            if (!String.IsNullOrEmpty(searchPattern))
                return Directory.GetFiles(folderPath, searchPattern);
            else
                return Directory.GetFiles(folderPath);
        }

        public bool IsDifferentCopyRequired(string sourcePath, string destinationPath)
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
        }

        public bool IsNewerCopyRequired(string sourcePath, string destinationPath)
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
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            File.Move(sourcePath, destinationPath);
        }

        public void CopyFile(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath, true);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public void WriteAllText(string filePath, string text)
        {
            File.WriteAllText(filePath, text);
        }

        public void WriteAllBytes(string filePath, byte[] bytes)
        {
            File.WriteAllBytes(filePath, bytes);
        }
    }
}
