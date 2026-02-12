using System;
using R3;


namespace CrowRx.Data.Bind
{
    public abstract class Binder<TTarget> : IDisposable
        where TTarget : class, ITarget, new()
    {
        private readonly Action<TTarget> _onUpdate;

        private IDisposable? _disposableBind;


        public bool IsBind => _disposableBind is not null;


        protected Binder(Action<TTarget> onUpdate)
        {
            _onUpdate = onUpdate;
        }


        public bool Bind()
        {
            if (IsBind)
            {
                return false;
            }

            _disposableBind = Managed<TTarget>.Observable.Subscribe(_onUpdate);

            return true;
        }

        public bool Unbind()
        {
            if (IsBind)
            {
                _disposableBind?.Dispose();
                _disposableBind = null;

                return true;
            }

            return false;
        }

        public abstract void Dispose();
    }
}