using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
	public class LessDisplay {

		private PSHostUserInterface hostui;
        // public string StatusLine {get; set;} // should this be handled by us?
        public ConsoleColor ForegroundStatusColour { get; set; }
        public ConsoleColor BackgroundStatusColour  { get; set; }

		public LessDisplay (PSHostUserInterface rui)
		{
			this.hostui = rui;
            ForegroundStatusColour = ConsoleColor.Yellow;
            BackgroundStatusColour = ConsoleColor.Black;
        }

		public KeyInfo ReadKey() { return hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp); }
		public ConsoleColor Background() { return hostui.RawUI.BackgroundColor; }
		public ConsoleColor Foreground() { return hostui.RawUI.ForegroundColor; }

		public Size WindowSize() { return hostui.RawUI.WindowSize; }
		public int WindowWidth() { return hostui.RawUI.WindowSize.Width; }
		public int WindowHeight() { return hostui.RawUI.WindowSize.Height; }
		public int PageHeight() { return hostui.RawUI.WindowSize.Height - 1; }

		private string padLine(string line) 
		{

			// do something smart with currentColumnNumber
			// in the controller... not here 
			
			// if(currentColumnNumber > 0)
			//	line = line.Substring(currentColumnNumber, line.Length - currentColumnNumber);

			if (line.Length > WindowWidth())
				return line.Substring(WindowWidth());

			return line.PadRight(WindowWidth());
		}

		public void draw (string[] line, string StatusLine)
		{
			this.hostui.Write("\r");

			foreach (string ll in line)
				this.hostui.WriteLine(Foreground(),Background(),padLine(ll));

			//this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, padLine(StatusLine));
			this.drawStatus(StatusLine);
		}

// should probably draw Status position for all
        public void drawStatus(string StatusLine) {
			int Position = StatusLine.Length;
            this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, padLine(StatusLine));
			this.hostui.Write("\r");
			string truncated = StatusLine.Substring(0,Position);
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, truncated);
        }

		public void clearStatus()
		{
			this.hostui.Write("\r");
			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, padLine("")); // or not inverted
		}

		public void drawStatusCursor(string StatusLine, int Position)
		{
			this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, padLine(StatusLine));
			this.hostui.Write("\r");
			string truncated = StatusLine.Substring(0,Position);
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, truncated);
		}

    }
}