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
		public string TimeStampFormat = "s";
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
			string timestamp = DateTime.Now.ToString(TimeStampFormat) + " ";
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					if (_lastCharWasNewLine)
						output.Write(timestamp);
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
			string timeStamp = string.Format("{0} ", DateTime.Now.ToString(TimeStampFormat));
			StringBuilder sb = new StringBuilder(buffer.Length + 32);
			string o;
			int i, start = 0;
			for (i = 0; i < buffer.Length; i++)
			{
				if (i == buffer.Length)
				{
					sb.Append(timeStamp);
					sb.Append(buffer, start, i - start + 1);
					start = i + 1;
					_lastCharWasNewLine = buffer[i - 1] == '\n';
				}
				else if (buffer[i] == '\n')
				{
					while (i < buffer.Length - 1 && buffer[i] == '\n')
						i++;
					sb.Append(timeStamp);
					sb.Append(buffer, start, i - start + 1);
					start = i + 1;
				}
			}
			o = sb.ToString();				
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					output.Write(o);
					output.Flush();
				}
			}
						
		}
	}
}

