using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltoFramework
{
    public class PoolableBehaviour : MonoBehaviour
    {
        protected IAltoObjectPool _pool;

        protected void ReturnToPool()
        {
            if (_pool == null)
            {
                AltoLog.FW_Warn("[PoolableBehaviour] Pool is not set.");
                Destroy(gameObject);
                return;
            }
            _pool.Return(this);
        }

        //----------------------------------------------------------------------
        // Called from AltoObjectPool
        //----------------------------------------------------------------------

        public void SetPool(IAltoObjectPool pool)
        {
            _pool = pool;
        }

        public virtual void OnGetFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
        }
    }
}
