using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics;

namespace Artefacts.Service
{
	public class ArtefactServiceHost
	{	
		public static TimeSpan DefaultTimeout = new TimeSpan(0, 0, 10);
		
		public static TimeSpan Timeout = DefaultTimeout;
		public static TextWriter Output;
		public static TextWriter Error;
		
		private void ApplyServiceHostBehaviours(ServiceHost host)
		{
			foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
			{
				foreach (OperationDescription operation in endpoint.Contract.Operations)
				{
					DataContractSerializerOperationBehavior dcsb = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
					if (dcsb == null)
						operation.Behaviors.Add(dcsb = new MyDataContractBehaviour(operation));
					dcsb.DataContractResolver = new WCFTypeResolver();
					dcsb.DataContractSurrogate = new WCFDataSerializerSurrogate();
				}
			}
		}
		
		private static void ApplyServiceHostSettings(ServiceHost host)
		{
			host.OpenTimeout = host.CloseTimeout = Timeout;
					
			ServiceDebugBehavior sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
			if (sdb == null)
				host.Description.Behaviors.Add(sdb = new ServiceDebugBehavior());
			sdb.IncludeExceptionDetailInFaults = true;
			
			host.AddServiceEndpoint(typeof(IRepository),
				new NetTcpBinding(SecurityMode.None)
				{
										MaxBufferSize = 4096,
					ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
					{
												MaxStringContentLength = 32768
					}
				},
				new Uri("net.tcp://localhost:3334/ArtefactRepository"));
			
//			ApplyServiceHostBehaviours(host);
			
			host.Opened += (sender, e) => Output.WriteLine("{0}: Opened", host.GetType().Name);
			host.Opening += (sender, e) => Output.WriteLine("{0}: Opening", host.GetType().Name);
			host.Closed += (sender, e) => Output.WriteLine("{0}: Closed", host.GetType().Name);
			host.Closing += (sender, e) => Output.WriteLine("{0}: Closing", host.GetType().Name);
			host.Faulted += (sender, e) => Output.WriteLine("{0}: Faulted", host.GetType().Name);
			host.UnknownMessageReceived += (sender, e) => Output.WriteLine("{0}: UnknownMessageReceived", host.GetType().Name);
		}
		
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

			Timeout = timeout;
			Output = output;
			Error = error;
			
			ServiceHost sh = new ServiceHost(typeof(Repository));
			ApplyServiceHostSettings(sh);
			
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
					
					output.WriteLine(sh.ToString());
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