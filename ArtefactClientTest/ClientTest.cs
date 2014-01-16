using System;
using System.Collections;
using System.Collections.Generic;
using TextWriter=System.IO.TextWriter;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

//using Serialize.Linq.Extensions;
//using Serialize.Linq.Nodes;

using Artefacts;
using Artefacts.Services;
using Artefacts.FileSystem;

using NHibernate.Criterion;
using NHibernate.Linq;

namespace ArtefactClientTest
{
	/// <summary>
	/// Artefact client test.
	/// </summary>
	/// <remarks>
	/// "value(Artefacts.Services.Queryable`1[Artefacts.Artefact]).Where(a => (a.TimeCreated.Ticks > new DateTime(2013, 12, 21, 7, 44, 40).Ticks))"
	/// </remarks>
	class ArtefactClientTest
	{
		class ArtefactAdder : IReceiver
		{
			private bool _completed = false;

			#region IObserver implementation
			public void OnCompleted()
			{
				_completed = true;
			}

			public void OnError(Exception error)
			{
				Console.WriteLine("ArtefactAdder: " + error.ToString() + "\n");
			}

			public void OnNext(Artefact value)
			{
				_clientProxy.Add(value);
//				_repoProxy.Add(value);
//				value.Id = _repoProxy.Add(value);
//				_repoProxy.Update(value);
			}
			#endregion
		}
		
		#region Constants
		private static TimeSpan DefaultTimeout = new TimeSpan(0, 0, 10);
		private const int ServiceHostStartDelay = 1800;
		private static Type[] artefactTypes = new Type[]
			#region Artefact known types
			{
//				typeof(NHibernate.Linq.NhQueryable<Artefact>),
	// shouldn't be in Artefact types, just temporary to pass it to known types in data contract serializer c'tor
				typeof(Artefact),
				typeof(Drive),
				typeof(FileSystemEntry),
				typeof(File),
				typeof(Directory)
			};
			#endregion
		#endregion
		
		#region Private static fields
		private static Thread _serviceHostThread = null;
//		private static ChannelFactory<IArtefactService> _proxyFactory = null;
//		private static ChannelFactory<IRepository<Artefact>> _repoProxyFactory = null;
//		private static IArtefactService _proxy = null;
//		private static IRepository<Artefact> _repoProxy = null;
		private static RepositoryClientProxy<Artefact> _clientProxy = null;
		private static ICreator _fsCreator = null;
		#endregion
		
		#region Debug Writer Constants
//		private const string _runTestOutputPrefix = "\n";
		private const string _runTestOutputNewLine = "\n";		//"\n# ";
		private const string _runTestFormatStart = "\n########\n# RunTest: Start: {0}\n########\n";
		private const string _runTestFormatSuccess = "\n########\n# RunTest: Success: {0}\n########\n";
		private const string _runTestOutputUnknownError = "\n########\n# RunTest: Failed: {0}\n########\n";
		private const string _runTestFormatError = "\n########\n# RunTest: Failed: {0}\n########\n{2}\n";
//		private const string _runTestOutputSuffix = "\n";
		#endregion
		
		#region RunTest methods
		protected static void RunTests(TextWriter output = null)
		{
			(typeof(ArtefactClientTest).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(mi => mi.GetCustomAttributes(typeof(ClientTestMethodAttribute), false).Length > 0)
				.OrderBy<MethodInfo, int>(
					mi => (mi.GetCustomAttributes(typeof(ClientTestMethodAttribute), false)[0] as ClientTestMethodAttribute).Order))
				.ToList().ForEach(mi => RunTest(
					(mi.GetCustomAttributes(typeof(ClientTestMethodAttribute), false)[0] as ClientTestMethodAttribute).Name,
					() => mi.Invoke(null, new object[] { })));
		}
		protected static void RunTest(string name, Action testMethod, TextWriter output = null)
		{
			if (output == null)
				output = Console.Out;
			string _oldNL = output.NewLine;
			try
			{
				output.WriteLine(_runTestFormatStart, name);
				testMethod();
				output.WriteLine(_runTestFormatSuccess, name);
			}
			catch (Exception ex)
			{
				output.WriteLine(_runTestFormatError, name, ex.GetType().FullName, ex.ToString());
			}
			finally
			{
				output.NewLine = _oldNL;			// you COULD wrap this in a using() if you write a class implementing IDisposable with this line in Dispose()
			}
		}
		protected static bool RunTest(string name, Func<bool> testMethod, TextWriter output = null)
		{
			if (output == null)
				output = Console.Out;
			string _oldNL = output.NewLine;
			try
			{
				output.WriteLine(_runTestFormatStart, name);
				if (!testMethod())
					output.WriteLine(_runTestOutputUnknownError, name);
				output.WriteLine(_runTestFormatSuccess, name);
				return true;
			}
			catch (Exception ex)
			{
				output.WriteLine(_runTestFormatError, name, ex.GetType().FullName, ex.ToString());
				return false;
			}
			finally
			{
				output.NewLine = _oldNL;			// you COULD wrap this in a using() if you write a class implementing IDisposable with this line in Dispose()
			}
		}
		#endregion
		
