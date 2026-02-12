using System;
using UnityEngine;
using R3;
using R3.Triggers;

namespace CrowRx
{
    using Data;
    using Data.Bind;


    public static class MonoBehaviourExtension
    {
        public static IDisposable BindEvent<T>(this MonoBehaviour monoBehaviour, Action<T> onUpdate, UnityEventBind eventBind)
            where T : class, ITarget, new()
            => new MonoBehaviourBinder<T>(onUpdate, monoBehaviour, eventBind);
    }
}

namespace CrowRx.Data.Bind
{
    public enum UnityEventBind
    {
        AwakeAndDestroy,
        EnableAndDisable,
    }

    public class MonoBehaviourBinder<TTarget> : Binder<TTarget>
        where TTarget : class, ITarget, new()
    {
        private readonly WeakReference<MonoBehaviour> _weakReference;

        private readonly UnityEventBind _eventBind;

        private IDisposable _disposableBind, _disposableUnbind;


        private MonoBehaviour BindMonoBehaviour => _weakReference.TryGetTarget(out var monoBehaviour) ? monoBehaviour : null;


        public MonoBehaviourBinder(Action<TTarget> onUpdate) : base(onUpdate)
        {
        }

        public MonoBehaviourBinder(Action<TTarget> onUpdate, MonoBehaviour monoBehaviour, UnityEventBind eventBind)
            : this(onUpdate)
        {
            _weakReference = new WeakReference<MonoBehaviour>(monoBehaviour);

            _eventBind = eventBind;

            AttachEventToMonoBehaviour();
        }


        public override void Dispose()
        {
            Unbind();

            DetachEventFromMonoBehaviour();

            _weakReference.SetTarget(null);
        }

        private void AttachEventToMonoBehaviour()
        {
            MonoBehaviour bindMonoBehaviour = BindMonoBehaviour;
            if (!bindMonoBehaviour)
            {
                return;
            }

            switch (_eventBind)
            {
                case UnityEventBind.AwakeAndDestroy:
                    Bind();
                    bindMonoBehaviour.OnDestroyAsObservable().Subscribe(_ => Dispose());
                    break;

                case UnityEventBind.EnableAndDisable:
                    if (bindMonoBehaviour.isActiveAndEnabled)
                    {
                        Bind();
                    }

                    _disposableBind = bindMonoBehaviour.OnEnableAsObservable().Subscribe(_ => Bind());
                    _disposableUnbind = bindMonoBehaviour.OnDisableAsObservable().Subscribe(_ => Unbind());

                    bindMonoBehaviour.OnDestroyAsObservable().Subscribe(_ => Dispose());
                    break;
            }
        }

        private void DetachEventFromMonoBehaviour()
        {
            _disposableBind?.Dispose();
            _disposableBind = null;

            _disposableUnbind?.Dispose();
            _disposableUnbind = null;
        }
    }
}