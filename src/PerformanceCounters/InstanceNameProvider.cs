using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NeedfulThings.PerformanceCounters
{
    internal sealed class InstanceNameProvider
    {
        private readonly Process _process = Process.GetCurrentProcess();

        public string GetInstanceName(InstanceNameType instanceNameType)
        {
            switch (instanceNameType)
            {
                case InstanceNameType.None:
                    return _process.ProcessName;
                case InstanceNameType.SystemProcess:
                    return GetInstanceName("Process", "ID Process");
                case InstanceNameType.DotNetProcess:
                    return GetInstanceName(".NET CLR Memory", "Process ID");
                case InstanceNameType.ProcessWithId:
                    return $"{_process.ProcessName}_{_process.Id}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(instanceNameType), instanceNameType, null);
            }
        }

        private string GetInstanceName(string categoryName, string counterName)
        {
            try
            {
                var category = new PerformanceCounterCategory(categoryName);
                var instances = category.GetInstanceNames().Where(i => i.StartsWith(_process.ProcessName, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var instance in instances)
                {
                    using (var counter = new PerformanceCounter(categoryName, counterName, instance, true))
                    {
                        if ((int) counter.RawValue == _process.Id)
                        {
                            return instance;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Validate(PerformanceCounterCategoryType categoryType, InstanceNameType instanceNameType, bool readOnly)
        {
            if (readOnly || categoryType == PerformanceCounterCategoryType.SingleInstance)
            {
                return;
            }

            if (instanceNameType == InstanceNameType.None || instanceNameType == InstanceNameType.ProcessWithId)
            {
                return;
            }

            var message = string.Format(
                CultureInfo.InvariantCulture,
                "Instance name type {0} is not supported for multi instance PerformanceCounter. Supports only ProcessWithId or None.",
                instanceNameType);

            throw new NotSupportedException(message);
        }
    }
}