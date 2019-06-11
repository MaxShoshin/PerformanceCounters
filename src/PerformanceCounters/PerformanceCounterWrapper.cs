namespace NeedfulThings.PerformanceCounters
{
    using System.Diagnostics;

    internal sealed class PerformanceCounterWrapper : IPerformanceCounter
    {
        private readonly PerformanceCounterProxyFactory _counterProxyFactory;
        private IPerformanceCounter _innerCounter;

        public PerformanceCounterWrapper(PerformanceCounterProxyFactory counterProxyFactory)
        {
            _counterProxyFactory = counterProxyFactory;
        }

        public string CounterName => _innerCounter.CounterName;

        public PerformanceCounterType CounterType => _innerCounter.CounterType;

        public float NextValue()
        {
            try
            {
                return _innerCounter.NextValue();
            }
            catch
            {
                ChangeCounter();

                return _innerCounter.NextValue();
            }
        }

        public void Increment()
        {
            try
            {
                _innerCounter.Increment();
            }
            catch
            {
                ChangeCounter();
            }
        }

        public void IncrementBy(long value)
        {
            try
            {
                _innerCounter.IncrementBy(value);
            }
            catch
            {
                ChangeCounter();
            }
        }

        public void Decrement()
        {
            try
            {
                _innerCounter.Decrement();
            }
            catch
            {
                ChangeCounter();
            }
        }

        public void Reset()
        {
            try
            {
                _innerCounter.Reset();
            }
            catch
            {
                ChangeCounter();
            }
        }

        public void ChangeCounter()
        {
            Initialize();
        }

        public void Dispose()
        {
            _innerCounter.Dispose();
        }

        public void Initialize()
        {
            _innerCounter = _counterProxyFactory.Create();
        }
    }
}