using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// JsonUtility で使う用のシリアライズ可能な HashSet.
    /// SerializableHashSet だと少々長いので省略形
    /// </summary>
    [System.Serializable]
    public class SerialHash<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        // * シリアライズ後のサイズに影響するので短い変数名にしている
        [SerializeField] List<T> v = new List<T>();    // values

        public void OnBeforeSerialize()
        {
            v = this.ToList();
        }

        public void OnAfterDeserialize()
        {
            this.Clear();
            foreach (var value in v)
            {
                this.Add(value);
            }
        }
    }
}
