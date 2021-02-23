using System;

namespace AltoLib
{
    public class IdUtil
    {
        /// <summary>
        /// GUID を 16 進 32 桁の文字列（ハイフンなし）で生成
        /// </summary>
        public static string GetGuidAs32Digits()
        {
            return System.Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// GUID を生成し、Base64 形式の 22 文字の文字列で返す
        /// （128 bit を Base64 でエンコードすると 24 文字になるが
        ///   最後の 2 文字はパディングの == なのでそれを除外）
        /// </summary>
        public static string GetGuidAs22Chars()
        {
            byte[] idBytes = System.Guid.NewGuid().ToByteArray();
            string idStr = Convert.ToBase64String(idBytes);
            return idStr.Substring(0, 22);
        }
    }
}
