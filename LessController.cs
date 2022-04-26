using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
	public class LessController {

		private TextDocument document;
		private LessDisplay display;
		private PSHostUserInterface hostui;
		private string commandLine;
		private string prefix;

		int currentLineNumber;
		string lastSearch;
		string statusLine;

		int statusInputCount;

		public LessController (TextDocument doc, PSHostUserInterface h, LessDisplay ld)
		{
			this.document = doc;
			this.display = ld;
			this.hostui = h;

			string[] statusString = new string[] {this.document.getBaseName()};

			commandLine = ""; 
			prefix = "";
			currentLineNumber = 1;
			statusInputCount = 0;

			updateStatus(this.document.getBaseName());
		}

		private void displayDocument(string s) 
		{ 
			display.StatusLine = s;
			display.redraw(document.ReadLine(currentLineNumber,display.pageHeight()));
		}

		private void scroll() { 
			display.scroll(document.ReadLine(currentLineNumber,display.pageHeight()));
		}

		//	private void setStatus(string s)
		//{
		//	display.StatusLine = s;
		//}

		private void displayStatus(string s)
		{
			display.StatusLine = s;
			display.drawStatus();
		}

		private void alert()
		{
			display.StatusLine = ":";
			System.Console.Beep();
		}
	
		private bool moveCurrentLine(int moveNumber)
		{
			int old = currentLineNumber;
			currentLineNumber += moveNumber;

			// don't move before start of document.

			// don't move past end of document but allow partial display

			if (currentLineNumber > document.Length())
			{
				currentLineNumber = old; return false;
			}
			if (currentLineNumber < 1)
			{
				currentLineNumber = old; return false;
			}
			return true;
		}

		public bool command()
		{

			//for(;;)
			//{
			KeyInfo ki = hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp);

			// printable
			// unprintable
			// empty commandLine
			// commandline length > 0

			/* prefix commands

			numerals : ESC / & m ' ^X - _ _ + !

			*/

			

/*
: numeral -> error
ESC numeral -> error

*/

		// business logic
		// it really shouldn't be in a bunch of else ifs
		// it's not always just a simple case of key press
		// there is state based on the contents of the command line and statusInputCount

			if (ki.Character >='0' && ki.Character <='9') {
				//commandLine = commandLine + ki.Character;
				if (commandLine != ":") {
				statusInputCount = StatusInputCount * 10 + Int.parseInt(ki.Character);
				commandLine = ":" + statusInputCount;
				displayStatus(":"+commandLine);
			} else if (ki.Character==':') {
				commandLine = ":";
				statusInputCount = 0;
				displayStatus(" " + commandLine);
			} else if (ki.Character == 'Z') {
				if (commandLine == "Z")
				{
					// prefix numeral

					displayStatus("");
					return false;
				} else if (commandLine.Length==0) {
					commandLine="Z";
				} else {
					alert();
					commandLine = "";
				}
			} else if (ki.VirtualKeyCode==27) {
				commandLine = "ESC";
				displayStatus(" " + commandLine);
			} else {

				switch(ki.VirtualKeyCode)
				{
					case 8: // delete
						if (commandLine.Length >0) {
							commandLine = commandLine.Substring(0, commandLine.Length -1);
							displayStatus(commandLine);
						} else {
							alert();
						}
						break;
					case 40: // cursor down

						default:
							switch (ki.Character)
							{
								case 'q':
								case 'Q':
									displayStatus("");  // erase bottom line
									return false;
								case ' ':
									// move X lines or 1 page (or less if end of document)
									if(moveCurrentLine(display.pageHeight()))
										displayDocument(":");
									else
									{
										displayStatus(":");
										alert();
									}
									break;
								case 'b':
									if(moveCurrentLine(-display.pageHeight()))
										displayDocument(":");
									else
									{
										displayStatus(":");
										alert();
									}
									break;
								case 'e':
								case 'j':

								default:
									commandLine="";
									displayStatus("(" + ki.VirtualKeyCode + "/"+ ki.ControlKeyState +")" + ":");
									alert();
									break;
							}
							break;
				}

			}
				// Console.WriteLine("(" + ki.VirtualKeyCode +")" );

			return true;
		}

	}
}
