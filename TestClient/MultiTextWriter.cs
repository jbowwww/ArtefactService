using System;
using System.IO;
using System.Collections.Generic;

namespace Artefacts.TestClient
{
	public class MultiTextWriter : TextWriter
	{
		public readonly List<TextWriter> Outputs = new List<TextWriter>();
		
		public MultiTextWriter(params TextWriter[] outputs)
		{
			Outputs.AddRange(outputs);
		}

		protected override void Dispose(bool disposing)
		{
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					output.Flush();
					output.Close();
				}
			}
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
					output.Write(value);
//					output.Flush();		// don't flush just for one char?
				}
			}
		}
		
		
		public override void Write(char[] buffer)
		{
			foreach (TextWriter output in Outputs)
			{
				if (output != null)
				{
					output.Write(buffer);
					output.Flush();
				}
			}
		}
	}
}

