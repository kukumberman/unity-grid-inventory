#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Kukumberman.SaveSystem
{
    public sealed class WebglSaveSystem : ISaveSystem
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern string LocalStorageGetItem(string key);

        [DllImport("__Internal")]
        private static extern void LocalStorageSetItem(string key, string value);

        [DllImport("__Internal")]
        private static extern void LocalStorageRemoveItem(string key);
#endif

        public string GetString(string key)
        {
#if UNITY_WEBGL
            return LocalStorageGetItem(key);
#else
            return string.Empty;
#endif
        }

        public void SetString(string key, string value)
        {
#if UNITY_WEBGL
            LocalStorageSetItem(key, value);
#endif
        }

        public bool Remove(string key)
        {
#if UNITY_WEBGL
            LocalStorageRemoveItem(key);
#endif
            return true;
        }
    }
}