		#region Test methods executed by RunTest
//		[ClientTestMethod(Order=1, Name="IEnumerator<Artefact> RepositoryClientProxy<Artefact>.Artefacts")]
		private static void TestQueryAllArtefacts()
		{
			Console.WriteLine("{0} artefacts currently in repository", _clientProxy.Artefacts.Count());
			foreach (Artefact artefact in _clientProxy.Artefacts)
				Console.WriteLine(artefact.ToString());
		}

		[ClientTestMethod(Order=2, Name="IEnumerator<Artefact> RepositoryClientProxy<Artefact>.Artefacts")]
		private static void TestQueryArtefactsEN()
		{
//			var q = from a in _clientProxy.Artefacts
//				where a.Id > 512
//					select a;
			
//			var q = _clientProxy.Artefacts.Where((a) => true).Select<Artefact, Artefact>((a) => a);//(a) => a.Id > 512);
			
			foreach (Artefact artefact in _clientProxy.Artefacts) //q)
				Console.WriteLine(artefact.ToString());
		}
		
//		[ClientTestMethod(Order=3, Name="FileSystemArtefactCreator")]
		private static void TestFileSystemArtefactCreator()
		{
			_fsCreator = new FileSystemArtefactCreator(_clientProxy)
			{
				BaseUri = new Uri("file:///media/Scarydoor/mystuff/moozik/samples/mycollections/")
			};
			_fsCreator.Run(null);
		}
		#endregion
		
		protected static void Init()
		{
			Console.ReadKey();

			Artefact.ArtefactTypes.AddRange(artefactTypes);
			
			// Start service host thread and pause for a few seconds to ensure it has started
			_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread();		//artefactTypes);
			_serviceHostThread.Start();
			Thread.Sleep(ServiceHostStartDelay);
			
			_clientProxy = new RepositoryClientProxy<Artefact>(new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3334/ArtefactRepository");
//			DataContractResolver resolver = new WCFTypeResolver();
				
//			_proxyFactory = new ChannelFactory<IArtefactService>(
//				new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3333/ArtefactService");
//			_proxyFactory.Endpoint.Binding.CloseTimeout = DefaultTimeout;
//			_proxyFactory.Endpoint.Binding.OpenTimeout = DefaultTimeout;
//			_proxyFactory.Endpoint.Binding.ReceiveTimeout = DefaultTimeout;
//			_proxyFactory.Endpoint.Binding.SendTimeout = DefaultTimeout;
////			ArtefactService.AddTypeResolver(_proxyFactory.Endpoint, resolver);
//			_proxy = _proxyFactory.CreateChannel();//_proxyFactory.Endpoint);// new EndpointAddress());
//			_repoProxyFactory = new ChannelFactory<IRepository<Artefact>>(
//				new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3334/ArtefactRepository");
//			_repoProxyFactory.Endpoint.Binding.CloseTimeout = DefaultTimeout;
//			_repoProxyFactory.Endpoint.Binding.OpenTimeout = DefaultTimeout;
//			_repoProxyFactory.Endpoint.Binding.ReceiveTimeout = DefaultTimeout;
//			_repoProxyFactory.Endpoint.Binding.SendTimeout = DefaultTimeout;
//			ArtefactService.AddTypeResolver(_repoProxyFactory.Endpoint, resolver);
//			_repoProxy = _repoProxyFactory.CreateChannel();

			Console.WriteLine("\nService Artefact Repository: {0}\n", _clientProxy.ToString());
		}
		
		public static void Main(string[] args)
		{			
			try
			{
				Init();
				RunTests();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("\nException: " + ex.ToString());
			}
			finally
			{
				Console.WriteLine("\nExiting...");
				if (_serviceHostThread != null)
				{
					Console.WriteLine("Stopping service host thread... ");
					ArtefactServiceHost.StopAsyncThread();
					if (_serviceHostThread.Join(3880))
						Console.WriteLine("done.");
					else
						Console.WriteLine("Error!");
				}
				else
					Console.WriteLine("Service host thread null");
			}
		}
	}
}
