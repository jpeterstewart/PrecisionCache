namespace PrecisionCache
{
    public interface IPrecisionCache
    {
        int Count { get; }
        void Dispose();
        bool TryGetValue(string lookupKey, out object value);
        void AddOrUpdate(string key, object value, params string[] tags);
        void TryRemove(string key);
        void Clear();
        int CountTag(string tag);
        int FlushTag(string tag);
    }
}
