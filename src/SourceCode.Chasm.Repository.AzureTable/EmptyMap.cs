using System;
using System.Collections;
using System.Collections.Generic;

namespace SourceCode.Chasm.Repository.AzureTable
{
    internal sealed class EmptyMap<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        public static readonly EmptyMap<TKey, TValue> Empty = new EmptyMap<TKey, TValue>();

        public TValue this[TKey key] => throw new KeyNotFoundException();

        public IEnumerable<TKey> Keys => Array.Empty<TKey>();

        public IEnumerable<TValue> Values => Array.Empty<TValue>();

        public int Count => 0;

        private EmptyMap()
        { }

        public bool ContainsKey(TKey key)
            => false;

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield break;
        }
    }
}
