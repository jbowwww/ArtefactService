using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Linq;

namespace Artefacts.Service
{
	/// <summary>
	/// Host.
	/// </summary>
	public static class ArtefactHost
	{
		#region Private fields
		private static TimeSpan _defaultTimeout = new TimeSpan(0, 0, 10);
		private static TimeSpan _timeout = _defaultTimeout;
		private static TextWriter _output;
		private static TextWriter _error;
		private static Type[] _artefactTypes = null;
		private static Repository _serviceInstance = null;
		private static ServiceHost _serviceHost = null;
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
//		new ServiceDebugBehavior() {
//			foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
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
		/// <param name="serviceInstance"></param>
		/// <param name="timeout">Timeout.</param>
		/// <param name="output">Output.</param>
		/// <param name="error">Error.</param>
		/// <param name="useCustomBehaviours">If set to <c>true</c> use custom behaviours.</param>
		public static ServiceHost BuildServiceHost(object serviceInstance, TimeSpan timeout = default(TimeSpan), bool useCustomBehaviours = false)
		{
			_timeout = timeout;			
			ServiceHost sh = new ServiceHost(serviceInstance);
			ApplyServiceHostSettings(sh, useCustomBehaviours);			
			return sh;
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			string hostTypeName = typeof(ArtefactHost).FullName;
			Process proc = Process.GetCurrentProcess();
			ProcessStartInfo procInfo = proc.StartInfo;
			TextWriter consoleOut = Console.Out;
			TextWriter consoleError = Console.Error;			

			_output = new MultiTextWriter(consoleOut, new LogTextWriter("Server.Log")) { UseTimeStamp = true };
			Console.SetOut(_output);
			Console.SetError(_output);			
			try
			{
				Console.Write("--- {0} starting ---\n", hostTypeName);
				Console.WriteLine("{2}:{3} ({1}) [{5}] @ {4}\n------------\n\n",
					null, proc.Id, proc.MachineName, proc.ProcessName, proc.StartTime.ToString("s"), proc.PriorityClass);
				foreach (string arg in args)
				{
					Assembly pluginAssembly;
					IEnumerable<Type> pluginArtefactTypes;
					if (arg.StartsWith("-P"))
					{
						int pluginLoadCount = 0;
						string pluginDir = arg.Length > 2 ? arg.Substring(2) : "Plugins/";
						string[] pluginFiles = Directory.GetFiles(pluginDir, "*.dll", SearchOption.TopDirectoryOnly);
						Console.WriteLine("{0}: Plugin directory \"{1}\" ({2} plugins):", hostTypeName, pluginDir, pluginFiles.Length);
						foreach (string filePath in pluginFiles)
						{
//							string filePath = Path.Combine(pluginDir, filename);
							Console.Write("\t{0}: ", filePath);
							pluginAssembly = Assembly.LoadFrom(filePath);
							if (pluginAssembly == null)
								Console.WriteLine("fail!");
							else
							{
								pluginArtefactTypes = pluginAssembly.ExportedTypes.Where((T) => typeof(Artefact).IsAssignableFrom(T));
								Console.WriteLine("{0} artefact types", pluginArtefactTypes.Count());
								Console.WriteLine(string.Concat("\t\t", string.Join(" ", pluginArtefactTypes.Select<Type, string>((T) => T.FullName))));
								Artefact.ArtefactTypes.AddRange(pluginArtefactTypes);
								pluginLoadCount++;
							}
						}
						Console.WriteLine("{0}: {1} plugins loaded", hostTypeName, pluginLoadCount);
					}
					else if (arg.StartsWith("-A") || arg.StartsWith("-R"))
					{
						string assembly = arg.Substring(2);
						Console.Write("{0}: {1} assembly {2}: ", hostTypeName, arg.StartsWith("-A") ? "Load" : "Ref", assembly);
						pluginAssembly = Assembly.LoadFrom(assembly);
						if (pluginAssembly == null)
							Console.WriteLine("fail!");
						else if (arg.StartsWith("-R"))
							Console.WriteLine("OK");
						else
						{
							pluginArtefactTypes = pluginAssembly.ExportedTypes.Where((T) => typeof(Artefact).IsAssignableFrom(T));
							Console.WriteLine("{0} artefact types", pluginArtefactTypes.Count());
							Console.WriteLine(string.Concat("\t\t", string.Join(" ", pluginArtefactTypes.Select<Type, string>((T) => T.FullName))));
						}
						
					}
					else if (arg.StartsWith("-T"))
					{
						string type = arg.Substring(2);
						Console.Write("{0}: Load type \"{1}\": ", hostTypeName, type);
						Type T = Type.GetType(type);
						Console.WriteLine(T == null ? "fail!" : "OK");
						if (T != null)
							Artefact.ArtefactTypes.Add(T);
					}
				}				
				
				_serviceInstance = new Repository();
				_serviceHost = BuildServiceHost(_serviceInstance, _defaultTimeout, false);
				_serviceHost.Open();
				Console.WriteLine(_serviceHost);
				Console.ReadLine();
				_serviceHost.Close();
			}
			catch (FaultException<ExceptionDetail> ex)
			{
				Console.Error.Write("\n--- Service Host Exception ---\n{0}: {1}\n  Action: {2}\n  Fault: {3}{4}: {5}\n  Detail:\n{6}\n  StackTrace:\n{7}\n------------\n\n",
					ex.GetType().FullName, ex.Message, ex.Action, ex.Code.Namespace, ex.Code.Name, ex.Reason.GetMatchingTranslation(),
					ex.Detail.ToString().Trim('\n').Insert(0, "  ").Replace("\n", "\n  "),
					ex.StackTrace.Trim('\n').Insert(0, "  ").Replace("\n", "\n  "));
			}
			catch (FaultException ex)
			{
				Console.Error.Write("\n--- Service Host Exception ---\n{0}: {1}\n  Action: {2}\n  Fault: {3}{4}: {5}\n  StackTrace:\n{6}\n------------\n\n",
					ex.GetType().FullName, ex.Message, ex.Action, ex.Code.Name, ex.Code.Name, ex.Reason.GetMatchingTranslation(),
					ex.StackTrace.Trim('\n').Insert(0, "  ").Replace("\n", "\n  "));
			}
			catch (CommunicationException ex)
			{
				Console.Error.Write("\n--- Service Host Exception ---\n{0}: {1}\n  StackTrace:\n{2}\n------------\n\n",
					ex.GetType().FullName, ex.Message,
					ex.StackTrace.Trim('\n').Insert(0, "  ").Replace("\n", "\n  "));
			}
			catch (Exception ex)
			{
				Console.Error.Write("\n--- Service Host Exception ---\n{0}: {1}\n  StackTrace:\n{2}\n------------\n\n",
					ex.GetType().FullName, ex.Message, ex.StackTrace.Trim('\n').Insert(0, "  ").Replace("\n", "\n  "));
			}
			finally
			{
				Console.Write("\n--- {0} exiting ---\nService host state: {1}\n------------\n\n",
					hostTypeName, _serviceHost == null ? "(null)" : _serviceHost.State.ToString());
				if (_serviceHost.State == CommunicationState.Opened)
					_serviceHost.Close ();
				else
					_serviceHost.Abort();
				_output.Close();
				Console.SetOut(consoleOut);
				Console.SetError(consoleError);
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
			_serviceHostThread = new Thread(() =>
			{
//				if (artefactTypes != null)
//				{
////					ArtefactRepository.ArtefactTypes.AddRange(artefactTypes);
//					//or
//					Artefact.ArtefactTypes.AddRange(artefactTypes);
//				}
				
				FileStream _logFileStream = null;
				try
				{
						_serviceInstance = new Repository();
						if (LogFilePath != null)
							output = error = /*TextWriter.Synchronized(*/ new StreamWriter(_logFileStream = File.OpenWrite(LogFilePath));//);
						_output = output;
						_error = error;
						_serviceHost = BuildServiceHost(_serviceInstance, timeout, false);
					_serviceHost.Open();
					
					output.WriteLine(_serviceHost.ToString());
					while (!_exitServiceHost)
						Thread.Sleep(333);
				}
				catch (Exception ex)
				{
					error.WriteLine("\nServiceHost Exception (State={0}):\n{1}\n", (_serviceHost == null ? "(null)" : _serviceHost.State.ToString()), ex.ToString());
				}
				finally
				{
					if (_serviceHost != null && _serviceHost.State != CommunicationState.Closed && _serviceHost.State != CommunicationState.Closing)
						_serviceHost.Close();
					if (LogFilePath != null && _logFileStream != null)
						{
							_logFileStream.Flush(true);
							_logFileStream.Close();
							_logFileStream = null;
						}
				}
			});
			_serviceHostThread.Priority = ThreadPriority.BelowNormal;	// this thread doesn't run the service host itself, and i don't think the thread created
			_serviceHostThread.Start();
			return _serviceHostThread;
			// by sh.Open() will inherit this lower priority (should b e normal??)
		}

		/// <summary>
		/// Executes the via command line.
		/// </summary>
		/// <param name="exe">Exe.</param>
		/// <param name="args">Arguments.</param>
		public static Process ExecuteViaCommandLine(string exe, string args = null)
		{
//			ProcessStartInfo oInfo = new ProcessStartInfo(exe, args);
			return Process.Start(exe, args);
		}
		
		/// <summary>
		/// Determines if is running the specified exe.
		/// </summary>
		/// <returns><c>true</c> if is running the specified exe; otherwise, <c>false</c>.</returns>
		/// <param name="exe">Exe.</param>
		public static bool IsRunning(string exe)
		{
			ProcessStartInfo oInfo = new ProcessStartInfo("ps", "-A");
			oInfo.RedirectStandardOutput = true;
			oInfo.UseShellExecute = false;
			Process proc = Process.Start(oInfo);
			proc.WaitForExit();
			return proc != null && proc.StandardOutput != null && proc.StandardOutput.ReadToEnd().Contains(exe);
		}
		#endregion
	}
}