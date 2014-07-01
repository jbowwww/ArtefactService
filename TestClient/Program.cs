using System;
using System.Collections;
using System.Collections.Generic;
using TextWriter=System.IO.TextWriter;
using System.Linq;
using System.ServiceModel;
using System.Reflection;

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
		private const string _clientLogFilePath = @"Client.Log";
		
		#region Debug Writer Constants
//		private const string _runTestOutputPrefix = "\n";
		private const string _runTestOutputNewLine = "\n";		//"\n# ";
		private const string _runTestFormatStart = "\n########\n# RunTest: Start: {0}\n########\n";
		private const string _runTestFormatSuccess = "\n########\n# RunTest: Success: {0}\n########\n";
		private const string _runTestOutputUnknownError = "\n########\n# RunTest: Failed: {0}\n########\n";
		private const string _runTestFormatError = "\n########\n# RunTest: Failed: {0}\n########\n";
		private const string _runTestFormatException = "{0}: {1}\n{2}\n";
//		private const string _runTestOutputSuffix = "\n";
		#endregion

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			Console.Write("\n--- Client starting ---\n\n");
			TextWriter consoleOut = Console.Out;
			TextWriter consoleError = Console.Error;
			List<TextWriter> textWriters = new List<TextWriter>();
			foreach (string arg in args)
			{
				if (string.Compare(arg, "-C") == 0)
					textWriters.Add(consoleOut);
				else if (arg.StartsWith("-L"))
					textWriters.Add(new LogTextWriter(arg.Length > 2 ? arg.Substring(2) : _clientLogFilePath));
			}
			Console.SetOut(new MultiTextWriter(textWriters.Count > 0 ? textWriters.ToArray() : new TextWriter[] { Console.Out }) { UseTimeStamp = true });
			Console.SetError(consoleOut);
			RunTests();
			Console.Write("\n--- Client exiting ---\n\n");
			consoleOut.Close();
			if (textWriters.Count > 0)
			{
				Console.Out.Close();
				Console.SetOut(consoleOut);
				Console.SetError(consoleError);
			}
		}

		/// <summary>
		/// Runs the tests.
		/// </summary>
		/// <param name="output">Output.</param>
		public static void RunTests()
		{
			ClientTestFixture _testFixture = new ClientTestFixture();
			_testFixture.Init();
			(typeof(ClientTestFixture).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(mi => mi.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
				.OrderBy<MethodInfo, int>(
					mi => (mi.GetCustomAttributes(typeof(TestMethodAttribute), false)[0] as TestMethodAttribute).Order))
				.ToList().ForEach(mi => RunTest(
					(mi.GetCustomAttributes(typeof(TestMethodAttribute), false)[0] as TestMethodAttribute).Name,
					() => mi.Invoke(_testFixture, new object[] { })));
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="testMethod">Test method.</param>
		/// <param name="output">Output.</param>
		public static void RunTest(string name, Action testMethod, TextWriter output = null)
		{
			if (output == null)
				output = Console.Out;
			string _oldNL = output.NewLine;
			try
			{
				output.WriteLine(_runTestFormatStart, name);
				testMethod();
				output.WriteLine(_runTestFormatSuccess, name);
			}
			catch (FaultException<ExceptionDetail> ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				foreach (DictionaryEntry de in ex.Data)
					output.Write("\t{0} = {1}\n", de.Key.ToString(), de.Value.ToString());
				int indent = 0;
				for (ExceptionDetail detail = ex.Detail; detail != null; detail = detail.InnerException)
					output.WriteLine(string.Concat(" ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, detail.Type, detail.Message, detail.StackTrace))
						.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
			}
			catch (Exception ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				foreach (DictionaryEntry de in ex.Data)
					output.Write("\t{0} = {1}\n", de.Key.ToString(), de.Value.ToString());
				int indent = 0;
				for (Exception innerEx = ex.InnerException; innerEx != null; innerEx = innerEx.InnerException)
					output.WriteLine(string.Concat("\n ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, innerEx.GetType().FullName, innerEx.Message, innerEx.StackTrace))
						.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
			}
			finally
			{
				output.NewLine = _oldNL;			// you COULD wrap this in a using() if you write a class implementing IDisposable with this line in Dispose()
			}
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		/// <returns><c>true</c>, if test was run, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		/// <param name="testMethod">Test method.</param>
		/// <param name="output">Output.</param>
		public static bool RunTest(string name, Func<bool> testMethod, TextWriter output = null)
		{
			if (output == null)
				output = Console.Out;
			string _oldNL = output.NewLine;
			try
			{
				output.WriteLine(_runTestFormatStart, name);
				if (!testMethod())
					output.WriteLine(_runTestOutputUnknownError, name);
				output.WriteLine(_runTestFormatSuccess, name);
				return true;
			}
			catch (FaultException<ExceptionDetail> ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				foreach (DictionaryEntry de in ex.Data)
					output.Write("\t{0} = {1}\n", de.Key.ToString(), de.Value.ToString());
				int indent = 0;
				for (ExceptionDetail detail = ex.Detail; detail != null; detail = detail.InnerException)
					output.WriteLine(string.Concat(" ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, detail.Type, detail.Message, detail.StackTrace))
						.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
				return false;
			}
			catch (Exception ex)
			{
				output.WriteLine(_runTestFormatError, name);
				output.WriteLine(_runTestFormatException, ex.GetType().FullName, ex.Message, ex.StackTrace);
				foreach (DictionaryEntry de in ex.Data)
					output.Write("\t{0} = {1}\n", de.Key.ToString(), de.Value.ToString());
				int indent = 0;
				for (Exception innerEx = ex.InnerException; innerEx != null; innerEx = innerEx.InnerException)
					output.WriteLine(string.Concat("\n ------ Inner Exception ------\n",
						string.Format(_runTestFormatException, innerEx.GetType().FullName, innerEx.Message, innerEx.StackTrace))
						.Replace("\n", string.Concat("\n", new string(' ', ++indent * 2))).TrimStart('\n'));
				return false;
			}
			finally
			{
				output.NewLine = _oldNL;			// you COULD wrap this in a using() if you write a class implementing IDisposable with this line in Dispose()
			}
		}
	}
}
