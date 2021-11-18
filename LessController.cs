using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
	public class LessController {

		private TextDocument document;
		private PSHostRawUserInterface interface;
		private ConsoleColor colourFore; 
		private ConsoleColor colourBack;
		// find a better name than Screen
		private Size sizeScreen;
		private Size sizeStatus;
		private Coordinates originScreen;
		private Coordinates originStatus;
		private BufferCell cellClear;
		private BufferCell[,] bcaStatus;
		private BufferCell[,] bcaScreen;
		private Rectangle rectScreen;
		private Rectangle rectStatus;

		int pageSize;
		int currentLineNumber;
		string lastSearch;

		public LessController (TextDocument doc, PSHostRawUserInterface rui)
		{
			this.document = doc;
			this.interface = rui;

			this.colourBack = this.interface.BackgroundColor;
			this.colourFore = this.inteface.ForegroundColor;

			Size windowFrame  = this.interface.WindowSize;
			originScreen = new Coordinates(0,0);
			originStatus = new Coordinates(0,windowFrame.Height - 1);

			this.cellClear = new BufferCell (' ',this.colourFore,this.colourBack,0);

			string[] statusString = new string[] {this.document.getBaseName()};
			bcaStatus = this.interface.NewBufferCellArray(statusString,this.colourBack,this.colourFore);  // bg/fg inverted
			bcaScreen = this.interface.NewBufferCellArray(windowFrame.Width,windowFrame.Height-1,cellClear);

			rectScreen = new Rectangle(0,0,windowFrame.Width,windowFrame.Height-2);
			rectStatus = new Rectangle(0,windowFrame.Height-1,windowFrame.Width,windowFrame.Height);

			sizeScreen = new Size(windowFrame.Width,windowFrame.Height -1);
			currentLineNumber = 1;
		}

		public void draw ()
		{

			string[] statusString = new string[] {this.document.getBaseName()};
			bcaStatus = this.interface.NewBufferCellArray(statusString,this.colourBack,this.colourFore);
			string[] line = document.ReadLine(currentLineNumber,sizeScreen);

			BufferCell[,] back =interface.NewBufferCellArray(line,Fg,Bg);

			interface.SetBufferContents(screenOrigin,back);
			interface.SetBufferContents(statusOrigin,statusLine);
		}

		public bool command()
		{
			KeyInfo ki = interface.ReadKey(ReadKeyOptions.NoEcho);
			bool changed = false;
				// ui.WriteLine(ki.ToString());
				// commands
				if (ki.Character == 'q')
					return false;

				else if (ki.Character == ' ') { 
					currentLineCount += sizey - 1;  // handled by textdocument or WindowBuffer.  ooo that's us.
					changed = true;
				}
				else if (ki.Character == 'b') {
					currentLineCount -= sizey - 1;
					changed = true;
				} 
				else if (ki.VirtualKeyCode == 40)
				{
					currentLineCount ++;
					changed = true;
				}
				else if (ki.VirtualKeyCode == 38)
				{
					currentLineCount --;
					changed = true;
				}
				else 
					Console.WriteLine( ki.VirtualKeyCode );

				if (changed)
				{
					try {
						line = doc.ReadLine(currentLineCount,tz);  // maybe throw on IO error
				// Console.WriteLine("lines read: " + line.Length);
				// Console.WriteLine(" line array: " + line);

						back =rui.NewBufferCellArray(line,Fg,Bg);

					// rui.SetBufferContents(screenOrigin,clearScreen);
						rui.SetBufferContents(screenOrigin,back);
						rui.SetBufferContents(statusOrigin,statusLine);
					} catch (Exception e) {
						// terminal beep
					}
				}
		}

	}
}

