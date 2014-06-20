using System;
using System.Collections;
using System.Collections.Generic;
using TextWriter=System.IO.TextWriter;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

//using Serialize.Linq.Extensions;
//using Serialize.Linq.Nodes;

using Artefacts;
using Artefacts.Service;
using Artefacts.FileSystem;
using System.IO;

namespace Artefacts.TestClient
{
	/// <summary>
	/// Artefact client test.
	/// </summary>
	/// <remarks>
	/// "value(Artefacts.Services.Queryable`1[Artefacts.Artefact]).Where(a => (a.TimeCreated.Ticks > new DateTime(2013, 12, 21, 7, 44, 40).Ticks))"
	/// </remarks>
	public static class Program
	{
		#region Private static fields
		private static int _init = 0;
		private static int _exit = 0;
		private static TimeSpan _defaultTimeout = new TimeSpan(0, 0, 10);
		private const int _serviceHostStartDelay = 1000;
		private const int _serviceHostStopTimeout = 2200;
		private static Type[] _artefactTypes = new Type[]
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
		private static Thread _serviceHostThread = null;
		private static string _serviceHostLogFilePath = @"ServiceHost.Log";
		private static FileStream _serviceHostLog = null;
		private static TextWriter _shLogWriter = null;
		#endregion

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			Init();
			TestRunner.RunTests();
			Exit();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public static void Init()
		{
			Console.Write("ClientTest.Init: ");
			if (Thread.VolatileRead(ref _init) != 0)
				Console.WriteLine("Already initialised");
			else
			{
				Console.WriteLine("Initialising...");
				Thread.VolatileWrite(ref _init, 1);

				// Add artefact types
				Artefact.ArtefactTypes.AddRange(_artefactTypes);

				// Start service host thread and pause for a few seconds to ensure it has started
//				_serviceHostLog = new FileStream(_serviceHostLogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 1024);
//				if (_serviceHostLog == null)
//					throw new ApplicationException("Could not open service host log");
//
				_shLogWriter = new LogTextWriter(_serviceHostLogFilePath);
				_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread(_artefactTypes, _defaultTimeout, _shLogWriter);
//				ArtefactServiceHost.LogFilePath = _serviceHostLogFilePath;
//				_serviceHostThread = ArtefactServiceHost.GetOrCreateAsyncThread(_artefactTypes, _defaultTimeout, false);
				_serviceHostThread.Start();
				Thread.Sleep(_serviceHostStartDelay);					//		ArtefactServiceHost.Main(null);

			}
		}

		/// <summary>
		/// Exit this instance.
		/// </summary>
		public static void Exit()
		{
			Console.Write("ClientTest.Exit: ");
			if (Thread.VolatileRead(ref _exit) != 0)
				Console.WriteLine("Already exited");
			else
			{
				Console.WriteLine("Exiting...");
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

//				Console.Write("\tClosing service host log... ");
//				if (_serviceHostLog != null)
//				{
//					_serviceHostLog.Flush();
//					_serviceHostLog.Close();
//					_serviceHostLog = null;
//					Console.WriteLine("done.");
//				}
//				else
//					Console.WriteLine("\tnot open!");
			}
		}
	}
}
