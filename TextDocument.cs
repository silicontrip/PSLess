using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Management.Automation.Host;

namespace net.ninebroadcast {

	public class TextDocument {

		private Dictionary<Int64,Int64> offsets;
		private string filename;
		private FileStream documentFile;
	//	private StreamReader documentStream;
	//	private bool unsaved;
		private Encoding readWriteEncoding;
		private Int64 currentLine;

		public TextDocument()
		{
		//	buffer = new List<string>();
			this.filename="";
			documentFile = null;
		//	unsaved=false;
		}

		public TextDocument(string filename)
		{
			readWriteEncoding = new ASCIIEncoding();
			// try {
				offsets = new Dictionary<Int64,Int64>();
				string absoluteFn = Path.GetFullPath(filename);
				documentFile = File.Open(absoluteFn,FileMode.Open,FileAccess.Read);
				readWriteEncoding = new ASCIIEncoding();
				currentLine = 1;
				offsets[currentLine] = documentFile.Position;
				// documentStream = OpenText(absoluteFn);
				// buffer = new List<string>(File.ReadAllLines(absoluteFn,readWriteEncoding));
			// } catch (FileNotFoundException) {
				// buffer = new List<string>();
			// }
			this.filename = filename;
		}

		public string getBaseName()
		{
			// TODO: strip directory
			return filename;
		}

		public void Close()
		{
			if (documentFile != null)
				documentFile.Close();
		}
/*
		public string ReadNextLine()
		{
			documentStream.ReadLine();
			currentLine++;
			offsets[currentLine] = documentFile.Position;
		}
*/
		public void Seek(Int64 line)
		{
			Int64? pos = offsets[line];
			// handle lines that arent in the offset array.
			if (pos.HasValue)
				documentFile.Seek(pos.Value,SeekOrigin.Begin);
			else
			{
				while ((currentLine != line) && (SeekNextLine()));
				  // double check that this is correct.
				//	SeekNextLine();
			}
		}

		private int Peek()
		{
			Int64 pos = documentFile.Position;
			int cc = documentFile.ReadByte();
			// Console.WriteLine("Peek byte: " + cc);

			documentFile.Position = pos;
			return cc;
		}

		public bool SeekNextLine()
		{
			int cc = documentFile.ReadByte();
			//Console.WriteLine("SeekNextLine byte: " + cc);
			while ((cc != 10) && (cc != 13) && cc >=0)
			{
				// sb.add(cc);
				cc = documentFile.ReadByte();
			}
			if (cc<0)
				return false;
			// check for CRLF
			if (cc == 13) // CR
			{
			// check for CRLF
				cc = Peek();  // TODO: what if EOF
				if (cc == 10 )
					cc = documentFile.ReadByte();
			}
			currentLine++;
			offsets[currentLine] = documentFile.Position;
			return true;
		}

		public string ReadLine()
		{
			// return null if nothing read.
			StringBuilder sb = new StringBuilder();
			// check for EOF
			int cc = documentFile.ReadByte();

			while ((cc != 10) && (cc != 13) && cc >=0)
			{
				sb.Append((char)cc);
				cc = documentFile.ReadByte();
				//Console.WriteLine("ReadLine byte: " +(char)cc + "(" + cc + ")");
			}
			if (cc < 0)
				return null;
			//sb.AppendLine();
			if (cc == 13) // CR
			{
			// check for CRLF
				cc = Peek();
				if (cc == 10 )
					cc = documentFile.ReadByte();
			}
			currentLine ++;
			offsets[currentLine] = documentFile.Position;

			return sb.ToString();
		}

		public string[] ReadLine (Int64 from, Size pagesize)
		{
			List<string> lineList = new List<string>();
			//StringBuilder lineBuffer = new StringBuilder();
			// TODO: is seekable?
			if (currentLine != from) 
			{
				this.Seek(from);
			}
			Int64 count = 0;
			string line = "";
			while ((count < pagesize.Height) && (line != null))
			{
				// needs EOL
				line = ReadLine(); // EOF anyone?
				// Console.Write(line);
				if (line != null)
				{
					// Console.WriteLine(">>>"+line+"<<<");

					string padline = "".PadRight(pagesize.Width);
					if (line.Length <= pagesize.Width)
						lineList.Add(line.PadRight(pagesize.Width));
					if (line.Length > pagesize.Width)
					{
						// TODO: check not last line in buffer
						lineList.Add(line.Substring(0,pagesize.Width));
						lineList.Add(line.Substring(pagesize.Width));
						count++;
					}
//					lineList.Add(line);

				}
				count++;
			}
		//	lineList.Add("this is a line");
		//	lineList.Add("a line with a \n new line");
			//currentLine += count;

			return lineList.ToArray();
		}
    }
}