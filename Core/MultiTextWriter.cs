using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Artefacts
{
	public class MultiTextWriter : TextWriter
	{
		#region Fields
		private bool _lastCharWasNewLine = true;
		public readonly List<TextWriter> Outputs = new List<TextWriter>();	
		public bool UseTimeStamp = false;
		public string TimeStampFormat = "HH:mm:s:fff ";		//"s";
		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.MultiTextWriter"/> class.
		/// </summary>
		/// <param name="outputs">Outputs.</param>
		public MultiTextWriter(params TextWriter[] outputs)
		{
			Outputs.AddRange(outputs);
			foreach (TextWriter output in Outputs)
				output.Flush();
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		/// <remarks>implemented abstract member of TextWriter</remarks>
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
		/// Gets the encoding.
		/// </summary>
		/// <remarks>implemented abstract member of TextWriter</remarks>
		public override System.Text.Encoding Encoding {
			get { return System.Text.Encoding.Default; }
		}
		
		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <remarks>implemented abstract member of TextWriter</remarks>
		public override void Write(char value)
		{
			string timeStamp = DateTime.Now.ToString(TimeStampFormat);
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					if (_lastCharWasNewLine)
						output.Write(timeStamp);
					output.Write(value);
//					output.Flush();		// don't flush just for one char?
				}
			}
			_lastCharWasNewLine = value == '\n';
		}
		
		/// <summary>
		/// Write the specified buffer.
		/// </summary>
		/// <param name="buffer">Buffer.</param>
		/// <remarks>implemented abstract member of TextWriter</remarks>
		public override void Write(char[] buffer)
		{
			string timeStamp = DateTime.Now.ToString(TimeStampFormat);
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					if (_lastCharWasNewLine)
						output.Write(timeStamp);
					output.Write(buffer);
					output.Flush();
				}
			}
			_lastCharWasNewLine = buffer[buffer.Length - 1] == '\n';
		}
	}
}

