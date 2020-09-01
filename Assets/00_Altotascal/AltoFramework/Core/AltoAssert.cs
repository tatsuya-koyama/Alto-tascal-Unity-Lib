using System.Diagnostics;
using System;
using UnityEngine.Assertions;

namespace AltoFramework
{
    /// <summary>
    ///   Custom Assertion.
    ///   Define "ALTO_DEBUG" symbol to enable this feature.
    /// </summary>
    public class AltoAssert
    {
        [Conditional("ALTO_DEBUG")]
        public static void IsTrue(bool condition, string errorMessage = null)
        {
            if (condition) { return; }

            throw new AssertionException(errorMessage, null);
        }

        [Conditional("ALTO_DEBUG")]
        public static void IsNotNull(UnityEngine.Object obj, string errorMessage = null)
        {
            if (obj != null) { return; }

            throw new AssertionException(errorMessage, null);
        }

        [Conditional("ALTO_DEBUG")]
        public static void IsNotNull(System.Object obj, string errorMessage = null)
        {
            if (obj != null) { return; }

            throw new AssertionException(errorMessage, null);
        }
    }
}
