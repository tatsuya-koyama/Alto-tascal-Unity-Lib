using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace AltoLib
{
    public class SimpleProfiler : MonoBehaviour
    {
        [SerializeField] protected Text text = null;
        [SerializeField] protected float updateInterval = 1.0f;

        int   _frameCount;
        float _timeToUpdate;
        float _passedTime;

        void Update()
        {
            ++_frameCount;
            _timeToUpdate -= Time.deltaTime;
            _passedTime += Time.deltaTime;

            if (_timeToUpdate > 0) { return; }
            _timeToUpdate += updateInterval;

            // FPS
            var fps = _frameCount / _passedTime;
            _frameCount = 0;
            _passedTime = 0;

            // Memory
            var totalMemory  = Profiler.GetTotalReservedMemoryLong()       / 1024f / 1024f;
            var usedMemory   = Profiler.GetTotalAllocatedMemoryLong()      / 1024f / 1024f;
            var unusedMemory = Profiler.GetTotalUnusedReservedMemoryLong() / 1024f / 1024f;

            Display(fps, totalMemory, usedMemory, unusedMemory);
        }

        protected virtual void Display(float fps, float totalMemory, float usedMemory, float unusedMemory)
        {
            text.text =
                "[FPS] " + fps.ToString("0.0") + "\n"
                + "[Memory] " + totalMemory.ToString("0.0") + " MB\n"
                + " - Used: " + usedMemory.ToString("0.0") + " MB";
        }
    }
}
