using System;
using System.Linq;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;

using Artefacts;
using Artefacts.Services;
using Artefacts.FileSystem;

namespace ArtefactClientTest
{
	class ArtefactClientTest
	{
		class ArtefactAdder : IReceiver
		{
			private bool _completed = false;

			#region IObserver implementation
			public void OnCompleted ()
			{
				_completed = true;
			}

			public void OnError (Exception error)
			{
				Console.WriteLine("ArtefactAdder: " + error.ToString() + "\n");
			}

			public void OnNext (Artefact value)
			{
				value.Id = _proxy.AddArtefact(value);
			}
			#endregion
		}

		private static Thread _serviceHostThread = null;

		private static ChannelFactory<IArtefactRepository> _proxyFactory = null;

		private static IArtefactRepository _proxy = null;

		private static ICreator _fsCreator = null;

		public static void Main (string[] args)
		{
			try
			{
				Console.ReadKey();

				// Start service host thread and pause for a few seconds to ensure it has started
				Type[] artefactTypes = new Type[]
				{
					typeof(Artefact),
					typeof(Drive),
					typeof(FileSystemEntry),
					typeof(File),
					typeof(Directory)
				};
				Artefact.ArtefactTypes.AddRange(artefactTypes);

				_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread(artefactTypes);
				_serviceHostThread.Start();
				Thread.Sleep(2880);

				_proxyFactory = new ChannelFactory<IArtefactRepository>(
					new NetTcpBinding(SecurityMode.None),
					"net.tcp://localhost:3333/ArtefactRepository");

//				foreach (OperationDescription operation in _proxyFactory.Endpoint.Contract.Operations)
//				{
//					DataContractSerializerOperationBehavior dcsob = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
//					if (dcsob == null)
//						operation.Behaviors.Add(dcsob = new DataContractSerializerOperationBehavior(operation));
//					dcsob.DataContractResolver = new WCFTypeResolver();
//				}

				_proxy = _proxyFactory.CreateChannel();

				Console.WriteLine("Client Proxy: {0}\n", _proxy.ToString());

//				_fsCreator = new FileSystemArtefactCreator()
//				{
//					BaseUri = new Uri("file:///home/jk/Downloads/moozik/")
//				};
//				using (_fsCreator.Subscribe(new ArtefactAdder()))
//				{
//					_fsCreator.Run(null);
//				}

				foreach ( Artefact artefact in (
					_proxy.Query() ) )
				{
					Console.WriteLine(artefact.ToString());
				}

//				foreach ( Artefact artefact in (
//					from a in _proxy.Query()
//				 	where a.TimeUpdated > new DateTime(2013, 12, 13, 20, 12, 10)
//						//a.GetType().Equals(typeof(Artefacts.FileSystem.Drive))
//					orderby ((Artefacts.FileSystem.Drive)a).Label
//					select a ) )
//				{
//					Console.WriteLine(artefact.ToString());
//				}

				Console.WriteLine();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("\nIArtefactRepository Exception\n" + ex.ToString() + "\n"); 		// + (proxy == null ? "" : proxy.State.ToString()) + "):
			}
			finally
			{
				if (_serviceHostThread != null)
				{
					Console.WriteLine("Stopping service host thread...");
					ArtefactServiceHost.StopAsyncThread();
					if (_serviceHostThread.Join(3880))
						Console.WriteLine("Stopped cleanly\n");
					else
						Console.WriteLine("Thread.Join() unsuccessful\n");
				}
				else
					Console.WriteLine("Service host thread null");
			}
		}
	}
}
