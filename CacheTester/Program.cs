using System;
using PrecisionCache;
using System.Threading;

namespace CacheTester
{
    class Program
    {
        private static readonly IPrecisionCache MyLocalCache = new SimpleLocalCache(2, 1);  // itemTimeout & trimInterval are optional

        private static void AddToCache(string key, string value, params string[] tags)
        {
            Console.WriteLine("Adding to cache:   {0} -> {1}", key, value);
            MyLocalCache.AddOrUpdate(key, value, tags);  // tags are optional
        }

        private static void TryToRetrieve(string key)
        {
            Console.WriteLine("\nRetrieving:\t{0}", key);
            if (MyLocalCache.TryGetValue(key, out var valueFromCache))
                Console.WriteLine("Found in cache:\t{0} (key) -> {1} (value)", key, valueFromCache);
            else
                Console.WriteLine("Key not found:\t{0}", key);
        }

        private static void WaitSomeSeconds(int seconds)
        {
            Console.Write("\nWaiting for cached item to expire: {0:00}", seconds);
            for (var i = seconds-1; i >= 0; i--)
            {
                Thread.Sleep(1000);
                Console.Write("\b\b{0:00}", i);
            }
            Console.WriteLine();
        }

        static void Main()
        {
            const string key = "This";
            const string key2 = "That";

            AddToCache(key, "These", "Tag1");
            AddToCache(key2, "Those");

            TryToRetrieve(key);
            TryToRetrieve(key2);
            TryToRetrieve("SomethingElse");

            WaitSomeSeconds(60);

            TryToRetrieve(key);
            TryToRetrieve(key2);

            Console.WriteLine("\nItems in cache:\t{0}", MyLocalCache.Count);
            Console.WriteLine("\nItems with tag \"{0}\":\t{1}", "Tag1", MyLocalCache.CountTag("Tag1"));

            AddToCache(key, "How now?", "Tag2");

            WaitSomeSeconds(60);

            TryToRetrieve(key);
            TryToRetrieve(key2);

            Console.WriteLine("\nItems in cache:\t{0}\n", MyLocalCache.Count);

         //   Console.ReadKey();
        }
    }
}
