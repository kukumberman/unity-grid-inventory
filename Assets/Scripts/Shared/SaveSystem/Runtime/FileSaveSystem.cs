using System.IO;
using UnityEngine;

namespace Kukumberman.SaveSystem
{
    public sealed class FileSaveSystem : ISaveSystem
    {
        public static FileSaveSystem Persistent = new FileSaveSystem(
            Application.persistentDataPath
        );

        private readonly string _baseDirectory;

        public FileSaveSystem(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string GetString(string key)
        {
            var path = GetSavePath(key);

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return null;
        }

        public bool Remove(string key)
        {
            var path = GetSavePath(key);

            if (File.Exists(path))
            {
                File.Delete(path);

                return true;
            }

            return false;
        }

        public void SetString(string key, string value)
        {
            var path = GetSavePath(key);
            File.WriteAllText(path, value);
        }

        public string GetSavePath(string fileName)
        {
            return Path.Combine(_baseDirectory, fileName);
        }

        public string GetBaseDirectory()
        {
            return _baseDirectory;
        }
    }
}
