using UnityEngine;

namespace AltoFramework
{
    public interface IBootConfig
    {
        void OnGameBoot();
    }

    public class DefaultBootConfig : IBootConfig
    {
        public virtual void OnGameBoot()
        {
            Application.targetFrameRate = 60;
        }
    }
}
