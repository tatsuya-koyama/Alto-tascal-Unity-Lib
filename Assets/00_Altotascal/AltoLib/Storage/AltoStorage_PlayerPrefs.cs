namespace AltoLib
{
    /// <summary>
    /// AltoStorage の永続化先に PlayerPrefs を使う版。WebGL ビルドで使う用
    /// ※ WebGL では persistentDataPath に書き込んでも JS の FS.syncfs() を呼ばないと
    ///    反映されなかったりして扱いが煩雑なので
    /// ※ WebGL の PlayerPrefs はサイズ 1 MB 制限があるので注意
    /// </summary>
    public abstract class AltoStorage_PlayerPrefs : AltoStorage
    {
        protected override IStorageIO StorageIO => _storageIO ??= new StorageIO_PlayerPrefs();
    }
}
