using System;
using System.Diagnostics;
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
                case InstanceNameType.ProcessHash:
                    return $"{_process.ProcessName}#{_process.Id}";
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
            catch (Exception e)
            {
                return null;
            }
        }
    }
}