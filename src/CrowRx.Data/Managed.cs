using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using R3;

namespace CrowRx.Data
{
    /// <summary>
    /// 사용 가능 : <see cref="Instance"/>, <see cref="Observable"/>.
    /// 그 외 구성 요소는 외부 사용 금지.
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class Managed<TTarget> : IManaged
        where TTarget : class, ITarget, new()
    {
        #region static

        private static readonly Dictionary<Type, ICouple> _targetUpdaters = new();

        private static Managed<TTarget>? _instance;

        public static TTarget Instance
        {
            get
            {
                Managed<TTarget> instance = Init();

                return instance._target;
            }

            set
            {
                Managed<TTarget> instance = Init();

                instance._target = value;

                if (instance._target is ISource targetAsSource)
                {
                    Broker.UpdateBy(targetAsSource);
                }
            }
        }

        public static Observable<TTarget> Observable
        {
            get
            {
                Managed<TTarget> instance = Init();

                return instance._subjectTarget.Publish().RefCount();
            }
        }

        /// <summary>
        /// only for generated code.
        /// </summary>
        internal static Managed<TTarget> Init()
        {
            if (_instance is null)
            {
                _instance = new Managed<TTarget>(new TTarget());

                foreach (
                    Type targetGenericInterfaceType
                    in
                    _instance._target.GetType()
                        .GetInterfaces()
                        .Where(interfaceType =>
                            interfaceType.IsGenericType && typeof(ITarget).IsAssignableFrom(interfaceType)))
                {
                    MethodInfo updateByMethodInfo =
                        targetGenericInterfaceType.GetMethod(
                            "UpdateBy", BindingFlags.Instance | BindingFlags.Public);

                    if (updateByMethodInfo is null)
                    {
                        continue;
                    }

                    ParameterInfo[] updateByMethodParams = updateByMethodInfo.GetParameters();
                    if (updateByMethodParams.Length != 1)
                    {
                        continue;
                    }

                    Type sourceType = updateByMethodParams[0].ParameterType;

                    if (sourceType.IsByRef)
                    {
                        sourceType = sourceType.GetElementType();
                    }

                    if (!typeof(ISource).IsAssignableFrom(sourceType))
                    {
                        continue;
                    }

                    Broker.ResisterInternal(sourceType, _instance);
                }
            }

            if (_instance._isDisposed)
            {
                _instance._subjectTarget = new Subject<TTarget>();
                _instance._isDisposed = false;
            }

            return _instance;
        }

        #endregion

        private readonly Type _targetType;

        private TTarget _target;

        private Subject<TTarget> _subjectTarget;
        private bool _isDisposed;

        public ITarget Target => _target;

        private Managed(TTarget target)
        {
            _target = target;
            _targetType = _target.GetType();

            _subjectTarget = new Subject<TTarget>();
            _isDisposed = false;
        }

        /// <summary>
        /// internal use only
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _subjectTarget.Dispose();

            _target = new TTarget();
        }

        #region internal use only

        void IManaged.UpdateTarget(Type sourceType, ISource source, Queue<IManaged> dataChangedManagedData)
        {
            if (!_targetUpdaters.TryGetValue(sourceType, out ICouple couple))
            {
                return;
            }

            try
            {
                if (!couple.UpdateTargetBySource(source, _target))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);

                return;
            }

            if (!dataChangedManagedData.Contains(this))
            {
                dataChangedManagedData.Enqueue(this);
            }

            // _target이 ISource에서 파생된 Type이라도 sourceType에서 파생된 Type일 경우 Manager.UpdateByInternal을 호출하지 않음
            if (_target is ISource targetAsSource &&
                !sourceType.IsAssignableFrom(_targetType) &&
                Broker.ContainsSourceType(_targetType))
            {
                Broker.UpdateByInternal(targetAsSource, dataChangedManagedData);
            }
        }

        void IManaged.UpdateTarget<TSource>(Type sourceType, in TSource source, Queue<IManaged> dataChangedManagedData)
        {
            if (!_targetUpdaters.TryGetValue(sourceType, out ICouple couple))
            {
                return;
            }

            try
            {
                if (couple is not ICouple<TSource> coupleTSource ||
                    !coupleTSource.UpdateTargetBySource(in source, _target))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);

                return;
            }

            if (!dataChangedManagedData.Contains(this))
            {
                dataChangedManagedData.Enqueue(this);
            }

            // _target이 ISource에서 파생된 Type이라도 sourceType에서 파생된 Type일 경우 Manager.UpdateByInternal을 호출하지 않음
            if (!sourceType.IsAssignableFrom(_targetType) &&
                Broker.ContainsSourceType(_targetType) &&
                _target is ISource targetAsSource)
            {
                Broker.UpdateByInternal(targetAsSource, dataChangedManagedData);
            }
        }

        void IManaged.OnChanged()
        {
            try
            {
                _subjectTarget?.OnNext(_target);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        void IManaged.AddCouple(Type sourceType, ICouple couple) => _targetUpdaters.Add(sourceType, couple);

        #endregion
    }

    /// <summary>
    /// internal use only
    /// </summary>
    internal interface IManaged : IDisposable
    {
        ITarget Target { get; }


        /// <summary>
        /// internal use only
        /// </summary>
        void UpdateTarget(Type sourceType, ISource source, Queue<IManaged> dataChangedManagedData);

        void UpdateTarget<TSource>(Type sourceType, in TSource source, Queue<IManaged> dataChangedManagedData) where TSource : ISource;

        /// <summary>
        /// internal use only
        /// </summary>
        void OnChanged();

        /// <summary>
        /// internal use only
        /// </summary>
        void AddCouple(Type sourceType, ICouple couple);
    }
}