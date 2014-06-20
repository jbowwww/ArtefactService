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
	/// <summary>
	/// Host.
	/// </summary>
	public static class ArtefactServiceHost
	{
		#region Private fields
		private static TimeSpan _defaultTimeout = new TimeSpan(0, 0, 10);
		private static TimeSpan _timeout = _defaultTimeout;
		private static TextWriter _output;
		private static TextWriter _error;
		private static Thread _serviceHostThread;
		private static bool _exitServiceHost;
		#endregion

		public static string LogFilePath { get; set; }

		#region Methods
		/// <summary>
		/// Applies the service host behaviours.
		/// </summary>
		/// <param name="host">Host.</param>
		private static void ApplyServiceHostBehaviours(ServiceHost host)
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

		/// <summary>
		/// Applies the service host settings.
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="useCustomBehaviours">If set to <c>true</c> use custom behaviours.</param>
		private static void ApplyServiceHostSettings(ServiceHost host, bool useCustomBehaviours = false)
		{
			host.OpenTimeout = host.CloseTimeout = _timeout;
					
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

			if (useCustomBehaviours)
				ApplyServiceHostBehaviours(host);
			
			host.Opened += (sender, e) => _output.WriteLine("{0}: Opened", host.GetType().Name);
			host.Opening += (sender, e) => _output.WriteLine("{0}: Opening", host.GetType().Name);
			host.Closed += (sender, e) => _output.WriteLine("{0}: Closed", host.GetType().Name);
			host.Closing += (sender, e) => _output.WriteLine("{0}: Closing", host.GetType().Name);
			host.Faulted += (sender, e) => _error.WriteLine("{0}: Faulted", host.GetType().Name);
			host.UnknownMessageReceived += (sender, e) => _error.WriteLine("{0}: UnknownMessageReceived", host.GetType().Name);
		}
		
		/// <summary>
		/// Builds the service host.
		/// </summary>
		/// <returns>The service host.</returns>
		/// <param name="timeout">Timeout.</param>
		/// <param name="output">Output.</param>
		/// <param name="error">Error.</param>
		/// <param name="useCustomBehaviours">If set to <c>true</c> use custom behaviours.</param>
		public static ServiceHost BuildServiceHost(
			TimeSpan timeout = default(TimeSpan),
			 TextWriter output = null,
			 TextWriter error = null, 
			bool useCustomBehaviours = false)
		{
			if (output == null)
				output = Console.Out;
			if (error == null)
				error = Console.Error;

			_timeout = timeout;
			_output = output;
			_error = error;
			
			ServiceHost sh = new ServiceHost(typeof(Repository));
			ApplyServiceHostSettings(sh, useCustomBehaviours);
			
			return sh;
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			ServiceHost sh = null;
			try
			{
				sh = BuildServiceHost(_defaultTimeout, Console.Out, Console.Error, false);
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

		/// <summary>
		/// Stops the async thread.
		/// </summary>
		public static void StopAsyncThread ()
		{
			_exitServiceHost = true;
		}

		/// <summary>
		/// Gets the or create async thread.
		/// </summary>
		/// <returns>The or create async thread.</returns>
		/// <param name="artefactTypes">Artefact types.</param>
		/// <param name="timeout">Timeout.</param>
		/// <param name="useOutput">If set to <c>true</c> use output.</param>
		public static Thread GetOrCreateAsyncThread(Type[] artefactTypes = null, TimeSpan timeout = default(TimeSpan), bool useOutput = true)
		{
			if (useOutput)
				return GetOrCreateAsyncThread(artefactTypes, timeout, Console.Out, Console.Error);
			else
				return GetOrCreateAsyncThread(artefactTypes, timeout, TextWriter.Null);
		}

		/// <summary>
		/// Gets the or create async thread.
		/// </summary>
		/// <returns>The or create async thread.</returns>
		/// <param name="artefactTypes">Artefact types.</param>
		/// <param name="timeout">Timeout.</param>
		/// <param name="output">Output.</param>
		/// <param name="error">Error.</param>
		public static Thread GetOrCreateAsyncThread(Type[] artefactTypes = null, TimeSpan timeout = default(TimeSpan), TextWriter output = null, TextWriter error = null)
		{
			if (output == null)
//				Console.SetOut(output);
				output = Console.Out;
			if (error == null)
//				Console.SetError(error);
				error = output == Console.Out ? Console.Error : output;

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
				
					FileStream _logFileStream = null;
				ServiceHost sh = null;
				try
				{
						if (LogFilePath != null)
							output = error = /*TextWriter.Synchronized(*/ new StreamWriter(_logFileStream = File.OpenWrite(LogFilePath));//);
						sh = BuildServiceHost(timeout, output, error, false);
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
					if (LogFilePath != null && _logFileStream != null)
						{
							_logFileStream.Flush(true);
							_logFileStream.Close();
							_logFileStream = null;
						}
				}
			});
			_serviceHostThread.Priority = ThreadPriority.BelowNormal;	// this thread doesn't run the service host itself, and i don't think the thread created
			// by sh.Open() will inherit this lower priority (should b e normal??)
		}

		/// <summary>
		/// Executes the via command line.
		/// </summary>
		/// <param name="exe">Exe.</param>
		/// <param name="args">Arguments.</param>
		public static void ExecuteViaCommandLine(string exe, string args)
		{
			ProcessStartInfo oInfo = new ProcessStartInfo(exe, args);
			Process proc = Process.Start(oInfo);
		}
		#endregion
	}
}