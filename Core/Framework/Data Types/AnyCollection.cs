
using Remedy.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

/// <summary>
/// A collection that can represent any type of collection, and contain any type of item within the collection.
/// </summary>
public class AnyCollection : IEnumerable<object>
{
    public enum CollectionType
    {
        List,
        Array
    }

    private object _collection;
    public CollectionType ParentType { get; private set; }

    private IEnumerable<object> _tempCollection;

    // Read-only enumerable to avoid breaking internal state
    public IEnumerable<object> Collection
    {
        get
        {
            if (_collection is Array array) return array.Cast<object>();
            if (_collection is IList list) return list.Cast<object>();
            throw new InvalidOperationException("Unsupported collection type");
        }
    }

    private Type _type;
    public Type ElementType => _type ??= _collection.GetElementType();

    // Strongly-typed indexer with replace semantics
    public object this[int index]
    {
        get
        {
            return ToList()[index];
        }
        set
        {
            if (ParentType == CollectionType.List)
            {
                ((IList)_collection)[index] = value; // replace
            }
            else
            {
                ((Array)_collection).SetValue(value, index); // replace
            }
        }
    }

    public AnyCollection(object collection)
    {
        if (collection is Array)
        {
            _collection = collection;
            ParentType = CollectionType.Array;
        }
        else if (collection is IList)
        {
            _collection = collection;
            ParentType = CollectionType.List;
        }
        else
        {
            throw new ArgumentException("Collection must be IList or Array");
        }
    }

    public IEnumerator<object> GetEnumerator() => Collection.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Single item add/remove
    public void Add(object item)
    {
        if (ParentType == CollectionType.List)
        {
            ((IList)_collection).Add(item);
        }
        else
        {
            _tempCollection = Collection.Append(item);
            UpdateArray();
        }
    }

    public void Remove(object item)
    {
        if (ParentType == CollectionType.List)
        {
            ((IList)_collection).Remove(item);
        }
        else
        {
            _tempCollection = Collection.Where(i => !Equals(i, item));
            UpdateArray();
        }
    }

    public void RemoveAt(int index)
    {
        if (ParentType == CollectionType.List)
        {
            var list = (IList)_collection;

            if (index < 0 || index >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            list.RemoveAt(index);
        }
        else
        {
            var array = (Array)_collection;

            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Rebuild array without the indexed item
            _tempCollection = Collection
                .Where((item, i) => i != index);

            UpdateArray();
        }
    }


    // Range operations
    public void AddRange(IEnumerable<object> items)
    {
        if (ParentType == CollectionType.List)
        {
            foreach (var item in items) ((IList)_collection).Add(item);
        }
        else
        {
            _tempCollection = Collection.Concat(items);
            UpdateArray();
        }
    }

    public void RemoveAll(Predicate<object> predicate)
    {
        if (ParentType == CollectionType.List)
        {
            var list = (IList)_collection;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i])) list.RemoveAt(i);
            }
        }
        else
        {
            _tempCollection = Collection.Where(i => !predicate(i));
            UpdateArray();
        }
    }

    /// <summary>
    /// Creates a List from the items in the collection (with the type of the elements within it).
    /// </summary>
    /// <returns></returns>
    public IList ToList()
    {
        var listType = typeof(List<>).MakeGenericType(ElementType);
        var list = (IList)Activator.CreateInstance(listType);

        foreach (var item in Collection)
            list.Add(item);

        return list;
    }

    /// <summary>
    /// Creates an Array from the items in the collection (with the type of the elements within it).
    /// </summary>
    /// <returns></returns>
    public Array ToArray()
    {
        var array = Array.CreateInstance(ElementType, Collection.Count());

        int i = 0;
        foreach (var item in Collection)
            array.SetValue(item, i++);

        return array;
    }

    private void UpdateArray()
    {
        var tempArray = Array.CreateInstance(ElementType, _tempCollection.Count());
        Array.Copy(_tempCollection.ToArray(), tempArray, _tempCollection.Count());
        _collection = tempArray;
    }

    // Implicit conversions from array/list
    public static implicit operator AnyCollection(Array array) => new AnyCollection(array);
    public static implicit operator AnyCollection(List<object> list) => new AnyCollection(list);

    // Explicit conversions to array/list
    public static explicit operator Array(AnyCollection c) => c.Collection.ToArray();
    public static explicit operator List<object>(AnyCollection c) => c.Collection.ToList();

    // Strongly-typed helpers
    public T[] ToArray<T>() => Collection.Cast<T>().ToArray();
    public List<T> ToList<T>() => Collection.Cast<T>().ToList();
}
