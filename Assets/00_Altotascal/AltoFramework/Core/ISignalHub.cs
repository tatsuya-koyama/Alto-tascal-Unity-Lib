namespace AltoFramework
{
    public interface ISignalHub
    {
        AltoSignalRegistry globalScopeSignalRegistry { get; }
        AltoSignalRegistry sceneScopeSignalRegistry  { get; }
    }
}
