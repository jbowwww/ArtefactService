using System;
using System.Diagnostics;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;

using Artefacts;
using Artefacts.Services;

namespace ArtefactClientTest
{
	class ArtefactClientTest
	{
		public static void Main (string[] args)
		{
			bool exitServiceHost = false;

//			Console.ReadKey();

//			ISession sesh = NHibernate_Helper.Session;

			//			Execute("../../../ArtefactServiceHost/bin/Debug/ArtefactServiceHost.exe");
			Thread serviceHostThread = new Thread(() =>
			{
				using (ServiceHost sh = ArtefactServiceHost.ArtefactServiceHost.BuildServiceHost())
				{
					try
					{
						sh.Open();		// need a brief thread.sleep before writing state string??

						Console.WriteLine("Service: " + sh.Description.ServiceType.FullName + " (" +
							sh.Description.Namespace + sh.Description.Name + ")"
						);
						foreach (ServiceEndpoint endpoint in sh.Description.Endpoints)
						{
//							foreach (OperationDescription od in endpoint.Contract.Operations)
//							{
//								DataContractSerializerOperationBehavior contractBehaviour =
//									od.Behaviors.Find<DataContractSerializerOperationBehavior>();
//								if (contractBehaviour == null)
//									contractBehaviour = new DataContractSerializerOperationBehavior(od);
//								contractBehaviour.
//							}
							Console.WriteLine(endpoint.ToString(true));
						}
						Console.WriteLine();

						while (!exitServiceHost)
							Thread.Sleep(255);
					}
					catch (Exception ex)
					{
						Console.Error.WriteLine("\nServiceHost Exception (State={0}):\n{1}\n", sh.State.ToString(), ex.ToString());
					}
					finally
					{
						if (sh.State != CommunicationState.Closed && sh.State != CommunicationState.Closing)
							sh.Close();
					}
				}
			});

			try
			{
				Console.ReadKey();

				// Start service host thread and pause for a few seconds to ensure it has started
				serviceHostThread.Start();
				Thread.Sleep(2880);

				IArtefactRepository proxy = new ChannelFactory<IArtefactRepository>(
					new NetTcpBinding(SecurityMode.None),
					"net.tcp://localhost:3333/ArtefactRepository")
					.CreateChannel();
				Console.WriteLine("Client: proxy=\"{0}\"\n", proxy.ToString());

				Artefact testArtefact = new Artefact();
				Console.WriteLine("testArtefact: {0}", testArtefact.ToString());
				testArtefact = (Artefact)proxy.AddArtefact(testArtefact);
				Console.WriteLine("proxy.AddArtefact(testArtefact) returned Id={0}", testArtefact.Id == null ? "(null)" : testArtefact.Id.ToString());

				Console.WriteLine();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("\nIArtefactRepository Exception\n" + ex.ToString() + "\n"); 		// + (proxy == null ? "" : proxy.State.ToString()) + "):
			}
			finally
			{
				Console.WriteLine("Stopping service host thread...");
				exitServiceHost = true;
				if (serviceHostThread.Join(2880))
					Console.WriteLine("Stopped cleanly\n");
				else
					Console.WriteLine("Thread.Join() unsuccessful\n");
			}
		}

		internal static void Execute (string exe, string args = "")
		{
			ProcessStartInfo oInfo = new ProcessStartInfo(exe, args);
			Process proc = Process.Start(oInfo);
		}
	}
}
