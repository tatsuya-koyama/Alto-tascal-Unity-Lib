using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AltoLib.UnityExt
{
    public static class AnimatorExtension
    {
        public static async UniTask PlayAsync(
            this Animator animator,
            int stateNameHash, int layer = 0, float normalizedTime = 0f
        )
        {
            animator.Play(stateNameHash, layer, normalizedTime);
            await UniTask.DelayFrame(1);

            await UniTask.WaitUntil(() =>
            {
                if (animator == null) { return true; }
                var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                return stateInfo.shortNameHash != stateNameHash
                    || stateInfo.normalizedTime >= 1f;
            });
        }
    }
}
