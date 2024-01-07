using UnityEngine;

namespace Kukumberman.SaveSystem
{
    public sealed class PlayerPrefsSaveSystem : ISaveSystem
    {
        public string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
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
