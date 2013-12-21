using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Linq.Expressions;

using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

using Artefacts;
using Artefacts.Services;
using Artefacts.FileSystem;

using NHibernate.Criterion;
using NHibernate.Linq;

namespace ArtefactClientTest
{
	class ArtefactClientTest
	{
		class ArtefactAdder : IReceiver
		{
			private bool _completed = false;
			
			public ArtefactAdder()
			{
				
			}
			
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
								value.Id = 
					_repoProxy.Add(value);
			}
			#endregion
		}
		
		private static Thread _serviceHostThread = null;

		private static ChannelFactory<IArtefactService> _proxyFactory = null;

		private static IArtefactService _proxy = null;
		
		private static IArtefactRepository<Artefact> _repoProxy = null;
		
		private static ICreator _fsCreator = null;

		public static void Main(string[] args)
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

				_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread();		//artefactTypes);
				_serviceHostThread.Start();
				Thread.Sleep(2880);

				_proxy = new ChannelFactory<IArtefactService>(
					new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3333/ArtefactService")
					.CreateChannel();

				_repoProxy = new ChannelFactory<IArtefactRepository<Artefact>>(
					new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:3334/ArtefactRepository")
//					_proxy.GetRepositoryAddress<Artefact>().AbsoluteUri)
					.CreateChannel();
				
				Console.WriteLine("\nClient Service Proxy: {0}\nService Artefact Repostiroy: {0}\n",
					_proxy.ToString(), _repoProxy.ToString());

				
//				Expression<Func<Artefact, bool>> query = a => a.TimeCreated > new DateTime(2013, 12, 21, 7, 44, 40);
//				ExpressionNode exNode = query.ToExpressionNode();
//					p.LastName == "Miller" 
//    && p.FirstName.StartsWith("M");
//				Artefact[] artefacts = _repoProxy.RunLinq(exNode);
				
//				IEnumerable<Artefact> artefacts = _repoProxy.GetAll();
				
				QueryResult r = _repoProxy.RunQuery(a => a.TimeCreated.Ticks > new DateTime(2013, 12, 21, 7, 44, 40).Ticks);
				r.ArtefactRepository = _repoProxy;
				Console.WriteLine("{0} artefacts currently in repository", r.TotalCount);
				foreach (Artefact artefact in r)
					Console.WriteLine(artefact.ToString());
				Console.WriteLine();
				
//				_proxy.BeginTransaction();
				
//				_fsCreator = new FileSystemArtefactCreator()
//				{
//					BaseUri = new Uri("file:///media/Scarydoor/mystuff/moozik/samples/mycollections/")
//				};
//				using (_fsCreator.Subscribe(new ArtefactAdder()))
//				{
//					_fsCreator.Run(null);
//				}
				
//				artefacts = _repoProxy.GetAll();
//				artefacts = _repoProxy.RunLinq(exNode);
				
//				QueryResult r2 = _repoProxy.RunQuery(a => a.TimeCreated > new DateTime(2013, 12, 21, 7, 44, 40));
//				r2.ArtefactRepository = _repoProxy;
//				Console.WriteLine("\n{0} artefacts currently in repository", r2.TotalCount);
//				foreach (Artefact artefact in r2)
//					Console.WriteLine(artefact.ToString());
				
				Console.WriteLine();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("\nIArtefactRepository Exception\n" + ex.ToString() + "\n"); 		// + (proxy == null ? "" : proxy.State.ToString()) + "):
			}
			finally
			{
//				_proxy.EndTransaction(true);

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


				
				
//				DetachedCriteria crit = DetachedCriteria.For<Artefact>().CreateCriteria("").AddOrder(Order.Asc("Id"));
//				foreach (Artefact artefact in _proxy.Query())				         // _proxy.GetCr(crit)
//				{					
//					Console.WriteLine(artefact.ToString());
//				}

//				foreach ( Artefact artefact in (
//					from a in _proxy.Query()
//				 	where a.TimeUpdated > new DateTime(2013, 12, 13, 20, 12, 10)
//						//a.GetType().Equals(typeof(Artefacts.FileSystem.Drive))
//					orderby ((Artefacts.FileSystem.Drive)a).Label
//					select a ) )
//				{
//					Console.WriteLine(artefact.ToString());
//				}
