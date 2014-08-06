using NUnit.Framework;
using System;
using System.Linq;
using Artefacts.Service;
using System.ServiceModel;
using Artefacts.FileSystem;

using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace Artefacts.TestClient
{
	/// <summary>
	/// Client tests.
	/// </summary>
	[TestFixture]
	public class ClientTestFixture : IDisposable
	{
		#region Private fields
		private int _init = 0;
		private int _exit = 0;
		private readonly TimeSpan _defaultTimeout = new TimeSpan(0, 0, 10);
		private const int _serviceHostStartDelay = 2200;
		private const int _serviceHostStopTimeout = 2200;
		private readonly Type[] _artefactTypes = new Type[]
			#region Artefact known types
			{
				typeof(Artefacts.Artefact),
				typeof(Artefacts.Host),
				typeof(Artefacts.FileSystem.Directory),
				typeof(Artefacts.FileSystem.Disk),
				typeof(Artefacts.FileSystem.Drive),
				typeof(Artefacts.FileSystem.FileSystemEntry),
				typeof(Artefacts.FileSystem.File),
			};
			#endregion

		private Process _serviceHostProcess = null;
		private Thread _serviceHostThread = null;
		private const string _serviceHostLogFilePath = @"ServiceHost.Log";
		private readonly FileStream _serviceHostLog = null;
		private TextWriter _shLogWriter = null;

		private RepositoryClientProxy _clientProxy = null;
		private FileSystemArtefactCreator _fsCreator = null;
//		private static ChannelFactory<IArtefactService> _proxyFactory = null;
//		private static ChannelFactory<IRepository<Artefact>> _repoProxyFactory = null;
//		private static IArtefactService _proxy = null;
//		private static IRepository<Artefact> _repoProxy = null;
//		private static RepositoryClientProxy<Artefact> _clientProxy = null;
		#endregion

		protected TextWriter ConsoleOut = null;
		protected TextWriter ConsoleError = null;
		protected readonly bool UseServiceHostAsync = true;
		protected readonly bool UseServiceHostProc = false;
		
		#region Construction & disposal
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.TestClient.ClientTests"/> class.
		/// </summary>
		public ClientTestFixture(bool init = true)
		{
			if (init)
				Init();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Artefacts.TestClient.ClientTestFixture"/> object.
		/// </summary>
		/// <remarks>
		/// IDisposable implementation
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="Artefacts.TestClient.ClientTestFixture"/>.
		/// The <see cref="Dispose"/> method leaves the <see cref="Artefacts.TestClient.ClientTestFixture"/> in an unusable
		/// state. After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Artefacts.TestClient.ClientTestFixture"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Artefacts.TestClient.ClientTestFixture"/> was occupying.
		/// </remarks>
		public void Dispose()
		{
			Exit();	
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			Console.Write("ClientTest.Init: ");
			if (Thread.VolatileRead(ref _init) != 0)
				Console.WriteLine("Already initialised");
			else
			{
				Console.WriteLine("Initialising...");
				Thread.VolatileWrite(ref _init, 1);
				Artefact.ArtefactTypes.AddRange(_artefactTypes);
				
				if (UseServiceHostAsync)
				{
					_serviceHostThread = ArtefactHost.GetOrCreateAsyncThread(_artefactTypes, _defaultTimeout);
//					_shLogWriter = new LogTextWriter(_serviceHostLogFilePath);
					Thread.Sleep(_serviceHostStartDelay);
				}
				else if (UseServiceHostProc)
				{
					if (!ArtefactHost.IsRunning("ServiceHost.exe"))
					{
						IEnumerable<string> args =
							_artefactTypes.Select<Type, string>((T) => string.Concat("-A", Path.GetFileName(T.Assembly.Location))).Distinct()
							.Concat(_artefactTypes.Select<Type, string>((T) => string.Concat("-T", T.FullName)));
						_serviceHostProcess = ArtefactHost.ExecuteViaCommandLine("ServiceHost.exe", string.Join(" ", args));
//						AppDomain.CurrentDomain.ExecuteAssembly("ServiceHost.exe", args.ToArray());
						Thread.Sleep(_serviceHostStartDelay);
					}
				}
				
				_clientProxy = new RepositoryClientProxy(new NetTcpBinding(), "net.tcp://localhost:3334/ArtefactRepository");
				Console.WriteLine("\nService Artefact Repository: {0}\n", _clientProxy.ToString());
				_fsCreator = new FileSystemArtefactCreator(_clientProxy)
				{
					BasePath = "/media/Scarydoor/mystuff/moozik/samples/mycollections/"
				};		//_clientProxy.Artefacts
				Console.WriteLine("\nFS Creator: {0}\n", _fsCreator.ToString());
			}
		}

		/// <summary>
		/// Exit this instance.
		/// </summary>
		public void Exit()
		{
			Console.Write("ClientTest.Exit: ");
			if (Thread.VolatileRead(ref _exit) != 0)
				Console.WriteLine("Already exited");
			else
			{
				Console.WriteLine("Exiting...");
				Thread.VolatileWrite(ref _exit, 1);
				if (_serviceHostThread != null)// && _serviceHostThread.IsAlive)
				{
					Console.Write("\tStopping service host thread... ");
					ArtefactHost.StopAsyncThread();
					if (_serviceHostThread.Join(_serviceHostStopTimeout))
						Console.WriteLine("done.");
					else
						Console.WriteLine("Error!");
				}
				else if (_serviceHostProcess != null)
				{
					Console.Write("\tStopping service host process... ");
					_serviceHostProcess.Close();
					if (_serviceHostProcess.WaitForExit(_serviceHostStopTimeout))		// TODO: Check this works (haven't tested yet)
						Console.WriteLine("done.");
					else
						Console.WriteLine("Error!");
				}
				if (_shLogWriter != null)
				{
					Console.Write("\tClosing service host log... ");
					_shLogWriter.Flush();
					_shLogWriter.Close();//.Dispose();
					_shLogWriter = null;
					Console.WriteLine("done.");
				}
			}
		}
		#endregion

		#region Test methods
		/// <summary>
		/// Queries all artefacts.
		/// </summary>
		[Test, TestMethod(Order=10, Name="IQueryable<Artefact> _clientProxy.Artefacts")]
		public void QueryAllArtefacts()
		{
			Console.WriteLine("{0} artefacts currently in repository", _clientProxy.Artefacts.Count());
//			foreach (Artefact artefact in _clientProxy.Artefacts)
//				Console.WriteLine(artefact.ToString());
//					string.Format("{0}: Id={1} TimeCreated={2} TimeUpdated={3} TimeChecked={4}",
//					artefact.GetType().Name, artefact.Id, artefact.TimeCreated, artefact.TimeUpdated, artefact.TimeChecked));
		}

		/// <summary>
		/// Tests the query artefacts_ linq_ statement.
		/// </summary>
//		[Test, TestMethod(Order=20, Name="IEnumerator<Artefact> _clientProxy.Artefacts using LINQ statement")]
		public void TestQueryArtefacts_Linq_Statement()
		{
			var q = from a in _clientProxy.Artefacts
				        where a.Id > 32799
			        select a;
			foreach (Artefact artefact in q)
				Console.WriteLine(artefact.ToString());
		}

		/// <summary>
		/// Tests the query artefacts_ linq_ method.
		/// </summary>
//		[Test, TestMethod(Order=30, Name="IEnumerator<Artefact> _clientProxy.Artefacts using LINQ method syntax")]
		public void TestQueryArtefacts_Linq_Method()
		{
			var q = _clientProxy.Artefacts.Where((a) => a.Id > 32799);
			foreach (Artefact artefact in q)
				Console.WriteLine(artefact.ToString());
		}

		[Test, TestMethod(Order=05, Name="Host.Current static property")]
		public void TestHostArtefactCurrent()
		{
			Console.WriteLine(Host.Current.ToString());
		}
		
//		[Test, TestMethod(Order=50, Name="int _clientProxy.Artefacts.Count()")]
		public void TestQueryArtefactsDrivesCount()
		{
			Console.WriteLine("{0} artefacts currently in repository\n{1} drives",
				_clientProxy.Artefacts.Count(), _fsCreator.Drives.Count());
		}

		/// <summary>
		/// Tests the file system artefact creator.
		/// </summary>
		[Test, TestMethod(Order=40, Name="FileSystemArtefactCreator")]
		public void TestFileSystemArtefactCreator()
		{
			Console.WriteLine("{0} artefacts currently in repository\n{1} drives", _clientProxy.Artefacts.Count(), _fsCreator.Drives.Count());
//				var q = _fsCreator.Drives.Where((a) => a.Id > 32799);
			//a.GetType().FullName == typeof(Drive).FullName && 
			foreach (Drive artefact in _fsCreator.Drives)
				Console.WriteLine(artefact.ToString());
			_fsCreator.Run(null);
		}
		#endregion
	}
}
