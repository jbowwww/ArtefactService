using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics;

namespace Artefacts.Services
{
	public class ArtefactServiceHost
	{	
		public static TimeSpan DefaultTimeout = new TimeSpan(0, 0, 10);
		
		/// <summary>
		/// Builds the service host.
		/// </summary>
		/// <returns>
		/// The service host.
		/// </returns>
		public static ServiceHost BuildServiceHost(TimeSpan timeout = default(TimeSpan), TextWriter output = null, TextWriter error = null)
		{
			if (output == null)
				output = Console.Out;
			if (error == null)
				error = Console.Error;

			ServiceHost sh = new ServiceHost(typeof(ArtefactRepository));
			timeout = timeout == default(TimeSpan) ? DefaultTimeout == default(TimeSpan) ? default(TimeSpan) : DefaultTimeout : timeout;
			if (timeout != null)
			{
				sh.OpenTimeout = sh.CloseTimeout = timeout;
			}
			
//			sh.AddServiceEndpoint(typeof(IArtefactService),
//				new NetTcpBinding(SecurityMode.None),
//				new Uri("net.tcp://localhost:3333/ArtefactService"));
			sh.AddServiceEndpoint(typeof(IRepository<Artefact>),
				new NetTcpBinding(SecurityMode.None)
				{
					MaxBufferSize = 16384,
					ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
					{
						MaxStringContentLength = 16384
					}
				},
				new Uri("net.tcp://localhost:3334/ArtefactRepository"));
			
			ServiceDebugBehavior sdb = sh.Description.Behaviors.Find<ServiceDebugBehavior>();
			if (sdb == null)
				sh.Description.Behaviors.Add(sdb = new ServiceDebugBehavior());
			sdb.IncludeExceptionDetailInFaults = true;
//			ArtefactService.AddTypeResolver(sh, new WCFTypeResolver());
			
//			foreach (ServiceEndpoint endpoint in sh.Description.Endpoints)
//			{
//				foreach (OperationDescription operation in endpoint.Contract.Operations)
//				{
//					DataContractSerializerOperationBehavior dcsb = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
//					if (dcsb == null)
//						operation.Behaviors.Add(dcsb = new MyDataContractBehaviour(operation));
//					dcsb.DataContractResolver = new WCFTypeResolver();
//					dcsb.DataContractSurrogate = new WCFDataSerializerSurrogate();
//				}
//			}
			
			sh.Opened += (sender, e) => output.WriteLine("{0}: Opened", sh.GetType().Name);
			sh.Opening += (sender, e) => output.WriteLine("{0}: Opening", sh.GetType().Name);
			sh.Closed += (sender, e) => output.WriteLine("{0}: Closed", sh.GetType().Name);
			sh.Closing += (sender, e) => output.WriteLine("{0}: Closing", sh.GetType().Name);
			sh.Faulted += (sender, e) => output.WriteLine("{0}: Faulted", sh.GetType().Name);
			sh.UnknownMessageReceived += (sender, e) => output.WriteLine("{0}: UnknownMessageReceived", sh.GetType().Name);
			
			return sh;
		}

		/// <summary>
		/// The entry point of the program when output as an .exe
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main(string[] args)
		{
			ServiceHost sh = null;
			try
			{
				sh = BuildServiceHost();
				sh.Open();
				Console.WriteLine(sh.ToString());
				Console.ReadKey();
			}
			catch (/*Communication*/Exception ex)
			{
				Console.Error.WriteLine("\nServiceHost Exception (State=" + (sh == null ? "(null)" : sh.State.ToString()) + "):\n" + ex.ToString() + "\n");
			}
			finally
			{
				if (sh != null && sh.State != CommunicationState.Closed && sh.State != CommunicationState.Closing)
					sh.Close();
			}
		}

		private static Thread _serviceHostThread;
		private static bool _exitServiceHost;

		public static void StopAsyncThread ()
		{
			_exitServiceHost = true;
		}

		/// <summary>
		/// Creates an async thread but does not start the thread
		/// </summary>
		/// <returns>
		/// The async thread.
		/// </returns>
		public static Thread GetOrCreateAsyncThread(Type[] artefactTypes = null, TimeSpan timeout = default(TimeSpan), bool useOutput = true)
		{
			TextWriter output, error;
			if (useOutput)
			{
				output = Console.Out;
				error = Console.Error;
			}
			else
				output = error = TextWriter.Null;

			if (_serviceHostThread != null && _serviceHostThread.IsAlive)
				return _serviceHostThread;

			_exitServiceHost = false;
			return _serviceHostThread = new Thread(() =>
			{
//				if (artefactTypes != null)
//				{
////					ArtefactRepository.ArtefactTypes.AddRange(artefactTypes);
//					//or
//					Artefact.ArtefactTypes.AddRange(artefactTypes);
//				}

				ServiceHost sh = null;
				try
				{
					sh = BuildServiceHost(timeout);
					sh.Open();
					
					output.WriteLine(sh.ToString(true));
					while (!_exitServiceHost)
						Thread.Sleep(333);
				}
				catch (Exception ex)
				{
					error.WriteLine("\nServiceHost Exception (State={0}):\n{1}\n", (sh == null ? "(null)" : sh.State.ToString()), ex.ToString());
				}
				finally
				{
					if (sh != null && sh.State != CommunicationState.Closed && sh.State != CommunicationState.Closing)
						sh.Close();
				}
			});
		}

		public static void ExecuteViaCommandLine(string exe, string args)
		{
			ProcessStartInfo oInfo = new ProcessStartInfo(exe, args);
			Process proc = Process.Start(oInfo);
		}
	}
}