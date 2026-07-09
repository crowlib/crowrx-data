using System;
using System.Collections.Generic;
using System.Linq;

namespace CrowRx.Data
{
    public static class Broker
    {
        private static readonly Dictionary<Type, List<IManaged>> _managedData = new();
        private static readonly Stack<Queue<IManaged>> _tempManagedBuffer = new();

        public static void UnsubscribeAll()
        {
            foreach (IManaged managed in _managedData.Values.SelectMany(managedData => managedData))
            {
                managed.Unsubscribe();
            }
        }

        public static TTarget[] All<TTarget>() where TTarget : ITarget =>
            _managedData
                .SelectMany(pair => pair.Value)
                .Where(managed => managed.Target is TTarget)
                .Select(managed => managed.Target)
                .Cast<TTarget>()
                .ToArray();

        /// <summary>
        /// 기본 사용 권장. Boxing/Unboxing이 발생하지 않음.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        public static void UpdateBy<TSource>(in TSource source)
            where TSource : ISource
        {
            Queue<IManaged> changedManagedData = GetManagedDataQueue();

            UpdateByInternal(in source, changedManagedData);

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            ReturnManagedDataQueue(changedManagedData);
        }

        public static void UpdateBy(ISource source)
        {
            Queue<IManaged> changedManagedData = GetManagedDataQueue();

            UpdateByInternal(source, changedManagedData);

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            ReturnManagedDataQueue(changedManagedData);
        }

        public static void UpdateBy(params ISource[] sources) => UpdateBy(sources, sources.Length);

        public static void UpdateBy(ISource[] sources, int count)
        {
            Queue<IManaged> changedManagedData = GetManagedDataQueue();

            for (int i = 0; i < count; i++)
            {
                UpdateByInternal(sources[i], changedManagedData);
            }

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            ReturnManagedDataQueue(changedManagedData);
        }

        public static void UpdateBy(ICollection<ISource> sources)
        {
            Queue<IManaged> changedManagedData = GetManagedDataQueue();

            foreach (ISource source in sources)
            {
                UpdateByInternal(source, changedManagedData);
            }

            foreach (IManaged changedManaged in changedManagedData)
            {
                changedManaged.OnChanged();
            }

            ReturnManagedDataQueue(changedManagedData);
        }

        internal static bool TryGetManagedData(Type sourceType, out List<IManaged> managedData) => _managedData.TryGetValue(sourceType, out managedData);

        internal static bool ContainsSourceType(Type sourceType) => _managedData.ContainsKey(sourceType);

        internal static void ResisterInternal(Type sourceType, IManaged managed)
        {
            if (!_managedData.TryGetValue(sourceType, out List<IManaged> managedData))
            {
                managedData = new List<IManaged>();

                _managedData.Add(sourceType, managedData);
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
                if (_managedData.TryGetValue(sourceType, out List<IManaged> managedData))
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
                if (_managedData.TryGetValue(sourceType, out List<IManaged> managedData))
                {
                    foreach (IManaged managed in managedData)
                    {
                        managed.UpdateTarget(sourceType, in source, changedManagedData);
                    }
                }

                sourceType = sourceType.BaseType;
            } while (sourceType is not null && typeof(ISource).IsAssignableFrom(sourceType));
        }

        private static Queue<IManaged> GetManagedDataQueue() => _tempManagedBuffer.Count > 0 ? _tempManagedBuffer.Pop() : new Queue<IManaged>();

        private static void ReturnManagedDataQueue(Queue<IManaged> queue)
        {
            queue.Clear();

            _tempManagedBuffer.Push(queue);
        }
    }
}