using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace net.ninebroadcast {

	public class TextDocument {

		private List<string> buffer;
		private string filename;
	//	private bool unsaved;
		private Encoding readWriteEncoding;

		public Document()
		{
			buffer = new List<string>();
			this.filename="";
			readWriteEncoding = new ASCIIEncoding();

		//	unsaved=false;
		}

		public Document(string filename)
		{
			readWriteEncoding = new ASCIIEncoding();
			try {
				string absoluteFn = Path.GetFullPath(filename);
				buffer = new List<string>(File.ReadAllLines(absoluteFn,readWriteEncoding));
			} catch (FileNotFoundException) {
				buffer = new List<string>();
			}
			this.filename = filename;
		}
    }
}