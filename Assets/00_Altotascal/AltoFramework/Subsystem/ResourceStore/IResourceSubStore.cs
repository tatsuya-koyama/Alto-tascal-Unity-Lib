using System.Collections.Generic;

namespace AltoFramework
{
    public interface IResourceSubStore<T> where T : UnityEngine.Object
    {
        T Get(string assetAddress);
        bool Contains(string assetAddress);
        void OnLoad(string assetAddress, T resource);
        void OnUnload(string assetAddress);
    }

    //--------------------------------------------------------------------------
    // Base implementation
    //--------------------------------------------------------------------------

    public class ResourceSubStore<T> : IResourceSubStore<T> where T : UnityEngine.Object
    {
        protected Dictionary<string, T> _resources = new Dictionary<string, T>();

        public T Get(string assetAddress)
        {
            if (!_resources.ContainsKey(assetAddress))
            {
                Alto.Log.FW_Error($"[{GetType().Name}] Resource not found : <b>{assetAddress}</b>");
                return null;
            }
            return _resources[assetAddress];
        }

        public bool Contains(string assetAddress)
        {
            return _resources.ContainsKey(assetAddress);
        }

        //----------------------------------------------------------------------
        // called by framework
        //----------------------------------------------------------------------

        public virtual void OnLoad(string assetAddress, T resource)
        {
            Alto.Log.FW($"[{GetType().Name}] OnLoad : <b>{assetAddress}</b> - {resource.GetType()}");
            Add(assetAddress, resource);
        }

        public virtual void OnUnload(string assetAddress)
        {
            Alto.Log.FW($"[{GetType().Name}] *** OnUnload : <b>{assetAddress}</b>");
            Remove(assetAddress);
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        void Add(string assetAddress, T resource)
        {
            if (_resources.ContainsKey(assetAddress)) { return; }
            _resources.Add(assetAddress, resource);
        }

        void Remove(string assetAddress)
        {
            if (!_resources.ContainsKey(assetAddress)) { return; }
            _resources.Remove(assetAddress);
        }
    }
}
