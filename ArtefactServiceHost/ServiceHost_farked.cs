using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Diagnostics;

namespace Artefacts.Services
{
	public class ArtefactServiceHost : ServiceHostBase
	{
		protected override ServiceDescription CreateDescription (out System.Collections.Generic.IDictionary<String, ContractDescription> id )
		{
			ServiceDescription sd = new ServiceDescription()
			{
				ServiceType = typeof(ArtefactRepository),
				ConfigurationName = "ArtefactRepository",
			};
			AddServiceEndpoint(new ServiceEndpoint(
				ContractDescription.GetContract(typeof(IArtefactRepository)),
//			                                typeof(ArtefactRepository)),
			                                       new NetTcpBinding(SecurityMode.None), new EndpointAddress("/Artefacts/")));
id = ImplementedContracts;
			return sd;
		}

		public ArtefactServiceHost (TextWriter output = null, TextWriter error = null)
		{
			if (output == null) output = Console.Out;
			if (error == null) error = Console.Error;

			base.InitializeDescription(new UriSchemeKeyedCollection(new Uri("net.tcp://localhost:4444")));
//			AddServiceEndpoint("IArtefactRepository", new NetTcpBinding(SecurityMode.None), "/Artefacts/");

			base.ApplyConfiguration();
			
//			AddBaseAddress();

			base.InitializeRuntime();

			output.WriteLine(this.ToString(true));

			base.Opened += (sender, e) => output.WriteLine("{0}: Opened", GetType().Name);
			base.Opening += (sender, e) => output.WriteLine("{0}: Opening", GetType().Name);
			base.Closed += (sender, e) => output.WriteLine("{0}: Closed", GetType().Name);
			base.Closing += (sender, e) => output.WriteLine("{0}: Closing", GetType().Name);
			base.Faulted += (sender, e) => error.WriteLine("{0}: Faulted", GetType().Name);
		}

		/// <summary>
		/// The entry point of the program when output as an .exe
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			ArtefactServiceHost sh = null;
			try
			{
				sh = new ArtefactServiceHost();
				sh.Open();
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
		public static Thread GetOrCreateAsyncThread (Type[] artefactTypes, bool useOutput = true)
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
				ArtefactServiceHost sh = null;
				try
				{
					sh = new ArtefactServiceHost();
					sh.Open();
					while (!_exitServiceHost)
						Thread.Sleep(255);
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