using System.ComponentModel;

namespace CrowRx.Data
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ForSourceGenerator
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void InitManaged<TTarget>()
            where TTarget : class, ITarget, new()
        {   
            Managed<TTarget>.DisposeAll();         
            Managed<TTarget>.Init();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void InitCouple<TSource, TTarget>()
            where TSource : ISource
            where TTarget : ITarget<TSource>
        {
            Couple<TSource, TTarget>.DisposeAll();
            Couple<TSource, TTarget>.Init();
        }
    }
}