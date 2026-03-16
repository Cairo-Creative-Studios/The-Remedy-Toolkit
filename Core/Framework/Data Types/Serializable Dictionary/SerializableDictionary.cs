using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Remedy.Framework
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        [SerializeField]
        private List<DictionaryItem> items = new List<DictionaryItem>();

        private bool invalidFlag;

        public TValue this[TKey key]
        {
            get
            {
                if (dictionary.ContainsKey(key))
                    return dictionary[key];
                else
                {
                    //Debug.LogWarning("Key " + key + " doesn't exist!");
                    return default(TValue);
                }
            }

            set
            {
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, value);
                else
                    dictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return dictionary.Values; }
        }

        public void Add(TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly; }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return (dictionary as IEnumerable<KeyValuePair<TKey, TValue>>).GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void OnBeforeSerialize()
        {
            if (invalidFlag)
            {
                return;
            }
            else
            {
                items.Clear();
            }

            foreach (var pair in dictionary)
            {
                items.Add(new DictionaryItem(pair.Key, pair.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            invalidFlag = false;

            for (var i = 0; i < items.Count; ++i)
            {
                if (items[i] != null)
                {
                    if (items[i].key != null || !(dictionary.ContainsKey(items[i].key)))
                    {
                        try
                        {
                            dictionary.Add(items[i].key, items[i].value);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        invalidFlag = true;
                        continue;
                    }
                }
            }

            if (!invalidFlag)
            {
                items.Clear();
            }
        }

        public SerializableDictionary()
        {
        }

        /// <summary>
        /// Clones the other Dictionary into this one.
        /// </summary>
        /// <param name="from">From.</param>
        public SerializableDictionary(SerializableDictionary<TKey, TValue> from)
        {
            foreach (TKey key in from.Keys)
            {
                Add(key, from[key]);
            }
        }

        public IReadOnlyDictionary<TKey, TValue> GetReadOnlyDictionary()
        {
            return dictionary;
        }

        public TKey KeyAt(int index)
        {
            return items[index].key;
        }

        public TValue ValueAt(int index)
        {
            return items[index].value;
        }

        public override string ToString()
        {
            var returnValue = "";

            var keyList = Keys.ToList();
            for (int i = 0; i < keyList.Count; i++)
            {
                var key = keyList[i];

                var keyString = key is float ? (Math.Truncate(((float)(object)key) * 100) / 100).ToString() : key.ToString();
                var valueString = this[key] is float ? (Math.Truncate(((float)(object)this[key]) * 100) / 100).ToString() : this[key].ToString();
                var itemString = keyString + ":" + valueString;

                returnValue += itemString;

                if (i < keyList.Count - 1)
                    returnValue += ",";
            }

            return returnValue;
        }

        /// <summary>
        /// Creates a new serializable dictionary from a string representation.
        /// </summary>
        /// <param name="dictionaryString">The string representation of the dictionary.</param>
        /// <returns>A new serializable dictionary with the same key-value pairs as the string.</returns>
        public static SerializableDictionary<TKey, TValue> NewFromString(string dictionaryString)
        {
            var dictionary = new SerializableDictionary<TKey, TValue>();

            var items = dictionaryString.Split(',');

            foreach (var item in items)
            {
                var parts = item.Split(':');
                var keyString = parts[0];
                var valueString = parts[1];

                var key = (TKey)Convert.ChangeType(keyString, typeof(TKey));
                var value = (TValue)Convert.ChangeType(valueString, typeof(TValue));

                dictionary.Add(key, value);
            }

            // Return the dictionary
            return dictionary;
        }

        /// <summary>
        /// Sets the dictionary of this instance from a string representation.
        /// </summary>
        public void FromString(string dictionaryString)
        {
            dictionary = NewFromString(dictionaryString).dictionary;
        }

        public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serializableDictionary)
        {
            if (serializableDictionary == null)
                return null;

            return new Dictionary<TKey, TValue>(serializableDictionary.dictionary);
        }

        public static implicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> normalDictionary)
        {
            if (normalDictionary == null)
                return null;

            var sDict = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in normalDictionary)
            {
                sDict.Add(kvp.Key, kvp.Value);
            }
            return sDict;
        }

        [Serializable]
        public class DictionaryItem
        {
            [SerializeField]
            public TKey key;
            [SerializeField]
            public TValue value;

            public DictionaryItem(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }
    }
}