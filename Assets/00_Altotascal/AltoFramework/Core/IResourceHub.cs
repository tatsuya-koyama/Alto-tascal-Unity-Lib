namespace AltoFramework
{
    public interface IResourceHub
    {
        IResourceStore globalScopeResourceStore { get; }
        IResourceStore sceneScopeResourceStore  { get; }
    }
}
