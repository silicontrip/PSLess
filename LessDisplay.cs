using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
	public class LessDisplay {

		private PSHostUserInterface hostui;
        public string StatusLine {get; set; }
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


		public void redraw (string[] line)
		{

			this.hostui.Write("\r");

			foreach (string ll in line)
				this.hostui.WriteLine(Foreground(),Background(),widthLine(ll));

			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, widthLine(StatusLine));
		}

        public void drawStatus() {
            this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, widthLine(StatusLine));
        }

		private void clearStatus()
		{
			this.hostui.Write("\r");

		}

        private void updateStatus(string status) 
        {
            StatusLine = status;
            this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, widthLine(StatusLine));
        }

// this method handles a specific case want to rename it to it's more correct action

		public void scroll(string[] line)
		{

			this.hostui.Write("\r");
			string ll = line[line.Length - 1];

			this.hostui.WriteLine(Foreground(),Background(),ll);
			this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour , StatusLine);

		}

    }
}