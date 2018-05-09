namespace QL.Core.Caching
{
    using System;

    public interface ICachingFactory
    {
        ICaching CreateCaching(string name);

        ICaching Default { get; }
    }
}
