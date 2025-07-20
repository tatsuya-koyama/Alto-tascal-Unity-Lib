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
        public int reservedNum { get; private set; } = 0;

        GameObject _original;
        Stack<T> _pool = new Stack<T>();

        public AltoObjectPool(GameObject original, int reserveNum = 64)
        {
            _original = original;
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
                    + $" (now total is <color=#{CustomLogger.COLOR_WARN}>{ this.reservedNum }</color>)");
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
            _pool.Clear();
        }

        public int RemainCount
        {
            get { return _pool.Count; }
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        T Create()
        {
            var newObj = GameObject.Instantiate<GameObject>(_original);
            var obj = newObj.GetComponent<T>();
            obj.OnCreate();
            obj.SetPool(this);
            ++this.reservedNum;
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
