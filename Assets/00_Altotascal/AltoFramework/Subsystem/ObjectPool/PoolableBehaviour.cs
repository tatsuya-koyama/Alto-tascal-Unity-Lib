using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltoFramework
{
    public class PoolableBehaviour : MonoBehaviour
    {
        protected IAltoObjectPool _pool;

        public void ReturnToPool()
        {
            if (_pool == null)
            {
                Alto.Log.FW_Warn("[PoolableBehaviour] Pool is not set.");
                Destroy(gameObject);
                return;
            }
            _pool.Return(this);
        }

        //----------------------------------------------------------------------
        // Called from AltoObjectPool
        //----------------------------------------------------------------------

        internal void SetPool(IAltoObjectPool pool)
        {
            _pool = pool;
        }

        public virtual void OnCreate()
        {
        }

        public virtual void OnGetFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
        }
    }
}
