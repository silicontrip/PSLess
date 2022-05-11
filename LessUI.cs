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

        public ConsoleColor ForegroundContentColour { get; set; }
        public ConsoleColor BackgroundContentColour  { get; set; }

		public ConsoleColor ForegroundHighlightColour { get; set; }
		public ConsoleColor BackgroundHighlightColour { get; set; }
		public LessDisplay (PSHostUserInterface rui)
		{
			this.hostui = rui;
            ForegroundStatusColour = ConsoleColor.Yellow;
            BackgroundStatusColour = (ConsoleColor)(-1);

			ForegroundContentColour = (ConsoleColor)(-1);
			BackgroundContentColour = (ConsoleColor)(-1);

			ForegroundHighlightColour = ConsoleColor.Yellow;
			BackgroundHighlightColour = ConsoleColor.Black;
		//	Console.WriteLine("\n\nBackground colour: " + Console.BackgroundColor);
		//	Console.WriteLine("\n\nForeground colour: " + Console.ForegroundColor);

        }

// the shift keyup (released after colon) is being processed
		public KeyInfo ReadKey() { return hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyDown); }
		public ConsoleColor Background() { return BackgroundContentColour; }
		public ConsoleColor Foreground() { return ForegroundContentColour; }

		public ConsoleColor HilightBackground() { return BackgroundHighlightColour; }
		public ConsoleColor HilightForeground() { return ForegroundHighlightColour; }

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

		public void drawHighlight(string match, string[] line, string StatusLine)
		{

			// multiple matches... ?
			this.hostui.Write("\r");

			foreach (string ll in line)
			{
				string ss = this.padLine(ll);
				if (ss.Contains(match))
				{
					int endPos=-1;
					string sub;

					int idx = ss.IndexOf(match);
					while (idx > -1)
					{
/*					
					Console.WriteLine("highlight found: " +idx);
					Console.WriteLine("substring 1: " + ss.Substring(0,idx));
					Console.WriteLine("substring 2: " + ss.Substring(idx,match.Length));
					Console.WriteLine("substring 3: " + ss.Substring(idx+match.Length));
*/
				//	Console.WriteLine("fg: " + Foreground() + "  bg: "+Background() );

						sub = ss.Substring(0,idx); 
						this.hostui.Write(Foreground(),Background(),sub);
						sub = ss.Substring(idx,match.Length); 
						this.hostui.Write(HilightForeground(),HilightBackground(),sub);
						endPos = idx+match.Length;
						if (endPos < ss.Length)
							idx = ss.IndexOf(match,endPos);
						else
							idx = -1;
					}
					// due to the above conditions this will never evaluate to false
					// but the compiler thinks that endPos still could be unassigned
					// if endPos isn't assigned a value outside of the while loop

					//if (endPos > -1) {
						sub = ss.Substring(endPos);
						this.hostui.Write(Foreground(),Background(),sub);
						this.hostui.Write(Foreground(),Background(),"\n");
					//}
// multiple matches per line?
				} else {

					//Console.WriteLine("No highlight found");

					this.hostui.WriteLine(Foreground(),Background(),ss);
				}
			}

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
			//Console.WriteLine("\n\n Status: " + StatusLine + " (" + StatusLine.Length + ") position: " + Position + "\n\n");
			this.hostui.Write("\r");
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, padLine(StatusLine));
			this.hostui.Write("\r");
			string truncated = StatusLine.Substring(0,Position);
            this.hostui.Write(ForegroundStatusColour, BackgroundStatusColour, truncated);
		}

    }
}