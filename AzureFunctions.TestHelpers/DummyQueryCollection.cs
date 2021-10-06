using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AzureFunctions.TestHelpers
{
    public sealed class DummyQueryCollection : IQueryCollection
    {
        private Dictionary<string, StringValues> innerDict = new Dictionary<string, StringValues>();

        public StringValues this[string key] => innerDict[key];
        public int Count => innerDict.Count;
        public ICollection<string> Keys => innerDict.Keys;
        public bool ContainsKey(string key) => innerDict.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => innerDict.GetEnumerator();
        public bool TryGetValue(string key, out StringValues value) => innerDict.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => innerDict.GetEnumerator();

        public void Add(string key, StringValues value) => innerDict.Add(key, value);
    }
}