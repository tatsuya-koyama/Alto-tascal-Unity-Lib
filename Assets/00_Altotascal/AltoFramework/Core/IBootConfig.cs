using UnityEngine;

namespace AltoFramework
{
    public interface IBootConfig
    {
        int numBgmSourcePool { get; }
        int numSeSourcePool { get; }
        bool useGlobalAudioListener { get; }

        void OnGameBoot();
    }

    public class DefaultBootConfig : IBootConfig
    {
        public int numBgmSourcePool => 2;
        public int numSeSourcePool  => 8;
        public bool useGlobalAudioListener => true;

        public virtual void OnGameBoot() {}
    }
}
