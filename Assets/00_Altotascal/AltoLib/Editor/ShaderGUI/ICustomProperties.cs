namespace AltoLib.ShaderGUI
{
    public interface ICustomProperties
    {
        /// <summary>
        /// インデクサでリフレクションして
        /// customProperties["propName"] とアクセスできるようにする。
        /// GUI の実装を簡略化するため
        /// </summary>
        object this[string propertyName] { get; set; }
    }
}
