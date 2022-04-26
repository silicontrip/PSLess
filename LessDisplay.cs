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

		public ConsoleColor Background() { return hostui.RawUI.BackgroundColor; }
		public ConsoleColor Foreground() { return hostui.RawUI.ForegroundColor; }

		public Size windowSize() { return hostui.RawUI.WindowSize; }
		public int windowWidth() { return hostui.RawUI.WindowSize.Width; }
		public int windowHeight() { return hostui.RawUI.WindowSize.Height; }
		public int pageHeight() { return hostui.RawUI.WindowSize.Height - 1; }

		private string widthLine(string line) 
		{
			if (line.Length > windowWidth())
				return line.Substring(windowWidth());

			return line.PadRight(windowWidth());
		}

		public void draw (string[] line, string StatusLine)
		{

			this.hostui.Write("\r");

			foreach (string ll in line)
				this.hostui.WriteLine(Foreground(),Background(),widthLine(ll));

			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, widthLine(StatusLine));
		}

        public void drawStatus(string StatusLine) {
            this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, widthLine(StatusLine));
        }

		public void clearStatus()
		{
			this.hostui.Write("\r");
			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, widthLine("")); // or not inverted
		}

    }
}