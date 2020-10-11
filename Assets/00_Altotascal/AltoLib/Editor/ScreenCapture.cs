#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public class ScreenCapture
    {
        [MenuItem("Alto/Screen Capture x 1")]
        static void CaptureScreen_x1()
        {
            CaptureScreen(1);
        }

        [MenuItem("Alto/Screen Capture x 2")]
        static void CaptureScreen_x2()
        {
            CaptureScreen(2);
        }

        [MenuItem("Alto/Screen Capture x 3")]
        static void CaptureScreen_x3()
        {
            CaptureScreen(3);
        }

        [MenuItem("Alto/Screen Capture x 4")]
        static void CaptureScreen_x4()
        {
            CaptureScreen(4);
        }

        static void CaptureScreen(int superSize)
        {
            DateTime now = DateTime.Now;
            string timestamp = $"{ now.Year }-{ now.Month.ToString("D2") }{ now.Day.ToString("D2") }-"
                             + $"{ now.Hour.ToString("D2") }{ now.Minute.ToString("D2") }{ now.Second.ToString("D2") }";
            string filePath = $"~/Desktop/{ Application.productName }-{ timestamp }.png";
            UnityEngine.ScreenCapture.CaptureScreenshot(filePath, superSize);
        }
    }
}
#endif
