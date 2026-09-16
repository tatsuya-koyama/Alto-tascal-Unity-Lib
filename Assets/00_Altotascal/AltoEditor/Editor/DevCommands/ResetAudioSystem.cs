using UnityEngine;
using UnityEditor;

namespace AltoEditor
{
    /// <summary>
    /// たまに Unity のサンプリングレートが狂って音のピッチが変わる現象があるので
    /// それを Unity を再起動せずに修正するコマンド
    /// </summary>
    public class ResetAudioSystem
    {
        [MenuItem(AltoMenuPath.DevCommands + "Reset Audio System")]
        public static void Reset()
        {
            Log();
            Debug.Log("--- Reset ---");
            var config = AudioSettings.GetConfiguration();
            if (!AudioSettings.Reset(config))
            {
                Debug.LogError("AudioSettings.Reset failed.");
            }
            Log();
        }

        static void Log()
        {
            var config = AudioSettings.GetConfiguration();
            Debug.Log(
                $"outputSampleRate : {AudioSettings.outputSampleRate}, " +
                $"config.sampleRate : {config.sampleRate}, " +
                $"speakerMode : {config.speakerMode}"
            );
        }
    }
}
