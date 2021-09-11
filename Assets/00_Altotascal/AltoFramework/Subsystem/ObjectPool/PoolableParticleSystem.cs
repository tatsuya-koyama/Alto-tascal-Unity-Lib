using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltoFramework
{
    public class PoolableParticleSystem : PoolableBehaviour
    {
        public new ParticleSystem particleSystem;

        public override void OnCreate()
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        void OnParticleSystemStopped()
        {
            ReturnToPool();
        }
    }
}
