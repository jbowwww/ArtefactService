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
using Artefacts.Service;
using Artefacts.FileSystem;

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
				private static FileSystemArtefactCreator/*ICreator*/ _fsCreator = null;
		#endregion
		
		#region Debug Writer Constants
//		private const string _runTestOutputPrefix = "\n";
		private const string _runTestOutputNewLine = "\n";		//"\n# ";
		private const string _runTestFormatStart = "\n########\n# RunTest: Start: {0}\n########\n";
		private const string _runTestFormatSuccess = "\n########\n# RunTest: Success: {0}\n########\n";
		private const string _runTestOutputUnknownError = "\n########\n# RunTest: Failed: {0}\n########\n";
		private const string _runTestFormatError = "\n########\n# RunTest: Failed: {0}\n########\n";
		private const string _runTestFormatException = "{0}: {1}\n{2}\n";
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
			catch (FaultException<ExceptionDetail> ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				int indent = 0;
				for (ExceptionDetail detail = ex.Detail; detail != null; detail = detail.InnerException)
					output.WriteLine(string.Concat(" ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, detail.Type, detail.Message, detail.StackTrace))
							.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
			}
			catch (Exception ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				int indent = 0;
				for (Exception innerEx = ex.InnerException; innerEx != null; innerEx = innerEx.InnerException)
					output.WriteLine(string.Concat("\n ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, innerEx.GetType().FullName, innerEx.Message, innerEx.StackTrace))
							.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
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
			catch (FaultException<ExceptionDetail> ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				int indent = 0;
				for (ExceptionDetail detail = ex.Detail; detail != null; detail = detail.InnerException)
					output.WriteLine(string.Concat(" ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, detail.Type, detail.Message, detail.StackTrace))
							.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
				return false;
			}
			catch (Exception ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				int indent = 0;
				for (Exception innerEx = ex.InnerException; innerEx != null; innerEx = innerEx.InnerException)
					output.WriteLine(string.Concat("\n ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, innerEx.GetType().FullName, innerEx.Message, innerEx.StackTrace))
							.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
				return false;
			}
			finally
			{
				output.NewLine = _oldNL;			// you COULD wrap this in a using() if you write a class implementing IDisposable with this line in Dispose()
			}
		}
		#endregion
		
		#region Test methods executed by RunTest
		[ClientTestMethod(Order=5, Name="int RepositoryClientProxy<Artefact>.Artefacts.Count()")]
		private static void TestQueryArtefactsCount()
		{
			Console.WriteLine("{0} artefacts currently in repository", _clientProxy.Artefacts.Count());
		}
		
//		[ClientTestMethod(Order=10, Name="IQueryable<Artefact> RepositoryClientProxy<Artefact>.Artefacts")]
		private static void TestQueryAllArtefacts()
		{
			foreach (Artefact artefact in _clientProxy.Artefacts)
				Console.WriteLine(artefact.ToString());
		}

//		[ClientTestMethod(Order=20, Name="IEnumerator<Artefact> RepositoryClientProxy<Artefact>.Artefacts using LINQ statement")]
		private static void TestQueryArtefacts_Linq_Statement()
		{
			var q = from a in _clientProxy.Artefacts
				where a.Id > 32799
					select a;
			foreach (Artefact artefact in q)
				Console.WriteLine(artefact.ToString());
		}
		
//		[ClientTestMethod(Order=30, Name="IEnumerator<Aertefact> RepositoryClientProxy<Artefact>.Artefacts using LINQ method syntax")]
		private static void TestQueryArtefacts_Linq_Method()
		{
			var q = _clientProxy.Artefacts.Where((a) => a.Id > 32799);
			foreach (Artefact artefact in q)
				Console.WriteLine(artefact.ToString());
		}

//				[ClientTestMethod(Order=35, Name="int RepositoryClientProxy<Artefact>.Artefacts.Count()")]
				private static void TestQueryArtefactsDrivesCount()
				{
						Console.WriteLine("{0} artefacts currently in repository\n{1} drives", _clientProxy.Artefacts.Count(), _fsCreator.Drives.Count());

				}
					
		[ClientTestMethod(Order=40, Name="FileSystemArtefactCreator")]
		private static void TestFileSystemArtefactCreator()
		{
			_fsCreator = new FileSystemArtefactCreator(_clientProxy)
			{
				BasePath = "/media/Scarydoor/mystuff/moozik/samples/mycollections/"
						};		//_clientProxy.Artefacts

						Console.WriteLine("{0} artefacts currently in repository\n{1} drives", _clientProxy.Artefacts.Count(), _fsCreator.Drives.Count());

						var q = _fsCreator.Drives.Where((a) => a.Id > 32799);
						//a.GetType().FullName == typeof(Drive).FullName && 
						foreach (Artefact artefact in q)
								Console.WriteLine(artefact.ToString());
			_fsCreator.Run(null);
		}
		#endregion
		
		protected static void Init()
		{
//			Console.ReadKey();

			Artefact.ArtefactTypes.AddRange(artefactTypes);
			
			// Start service host thread and pause for a few seconds to ensure it has started
			_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread();		//artefactTypes);
			_serviceHostThread.Start();
			Thread.Sleep(ServiceHostStartDelay);
//						ArtefactServiceHost.Main(null);
//						Thread.Sleep(ServiceHostStartDelay);

			_clientProxy = new RepositoryClientProxy<Artefact>(new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3334/ArtefactRepository");
			
			Console.WriteLine("\nService Artefact Repository: {0}\n", _clientProxy.ToString());
		}
		
		public static void Main(string[] args)
		{
			Init();
			RunTests();
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
