using System;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Artefacts
{
	public class LogTextWriter : TextWriter
	{
		const int _outputThreadDelay = 555;
		ConcurrentQueue<string> _outputQueue;
		Thread _outputThread;
		int _outputRun;
		TextWriter _innerWriter;
		string _prefix;
		string _suffix;

		public override Encoding Encoding {
			get { return Encoding.Default; }
		}

		public LogTextWriter(string filePath, string prefix = null, string suffix = null, string newline = "\n")
		{
			_outputQueue = new ConcurrentQueue<string>();
			_outputRun = 1;
			_outputThread = new Thread(() =>
			{
				string o;
				using (FileStream _fs = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
				{
					_innerWriter = new StreamWriter(_fs);
					_innerWriter.NewLine = newline;
					while (Thread.VolatileRead(ref _outputRun) == 1 || _outputQueue.Count > 0)
					{
						if (_outputQueue.Count > 0)
						{
							while (_outputQueue.TryDequeue(out o))
								_innerWriter.Write(o);
							_innerWriter.Flush();
						}
						else
							Thread.Sleep(_outputThreadDelay);
					}
//				_fs.Close();
				_innerWriter.Close();
				}
			});
			_outputThread.Priority = ThreadPriority.BelowNormal;
			_outputThread.Start();
			_prefix = prefix;
			_suffix = suffix;
		}

		public override void Close()
		{
			Thread.VolatileWrite(ref _outputRun, 0);
			base.Close();
		}

		public override void Write(char value)
		{
			if (_prefix != null)
			{
				if (_suffix != null)
					_outputQueue.Enqueue(string.Concat(_prefix, new string(value, 1), _suffix));
				else
					_outputQueue.Enqueue(string.Concat(_prefix, new string(value, 1)));
			}
			else
				_outputQueue.Enqueue(new string(value, 1));
		}

		public override void Write(char[] buffer)
		{
			if (_prefix != null)
			{
				if (_suffix != null)
					_outputQueue.Enqueue(string.Concat(_prefix, new string(buffer), _suffix));
				else
					_outputQueue.Enqueue(string.Concat(_prefix, new string(buffer)));
			}
			else
						_outputQueue.Enqueue(new string(buffer));
		}
	}
}

