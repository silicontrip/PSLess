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

		public void draw (string[] line, string StatusLine)
		{
			this.hostui.Write("\r");

			foreach (string ll in line)
				this.hostui.WriteLine(Foreground(),Background(),ll);

			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, StatusLine);
		}

        public void drawStatus(string StatusLine) {
            this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, StatusLine);
        }

		public void clearStatus()
		{
			this.hostui.Write("\r");
			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, "".PadRight(WindowWidth())); // or not inverted
		}

    }
}