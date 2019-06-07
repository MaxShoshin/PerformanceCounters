using System.Linq;
using System.Reflection;

namespace NeedfulThings.PerformanceCounters
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	public static class PerformanceCounterFactory
	{
		private static readonly Dictionary<Type, IPerformanceCounterSet> _counters =
			new Dictionary<Type, IPerformanceCounterSet>();

		public static PerformanceCounterInstaller GetInstallerFor<T>() where T : IPerformanceCounterSet
		{
			var category = Helper.GetCategoryAttribute(typeof (T));
			if (category == null)
			{
				var message = string.Format("Type '{0}' should be marked with PerformanceCounterCategoryAttribute", typeof (T));
				throw new ArgumentException(message);
			}

			var installer = new PerformanceCounterInstaller
			{
				CategoryName = category.CategoryName,
				CategoryHelp = category.CategoryHelp,
				CategoryType = category.CategoryType
			};

			foreach (var propertyInfo in typeof (T).GetProperties())
			{
				var counterCreationData = Helper.GetCounterCreationData(propertyInfo);
				if (counterCreationData == null)
					continue;

				installer.Counters.Add(counterCreationData);
			}

			return installer;
		}

		public static T GetCounters<T>(string instanceName = null) where T : class, IPerformanceCounterSet
		{
			lock (_counters)
			{
				IPerformanceCounterSet counterSet;
				if (!_counters.TryGetValue(typeof (T), out counterSet))
				{
					var counterSetType = CounterSetTypeEmitter.GeneratePerformanceCounterSetImplementation(typeof(T));
					var category = Helper.GetCategoryAttribute(typeof(T));
					var performanceCounters = GetPerformanceCounters<T>(instanceName);

					var arguments = new object[performanceCounters.Count + 2];
					arguments[0]= category.CategoryName;
					arguments[1]= performanceCounters;
					for (int i = 0; i < performanceCounters.Count; i++)
					{
						arguments[i + 2] = performanceCounters[i];
					}

					counterSet = (T)Activator.CreateInstance(counterSetType, arguments.ToArray());

					_counters.Add(typeof (T), counterSet);
				}

				return (T) counterSet;
			}
		}

		private static IReadOnlyList<IReadOnlyPerformanceCounter> GetPerformanceCounters<T>(string instanceName)
		{
			var type = typeof (T);
			var categoryAttribute = Helper.GetCategoryAttribute(type);
			if (categoryAttribute == null)
			{
				throw new ArgumentException();
			}

			var counters = new List<IReadOnlyPerformanceCounter>();
			foreach (var propertyInfo in type.GetProperties())
			{
				var counterAttribute = GetCounterAttribute(propertyInfo);
				if (counterAttribute == null)
				{
					continue;
				}

				var getMethod = propertyInfo.GetGetMethod();
				if (getMethod == null)
				{
					throw new InvalidProgramException();
				}

				var counter = GetInstance(categoryAttribute, counterAttribute,
					propertyInfo.PropertyType == typeof (IReadOnlyPerformanceCounter),
					instanceName);

				counters.Add(counter);
			}

			return counters;
		}

		private static IPerformanceCounter GetInstance(PerformanceCounterCategoryAttribute categoryAttribute,
			PerformanceCounterAttribute counterAttribute,
			bool readOnly,
			string customInstanceName)
		{
			var categoryName = categoryAttribute.CategoryName;
			var categoryType = categoryAttribute.CategoryType;
			var counterName = counterAttribute.CounterName;
			var counterType = counterAttribute.CounterType;
			var instanceNameType = categoryAttribute.InstanceNameType;

			try
			{
				if (PerformanceCounterCategory.Exists(categoryName) &&
				    PerformanceCounterCategory.CounterExists(counterName, categoryName))
				{
					var counter = new PerformanceCounter
					{
						CategoryName = categoryName,
						CounterName = counterName,
						InstanceName = categoryType == PerformanceCounterCategoryType.SingleInstance
							? string.Empty
							: customInstanceName ?? GetInstanceName(),
						ReadOnly = readOnly,
					};

					if (categoryType == PerformanceCounterCategoryType.MultiInstance && !readOnly)
					{
						counter.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
					}

					return new PerformanceCounterWrapper(new PerformanceCounterProxy(counter));
				}
			}
			catch
			{
			}

			return new NullPerformanceCounter(counterName, counterType);
		}

		private static string GetInstanceName()
		{
			var process = Process.GetCurrentProcess();

			return $"{process.ProcessName}#{process.Id}";
		}

		private static PerformanceCounterAttribute GetCounterAttribute(PropertyInfo propertyInfo)
		{
			var attribute =
				(PerformanceCounterAttribute)
				propertyInfo.GetCustomAttributes(typeof (PerformanceCounterAttribute), false).FirstOrDefault();

			return attribute;
		}
	}
}