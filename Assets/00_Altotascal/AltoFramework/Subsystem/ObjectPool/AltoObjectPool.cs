using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AltoFramework
{
    public interface IAltoObjectPool
    {
        void Reserve(int num);
        void Return(PoolableBehaviour obj);
        void Clear();
    }

    public class AltoObjectPool<T> : IAltoObjectPool where T : PoolableBehaviour
    {
        GameObject _original;
        Transform _parentTransform;
        Stack<T> _pool = new();
        HashSet<T> _allObjects = new();

        public int ReservedNum => _allObjects.Count;
        public int RemainCount => _pool.Count;

        public AltoObjectPool(GameObject original, Transform parentTransform, int reserveNum = 64)
        {
            _original = original;
            _parentTransform = parentTransform;
            Reserve(reserveNum);
        }

        public void Reserve(int num)
        {
            for (var i = 0; i < num; ++i)
            {
                var obj = Create();
                Return(obj);
            }
        }

        /// <summary>
        /// プールから取得。プールが枯渇していた場合はインスタンスを生成する
        /// </summary>
        public T Get()
        {
            T obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Pop();
            }
            else
            {
                obj = Create();
                Alto.Log.Info($"[AltoObjectPool] Pool ({typeof(T)}) is empty"
                    + $" (now total is <color=#{CustomLogger.COLOR_WARN}>{ this.ReservedNum }</color>)");
            }

            obj.gameObject.SetActive(true);
            obj.OnGetFromPool();
            return obj;
        }

        /// <summary>
        /// プールに余りがあれば取得。なければ null
        /// </summary>
        public T GetIfAvailable()
        {
            if (_pool.Count == 0) { return null; }
            return Get();
        }

        public void Return(T obj)
        {
            // Clear 後、破棄がフレーム終端で確定する前に貸出先から返却される場合がある
            if (obj == null || !_allObjects.Contains(obj)) { return; }

            CheckMultipleReturn(obj);
            obj.gameObject.SetActive(false);
            obj.OnReturnToPool();
            _pool.Push(obj);
        }

        public void Return(PoolableBehaviour obj)
        {
            Return(obj as T);
        }

        public void Clear()
        {
            foreach (var obj in _allObjects)
            {
                if (obj == null) { continue; }
                GameObject.Destroy(obj.gameObject);
            }
            _pool.Clear();
            _allObjects.Clear();
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        T Create()
        {
            var newObj = GameObject.Instantiate<GameObject>(_original, _parentTransform);
            var obj = newObj.GetComponent<T>();
            obj.OnCreate();
            obj.SetPool(this);
            _allObjects.Add(obj);
            return obj;
        }

        //----------------------------------------------------------------------
        // For debug
        //----------------------------------------------------------------------

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        void CheckMultipleReturn(T obj)
        {
            if (_pool.Contains(obj))
            {
                Alto.Log.FW_Warn($"[AltoObjectPool] Multiple return detected : {typeof(T)}");
            }
        }
    }
}
