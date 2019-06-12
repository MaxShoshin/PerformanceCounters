using System;
using System.Diagnostics;

namespace NeedfulThings.PerformanceCounters
{
    internal sealed class PerformanceCounterProxyFactory
    {
        private readonly PerformanceCounterCategoryAttribute _categoryDescription;
        private readonly PerformanceCounterAttribute _counterDescription;
        private readonly bool _readOnly;
        private readonly string _customInstanceName;
        private readonly InstanceNameProvider _instanceNameProvider;

        public PerformanceCounterProxyFactory(
            PerformanceCounterCategoryAttribute categoryDescription,
            PerformanceCounterAttribute counterDescription,
            InstanceNameProvider instanceNameProvider,
            bool readOnly,
            string customInstanceName)
        {
            _categoryDescription = categoryDescription ?? throw new ArgumentNullException(nameof(categoryDescription));
            _counterDescription = counterDescription ?? throw new ArgumentNullException(nameof(counterDescription));
            _instanceNameProvider = instanceNameProvider ?? throw new ArgumentNullException(nameof(instanceNameProvider));
            _readOnly = readOnly;
            _customInstanceName = customInstanceName;
        }

        public IPerformanceCounter Create()
        {
            var categoryName = _categoryDescription.CategoryName;
            var categoryType = _categoryDescription.CategoryType;
            var counterName = _counterDescription.CounterName;
            var counterType = _counterDescription.CounterType;
            var instanceNameType = _categoryDescription.InstanceNameType;

            _instanceNameProvider.Validate(categoryType, instanceNameType, _readOnly);

            try
            {
                if (PerformanceCounterCategory.Exists(categoryName) &&
                    PerformanceCounterCategory.CounterExists(counterName, categoryName))
                {
                    var counter = new PerformanceCounter()
                    {
                        CategoryName = categoryName,
                        CounterName = counterName,
                        InstanceName = categoryType == PerformanceCounterCategoryType.SingleInstance
                            ? string.Empty
                            : _customInstanceName ?? _instanceNameProvider.GetInstanceName(instanceNameType),
                        ReadOnly = _readOnly
                    };

                    if (categoryType == PerformanceCounterCategoryType.MultiInstance && !_readOnly)
                    {
                         counter.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
                    }

                    return new PerformanceCounterProxy(counter);
                }
            }
            catch
            {
            }

            return new NullPerformanceCounter(counterName, counterType);
        }
    }
}