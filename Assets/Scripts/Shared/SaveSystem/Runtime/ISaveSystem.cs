namespace Kukumberman.SaveSystem
{
    public interface ISaveSystem
    {
        string GetString(string key);

        void SetString(string key, string value);

        byte[] GetBytes(string key);

        void SetBytes(string key, byte[] value);

        bool Remove(string key);
    }
}
