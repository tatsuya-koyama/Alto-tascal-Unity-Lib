using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AltoFramework
{
    public class AltoObjectPoolRegistry
    {
        Dictionary<Type, IAltoObjectPool> _pools = new Dictionary<Type, IAltoObjectPool>();

        public AltoObjectPool<T> CreatePool<T>(GameObject original, int reserveNum) where T : PoolableBehaviour
        {
            Type behaviourType = typeof(T);
            CheckMultipleCreate(behaviourType);

            var objectPool = new AltoObjectPool<T>(original, reserveNum);
            _pools.Add(behaviourType, objectPool);
            return objectPool;
        }

        public AltoObjectPool<T> GetPool<T>() where T : PoolableBehaviour
        {
            Type behaviourType = typeof(T);
            IAltoObjectPool objectPool;
            if (!_pools.TryGetValue(behaviourType, out objectPool))
            {
                throw new Exception($"[AltoObjectPoolRegistry] Object Pool not initialized : {behaviourType}");
            }
            return (AltoObjectPool<T>)objectPool;
        }

        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }

        //----------------------------------------------------------------------
        // For debug
        //----------------------------------------------------------------------

        [Conditional("ALTO_DEBUG")]
        void CheckMultipleCreate(Type behaviourType)
        {
            if (_pools.ContainsKey(behaviourType))
            {
                AltoLog.FW_Warn($"[AltoObjectPoolRegistry] Multiple creation detected : {behaviourType}");
            }
        }
    }
}
