using System.Collections.Generic;
using System.Linq;


namespace CrowRx.Data
{
    internal interface ICouple
    {
        bool UpdateTargetBySource(ISource source, ITarget target);
    }

    internal interface ICouple<TSource> : ICouple
        where TSource : ISource
    {
        bool UpdateTargetBySource(in TSource source, ITarget target);
    }


    /// <summary>
    /// only for generated code.
    /// </summary>
    public class Couple<TSource, TTarget> : ICouple<TSource>
        where TSource : ISource
        where TTarget : ITarget<TSource>
    {
        private static Couple<TSource, TTarget>? s_instance;


        public static void Init()
        {
            if (s_instance is not null)
            {
                return;
            }

            s_instance = new Couple<TSource, TTarget>();

            if (!Broker.TryGetManagedData(typeof(TSource), out List<IManaged> managedData))
            {
                return;
            }

            foreach (
                IManaged managed
                in
                managedData.Where(managed => managed.GetType().GetGenericArguments()[0] == typeof(TTarget)))
            {
                managed.AddCouple(typeof(TSource), s_instance);

                break;
            }
        }


        bool ICouple.UpdateTargetBySource(ISource source, ITarget target) => ((TTarget)target).UpdateBy((TSource)source);
        bool ICouple<TSource>.UpdateTargetBySource(in TSource source, ITarget target) => ((TTarget)target).UpdateBy(source);
    }
}