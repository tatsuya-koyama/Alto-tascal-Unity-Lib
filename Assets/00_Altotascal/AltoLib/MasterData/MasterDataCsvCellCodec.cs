namespace AltoLib
{
    /// <summary>
    /// CSV 内のコンマと改行の退避表現を相互変換
    /// </summary>
    public static class MasterDataCsvCellCodec
    {
        const string CommaTag = "<comma>";
        const string LineBreakTag = "<br>";

        public static string Encode(string value)
        {
            return value
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Replace(",", CommaTag)
                .Replace("\n", LineBreakTag);
        }

        public static string Decode(string value)
        {
            return value
                .Replace(CommaTag, ",")
                .Replace(LineBreakTag, "\n");
        }
    }
}
