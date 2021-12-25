using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// JsonUtility で使う用のシリアライズ可能な Dictionary.
    /// SerializableDictionary だと少々長いので省略形
    /// </summary>
    [System.Serializable]
    public class SerialDict<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // * シリアライズ後のサイズに影響するので短い変数名にしている
        [SerializeField] List<TKey>   k = new List<TKey>();    // keys
        [SerializeField] List<TValue> v = new List<TValue>();  // values

        public void OnBeforeSerialize()
        {
            k.Clear();
            v.Clear();

            foreach (var item in this)
            {
                k.Add(item.Key);
                v.Add(item.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();
            for (int i = 0; i < k.Count; ++i)
            {
                this[k[i]] = v[i];
            }
        }
    }
}
