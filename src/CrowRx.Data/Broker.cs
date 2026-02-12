using System;
using System.Collections.Generic;
using System.Linq;


namespace CrowRx.Data
{
    /// <summary>
    /// 사용 가능 : <see cref="Release"/>, UpdateBy().
    /// 그 외 구성 요소는 외부 사용 금지.
    /// </summary>
    public class Broker
    {
        private static readonly Dictionary<Type, List<IManaged>> s_managedData = new();

        private static readonly Stack<Queue<IManaged>> s_tempManagedBuffer = new();


        static Broker()
        {
            s_tempManagedBuffer.Push(new Queue<IManaged>(1));
        }


        public static void Release()
        {
            foreach (IManaged managed in s_managedData.Values.SelectMany(managedData => managedData))
            {
                managed.Dispose();
            }
        }

        public static TTarget[] All<TTarget>() where TTarget : ITarget =>
            s_managedData
                .SelectMany(pair => pair.Value)
                .Where(managed => managed.Target is TTarget)
                .Select(managed => managed.Target)
                .Cast<TTarget>()
                .ToArray();

        public static void UpdateBy<TSource>(in TSource source)
            where TSource : ISource
        {
            Queue<IManaged> changedManagedData =
                s_tempManagedBuffer.Count > 0
                    ? s_tempManagedBuffer.Pop()
                    : new Queue<IManaged>();

            UpdateByInternal(in source, changedManagedData);

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            changedManagedData.Clear();

            s_tempManagedBuffer.Push(changedManagedData);
        }

        public static void UpdateBy(ISource source)
        {
            Queue<IManaged> changedManagedData =
                s_tempManagedBuffer.Count > 0
                    ? s_tempManagedBuffer.Pop()
                    : new Queue<IManaged>();

            UpdateByInternal(source, changedManagedData);

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            changedManagedData.Clear();

            s_tempManagedBuffer.Push(changedManagedData);
        }

        /// <summary>
        /// GC가 발생하니 되도록 사용하지 말 것. 특히 매프레임 또는 자주 호출되는 곳에서는 사용 금지.
        /// 다른 UpdateBy 사용 권장.
        /// </summary>
        /// <param name="sources"></param>
        public static void UpdateBy(params ISource[] sources)
        {
            Queue<IManaged> changedManagedData =
                s_tempManagedBuffer.Count > 0
                    ? s_tempManagedBuffer.Pop()
                    : new Queue<IManaged>();

            foreach (ISource source in sources)
            {
                UpdateByInternal(source, changedManagedData);
            }

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            changedManagedData.Clear();

            s_tempManagedBuffer.Push(changedManagedData);
        }

        public static void UpdateBy(ISource[] sources, int count)
        {
            Queue<IManaged> changedManagedData =
                s_tempManagedBuffer.Count > 0
                    ? s_tempManagedBuffer.Pop()
                    : new Queue<IManaged>();

            for (int i = 0; i < count; i++)
            {
                UpdateByInternal(sources[i], changedManagedData);
            }

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            changedManagedData.Clear();

            s_tempManagedBuffer.Push(changedManagedData);
        }

        public static void UpdateBy(in ICollection<ISource> sources)
        {
            Queue<IManaged> changedManagedData =
                s_tempManagedBuffer.Count > 0
                    ? s_tempManagedBuffer.Pop()
                    : new Queue<IManaged>();

            foreach (ISource source in sources)
            {
                UpdateByInternal(source, changedManagedData);
            }

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            changedManagedData.Clear();

            s_tempManagedBuffer.Push(changedManagedData);
        }

    #region internal use only

        internal static bool TryGetManagedData(Type sourceType, out List<IManaged> managedData) =>
            s_managedData.TryGetValue(sourceType, out managedData);

        internal static bool ContainsSourceType(Type sourceType) => s_managedData.ContainsKey(sourceType);

        internal static void ResisterInternal(Type sourceType, IManaged managed)
        {
            if (!s_managedData.TryGetValue(sourceType, out List<IManaged> managedData))
            {
                managedData = new List<IManaged>();

                s_managedData.Add(sourceType, managedData);
            }

            if (!managedData.Contains(managed))
            {
                managedData.Add(managed);
            }
        }

        internal static void UpdateByInternal(ISource source, Queue<IManaged> changedManagedData)
        {
            Type? sourceType = source.GetType();

            do
            {
                if (s_managedData.TryGetValue(sourceType, out List<IManaged> managedData))
                {
                    foreach (IManaged managed in managedData)
                    {
                        managed.UpdateTarget(sourceType, source, changedManagedData);
                    }
                }

                sourceType = sourceType.BaseType;
            } while (sourceType is not null && typeof(ISource).IsAssignableFrom(sourceType));
        }

        internal static void UpdateByInternal<TSource>(in TSource source, Queue<IManaged> changedManagedData)
            where TSource : ISource
        {
            Type? sourceType = typeof(TSource);

            do
            {
                if (s_managedData.TryGetValue(sourceType, out List<IManaged> managedData))
                {
                    foreach (IManaged managed in managedData)
                    {
                        managed.UpdateTarget(sourceType, in source, changedManagedData);
                    }
                }

                sourceType = sourceType.BaseType;
            } while (sourceType is not null && typeof(ISource).IsAssignableFrom(sourceType));
        }

    #endregion
    }
}