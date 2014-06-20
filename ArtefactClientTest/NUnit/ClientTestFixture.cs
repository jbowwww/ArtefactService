using NUnit.Framework;
using System;
using System.Linq;
using Artefacts.Service;
using System.ServiceModel;
using Artefacts.FileSystem;

using System.IO;
using System.Threading;

namespace Artefacts.TestClient
{
	/// <summary>
	/// Client tests.
	/// </summary>
	[TestFixture]
	public class ClientTestFixture : IDisposable
	{
		#region Private static fields
		private int _init = 0;
		private int _exit = 0;
		private TimeSpan _defaultTimeout = new TimeSpan(0, 0, 10);
		private int _serviceHostStartDelay = 1000;
		private int _serviceHostStopTimeout = 2200;
		private Type[] _artefactTypes = new Type[]
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

		private Thread _serviceHostThread = null;
		private string _serviceHostLogFilePath = @"		ServiceHost.Log";
		private FileStream _serviceHostLog = null;
		private TextWriter _shLogWriter = null;

		private RepositoryClientProxy _clientProxy = null;
		private FileSystemArtefactCreator _fsCreator = null;
//		private static ChannelFactory<IArtefactService> _proxyFactory = null;
//		private static ChannelFactory<IRepository<Artefact>> _repoProxyFactory = null;
//		private static IArtefactService _proxy = null;
//		private static IRepository<Artefact> _repoProxy = null;
//		private static RepositoryClientProxy<Artefact> _clientProxy = null;
		#endregion

		#region Construction & disposal
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.TestClient.ClientTests"/> class.
		/// </summary>
		public ClientTestFixture()
		{
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
				_shLogWriter = new LogTextWriter(_serviceHostLogFilePath);
				_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread(_artefactTypes, _defaultTimeout, _shLogWriter);
				_serviceHostThread.Start();
				Thread.Sleep(_serviceHostStartDelay);
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
					ArtefactServiceHost.StopAsyncThread();
					if (_serviceHostThread.Join(_serviceHostStopTimeout))
						Console.WriteLine("done.");
					else
						Console.WriteLine("Error!");
				}
				else
					Console.WriteLine("\tService host thread null!");
				Console.Write("\tClosing service host log... ");
				if (_shLogWriter != null)
				{
					_shLogWriter.Flush();
					_shLogWriter.Close();
					_shLogWriter = null;
					Console.WriteLine("done.");
				}
				else
					Console.WriteLine("\tnot open!");
			}
		}
		#endregion

		#region Test methods
		/// <summary>
		/// Queries all artefacts.
		/// </summary>
		[Test, ClientTestMethod(Order=10)]		//, Name="IQueryable<Artefact> _clientProxy.Artefacts")]
		public void QueryAllArtefacts()
		{
			Console.WriteLine("{0} artefacts currently in repository", _clientProxy.Artefacts.Count());
			foreach (Artefact artefact in _clientProxy.Artefacts)
				Console.WriteLine(artefact.ToString());
//					string.Format("{0}: Id={1} TimeCreated={2} TimeUpdated={3} TimeChecked={4}",
//					artefact.GetType().Name, artefact.Id, artefact.TimeCreated, artefact.TimeUpdated, artefact.TimeChecked));
		}

		/// <summary>
		/// Tests the query artefacts_ linq_ statement.
		/// </summary>
		[Test, ClientTestMethod(Order=20)]		//, Name="IEnumerator<Artefact> _clientProxy.Artefacts using LINQ statement")]
		public void TestQueryArtefacts_Linq_Statement()
		{
			var q = from a in _clientProxy.Artefacts
				        where a.Id > 32799
			        select a;
			foreach (Artefact artefact in q)
				Console.WriteLine(artefact.ToString());
		}

		[Test, ClientTestMethod(Order=30)]		//, Name="IEnumerator<Artefact> _clientProxy.Artefacts using LINQ method syntax")]
		public void TestQueryArtefacts_Linq_Method()
		{
			var q = _clientProxy.Artefacts.Where((a) => a.Id > 32799);
			foreach (Artefact artefact in q)
				Console.WriteLine(artefact.ToString());
		}

		[Test, ClientTestMethod(Order=50)]		//, Name="int _clientProxy.Artefacts.Count()")]
		public void TestQueryArtefactsDrivesCount()
		{
			Console.WriteLine("{0} artefacts currently in repository\n{1} drives",
				_clientProxy.Artefacts.Count(), _fsCreator.Drives.Count());
		}

		/// <summary>
		/// Tests the file system artefact creator.
		/// </summary>
		[Test, ClientTestMethod(Order=40)]		//, Name="FileSystemArtefactCreator")]
		public void TestFileSystemArtefactCreator()
		{
			Console.WriteLine("{0} artefacts currently in repository\n{1} drives", _clientProxy.Artefacts.Count(), _fsCreator.Drives.Count());
//				var q = _fsCreator.Drives.Where((a) => a.Id > 32799);
			//a.GetType().FullName == typeof(Drive).FullName && 
			foreach (Artefact artefact in _fsCreator.Drives)
				Console.WriteLine(artefact.ToString());
			_fsCreator.Run(null);
		}
		#endregion
	}
}
