using System;
using System.IO;
using System.Collections.Generic;

namespace Artefacts
{
	public class MultiTextWriter : TextWriter
	{
		private bool _lastCharWasNewLine = true;
		public readonly List<TextWriter> Outputs = new List<TextWriter>();
		
		public bool UseTimeStamp = false;
		public string TimeStampFormat = "s";
		
		public MultiTextWriter(params TextWriter[] outputs)
		{
			Outputs.AddRange(outputs);
		}

		public override void Close()
		{
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					output.Flush();
					output.Close();
				}
			}
			base.Close();
		}
		
		/// <summary>
		/// implemented abstract members of TextWriter
		/// </summary>
		public override System.Text.Encoding Encoding {
			get { return System.Text.Encoding.Default; }
		}
		
		
		public override void Write(char value)
		{
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					if (_lastCharWasNewLine)
						output.Write(DateTime.Now.ToString(TimeStampFormat) + " ");
					output.Write(value);
//					output.Flush();		// don't flush just for one char?
					_lastCharWasNewLine = value == '\n';
				}
			}
		}
		
		
		public override void Write(char[] buffer)
		{
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					if (_lastCharWasNewLine)
						output.Write(DateTime.Now.ToString(TimeStampFormat) + " ");
					output.Write(buffer);
					output.Flush();
					_lastCharWasNewLine = buffer[buffer.Length - 1] == '\n';
				}
			}
		}
	}
}

