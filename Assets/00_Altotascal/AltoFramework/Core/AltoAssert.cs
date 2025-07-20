using System.Diagnostics;
using System;
using UnityEngine.Assertions;

namespace AltoFramework
{
    /// <summary>
    /// Custom Assertion
    /// </summary>
    public class AltoAssert
    {
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void IsTrue(bool condition, string errorMessage = null)
        {
            if (condition) { return; }

            throw new AssertionException(errorMessage, null);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void IsNotNull(UnityEngine.Object obj, string errorMessage = null)
        {
            if (obj != null) { return; }

            throw new AssertionException(errorMessage, null);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void IsNotNull(System.Object obj, string errorMessage = null)
        {
            if (obj != null) { return; }

            throw new AssertionException(errorMessage, null);
        }
    }
}
