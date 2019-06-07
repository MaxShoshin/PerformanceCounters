namespace NeedfulThings.PerformanceCounters
{
	using System;
	using System.Diagnostics;

	[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	public sealed class PerformanceCounterCategoryAttribute : Attribute
	{
		public PerformanceCounterCategoryAttribute(string categoryName, string categoryHelp,
		                                           PerformanceCounterCategoryType categoryType,
		                                           InstanceNameType instanceNameType = InstanceNameType.None)
		{
			CategoryName = categoryName;
			CategoryHelp = categoryHelp;
			CategoryType = categoryType;
			InstanceNameType = instanceNameType;
		}

		public PerformanceCounterCategoryAttribute(
			string categoryName,
			PerformanceCounterCategoryType categoryType,
			InstanceNameType instanceNameType = InstanceNameType.None)
		{
			CategoryName = categoryName;
			CategoryType = categoryType;
			InstanceNameType = instanceNameType;
		}

		public string CategoryName { get; }

		public string CategoryHelp { get; }

		public PerformanceCounterCategoryType CategoryType { get; }

		public InstanceNameType InstanceNameType { get; }
	}
}