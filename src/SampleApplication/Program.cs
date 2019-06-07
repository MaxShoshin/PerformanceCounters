using System.Diagnostics;
using System.ServiceModel;
using NeedfulThings.PerformanceCounters;
using NeedfulThings.PerformanceCounters.WebApi;

namespace SampleApplication
{
	using System;
	using System.Threading;
	using System.Web.Http.SelfHost;
	using Autofac.Integration.WebApi;
	using Autofac;

	class Program
	{
		static void Main(string[] args)
		{
			var config = new HttpSelfHostConfiguration("http://localhost:2707/");
		    config.UsePerformanceCounters("counters");

			var builder = new ContainerBuilder();
			builder.RegisterModule(new AutofacModule());

			var container = builder.Build();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);



			using (var server = new HttpSelfHostServer(config))
			{
			    try
			    {
                    server.OpenAsync().GetAwaiter().GetResult();
			    }
			    catch (AddressAccessDeniedException)
			    {
                    Console.WriteLine("Run this program under Administrator account or use the following command:");
                    Console.WriteLine(@"netsh http add urlacl url=http://+:2707/ user=Everyone");
			        return;
			    }

			    Process.Start("http://localhost:2707/counters");

				Console.WriteLine("Press any key to exit...");

				var sampleCounters = PerformanceCounterFactory.GetCounters<SampleCounters>();

                // simulate memory pressure
				var random = new Random();
				while (true)
				{
					sampleCounters.RequestsTotalCount.Increment();

					var n = random.Next(100);
					while (n-- > 0)
					{
						if (Console.KeyAvailable)
							return;

						var dummyString = n.ToString();
					}

					Thread.Sleep(100);
				}
			}
		}
	}
}
