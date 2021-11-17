using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AzureFunctions.TestHelpers
{
    public sealed class DummyQueryCollection : IQueryCollection
    {
        private readonly Dictionary<string, StringValues> _store = new();

        public StringValues this[string key]
        {
            get => _store[key];
            set => _store[key] = value;
        }

        public int Count => _store.Count;
        public ICollection<string> Keys => _store.Keys;
        public bool ContainsKey(string key) => _store.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _store.GetEnumerator();
        public bool TryGetValue(string key, out StringValues value) => _store.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _store.GetEnumerator();

        public void Add(string key, StringValues value) => _store.Add(key, value);
    }
}