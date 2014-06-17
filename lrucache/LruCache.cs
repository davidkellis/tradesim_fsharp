using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archer.Collections
{
  public class LruCache<TKey, TValue> : IDictionary<TKey, TValue>
  {
    #region Internals
    private IDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> NodeMap;
    private LinkedList<KeyValuePair<TKey, TValue>> NodeList; 
    #endregion

    #region Interface
    public int MaxSize { get; set; }
    public TValue MostRecent { get { return NodeList.First.Value.Value; } }
    public TValue LeastRecent { get { return NodeList.Last.Value.Value; } }
    #endregion

    #region Constructors
    public LruCache(int maxSize = 32)
    {
      MaxSize = maxSize;
      NodeMap = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
      NodeList = new LinkedList<KeyValuePair<TKey, TValue>>();
    }
    #endregion

    #region IDictionary<TKey, TValue>
    /// <summary>
    /// Adds a key/value pair to the cache, removing the least recently used 
    /// item in the cache if necessary.
    /// </summary>
    public void Add(TKey key, TValue value)
    {
      if (NodeList.Count >= MaxSize)
      {
        var nodeToRemove = NodeList.Last;
        NodeMap.Remove(nodeToRemove.Value.Key);
        NodeList.Remove(nodeToRemove);
      }

      NodeList.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
      NodeMap.Add(key, NodeList.First);
    }

    /// <summary>
    /// Checks the cache for a key is found. If the key is found, 
    /// the last use state of the associated value is updated.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(TKey key)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (NodeMap.TryGetValue(key, out node))
      {
        UpdateUse(node);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Gets the collection of keys contained by the dictionary.
    /// </summary>
    public ICollection<TKey> Keys
    {
      get { return NodeMap.Keys; }
    }

    /// <summary>
    /// Removes the key and its associated value from the dictionary.
    /// </summary>
    public bool Remove(TKey key)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (NodeMap.TryGetValue(key, out node))
      {
        NodeList.Remove(node);
        NodeMap.Remove(key);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Gets the collection of values contained by the dictionary.
    /// </summary>
    public ICollection<TValue> Values
    {
      get { return NodeList.Select(n => n.Value).ToList(); }
    }

    /// <summary>
    /// Gets or sets the value associated with the provided key, updating
    /// its last-used state in the dictionary.
    /// </summary>
    public TValue this[TKey key]
    {
      get
      {
        LinkedListNode<KeyValuePair<TKey, TValue>> node;
        if (NodeMap.TryGetValue(key, out node))
        {
          UpdateUse(node);
          return node.Value.Value;
        }
        else throw new KeyNotFoundException(string.Format("Key not found: {0}", key));
      }
      set
      {
        LinkedListNode<KeyValuePair<TKey, TValue>> node;
        if (NodeMap.TryGetValue(key, out node))
        {
          UpdateUse(node);
          node.Value = new KeyValuePair<TKey, TValue>(key, value);
        }
        else
        {
          node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
          NodeList.AddFirst(node);
          NodeMap[key] = node;
        }
      }
    }

    /// <summary>
    /// Adds a key/value pair to the dictionary.
    /// </summary>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      NodeList.AddFirst(item);
      NodeMap.Add(item.Key, NodeList.First);
    }

    /// <summary>
    /// Clears the dictionary.
    /// </summary>
    public void Clear()
    {
      NodeList.Clear();
      NodeMap.Clear();
    }

    /// <summary>
    /// Gets whether or not a given key is contained by the dictionary.
    /// </summary>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return NodeMap.ContainsKey(item.Key);
    }

    /// <summary>
    /// Copies the key/value pairs values contained by the dictionary to the provided 
    /// array in the order in which they were last used.
    /// </summary>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      NodeMap.Select(kv => new KeyValuePair<TKey, TValue>(kv.Key, kv.Value.Value.Value)).ToArray().CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Gets the number of items contained by the dictionary.
    /// </summary>
    public int Count
    {
      get { return NodeList.Count; }
    }

    /// <summary>
    /// Returns false. :)
    /// </summary>
    public bool IsReadOnly
    {
      get { return false; }
    }

    /// <summary>
    /// Removes a key/value pair from the dictionary based on the provided key.
    /// </summary>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      return this.Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return NodeMap.Select(kv => new KeyValuePair<TKey, TValue>(kv.Key, kv.Value.Value.Value)).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    } 
    #endregion

    #region Methods
    public bool TryGetValue(TKey key, out TValue value)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;

      if (NodeMap.TryGetValue(key, out node))
      {
        value = node.Value.Value;
        UpdateUse(node);
        return true;
      }

      value = default(TValue);
      return false;
    }

    private void UpdateUse(LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
      NodeList.Remove(node);
      NodeList.AddFirst(node);
    }

    public IEnumerable<LinkedListNode<KeyValuePair<TKey, TValue>>> EnumerateNodes()
    {
      var current = NodeList.First;
      while (current != null)
      {
        yield return current;
        current = current.Next;
      }
    }
    #endregion
  }
}