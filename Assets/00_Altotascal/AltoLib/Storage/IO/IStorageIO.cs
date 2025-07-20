using Cysharp.Threading.Tasks;

namespace AltoLib
{
    public interface IStorageIO
    {
        UniTask<bool> WriteFileAsync(
            IAltoStorageData data, string cryptoKey, string dataPath, string slotPrefix
        );

        UniTask<bool> WriteDebugFileAsync(
            IAltoStorageData data, string dataPath, string slotPrefix
        );

        UniTask<bool> ReadFileAsync(
            IAltoStorageData data, string cryptoKey, string dataPath, string slotPrefix
        );
    }
}
