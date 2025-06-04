using System;
using UnityEngine;

namespace Kukumberman.SaveSystem
{
    public sealed class PlayerPrefsSaveSystem : ISaveSystem
    {
        public string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public byte[] GetBytes(string key)
        {
            var base64 = GetString(key);

            return Convert.FromBase64String(base64);
        }

        public void SetBytes(string key, byte[] value)
        {
            var base64 = Convert.ToBase64String(value);

            SetString(key, base64);
        }

        public bool Remove(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();

            return true;
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
    }
}
