using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Management.Automation.Host;

namespace net.ninebroadcast {


	public abstract class LessDocument
	{
		public abstract int Length();
		public abstract string FileName(); 
		public abstract string ReadLine(int line);
		public abstract string[] ReadLine (int from, int lineCount);

	}


	public class LessDocumentFile : LessDocument
	{

		private Dictionary<Int64,Int64> offsets;
		private string filename;
		private FileStream documentFile;
	//	private StreamReader documentStream;
	//	private bool unsaved;
		private Encoding readWriteEncoding;
		private Int64 currentLine;  //used for line seeking.
		private int length;

		public LessDocumentFile()
		{
		//	buffer = new List<string>();
			this.filename="";
			documentFile = null;
		//	unsaved=false;
		}

		public LessDocumentFile(string filename)
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
			countLines();
		}

		public override int Length () { return length; }
		public override string FileName() { return filename; }

		private void countLines() { 
			length = 0;
			while (SeekNextLine()) 
				length++;
			documentFile.Seek(0, SeekOrigin.Begin);
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
		public void Seek(int line)
		{
			Int64? pos = offsets[line];
			// handle lines that arent in the offset array.
			if (pos.HasValue)
			{
				// Console.WriteLine("\n\n Seeking to line: " + line +"/" +pos);
				documentFile.Seek(pos.Value,SeekOrigin.Begin);
			}
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
				cc = documentFile.ReadByte();  // Dave Cutler's biggest nightmare, read a byte, read a byte, read a byte byte byte
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

		public override string ReadLine(int line)
		{
			Seek(line);
			return ReadLine();
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

		public override string[] ReadLine (int from, int lineCount)
		{
			List<string> lineList = new List<string>();
			if (from < this.Length())
			{

				//Console.WriteLine("\n document read from: " + from + " < length: " + this.Length() + " count: " + lineCount + "\n");

				//StringBuilder lineBuffer = new StringBuilder();
				// TODO: is seekable?
				if (currentLine != from) 
				{
					this.Seek(from);
				}

				Int64 count = 0;
				string line = "";

				line = ReadLine(); // EOF anyone?

				while ((count < lineCount) && (line != null))
				{
					lineList.Add(line);
					count++;

				// needs EOL
					line = ReadLine(); // EOF anyone?
				}
			}
			return lineList.ToArray();
		}
    }

	public class LessDocumentPipeline : LessDocument
	{

		List<string>puffer; // they are much cuter than buffers

		public LessDocumentPipeline()
		{
			puffer = new List<string>();
		}

		public override int Length() { return puffer.Count; }
		public override string FileName() { return ":"; }

		public override string ReadLine(int l)
		{
			return puffer[l];
		}

		public override string[] ReadLine(int l, int count)
		{
			return puffer.GetRange(l,count).ToArray();
		}

		public void AddLine (string l)
		{
			puffer.Add(l);
		}
	}

}