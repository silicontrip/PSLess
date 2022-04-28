using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
	public class LessController {

		private LessDocument document;
		private LessDisplay display;
		// private PSHostUserInterface hostui;
		// private string commandLine;
		// private string prefix;

		private int currentLineNumber;
		private int currentColumnNumber;

		private string lastSearch;
		private string statusLine;

		private int windowHeight;
		private int windowWidth;
		private int windowHalfHeight;
		public LessController (LessDocument doc, LessDisplay ld)
		{
			this.document = doc;
			this.display = ld;

			//string[] statusString = new string[] {this.document.getBaseName()};

			currentLineNumber = 1;
			currentColumnNumber = 0;
			//statusInputCount = 0;
			//statusLine = this.document.getBaseName()

			Status(this.document.getBaseName());

			windowWidth = display.WindowWidth();
			windowHeight = display.WindowHeight();
			windowHalfHeight = windowHeight / 2;
			repaintScreen();

		}

		private string[] ReadLine()
		{
			string[] window = document.ReadLine(currentLineNumber,windowHeight);

			if (currentColumnNumber == 0)
				return window;

			string[] padWindow = new string[window.Length];

			int count = 0 ;
			foreach (string l in window)
				padWindow[count++] = l.Substring(currentColumnNumber);

			return padWindow;
		}

		public void repaintScreen() 
		{ 
			string[] window = ReadLine();
			display.draw(window,statusLine);
		}


		private int validateLineNumber(int ln)
		{
			// don't move before start of document.
			// don't move past end of document but allow partial display

			if (ln > document.Length())
			{
				Alert(); 
				return document.Length();
			}
			if (ln < 1)
			{
				Alert();
				return 1;
			}
			return ln;
		}
	
		private int defaultInteger (string moveNumber, int d)
		{
			// weird ass conditional behaviour, numeric overrides its primary function
			if (moveNumber.Length > 0)
				return Int32.Parse(moveNumber);
			return d;
		}
// this should only take a single line of text
/*
		private void scroll(string s) { 
			string line = document.ReadLine(currentLineNumber+windowHeight,1);
			display.draw(line,s);
		}
*/
		public KeyInfo ReadKey()
		{
			return display.ReadKey();
		}

		public void setWindowHeight(string moveNumber)
		{
			int newHeight = Int32.Parse(moveNumber);
			if (newHeight >= 1 && newHeight <= document.Length())
				windowHeight = newHeight;
		}
		public void setWindowHalfHeight(string moveNumber)
		{
			int newHeight = Int32.Parse(moveNumber);
			if (newHeight >= 1 && newHeight <= document.Length())
				windowHalfHeight = newHeight;
		}
		public void displayCurrentFileDetails()
		{
			// less-help.txt lines 78-141/236 byte 7335/11925 61%  (press RETURN)

			int bottom = currentLineNumber + windowHeight;
			int total = document.Length();

			display.drawStatus(document.FileName() + " lines " + currentLineNumber +"-" +bottom+"/"+total);
			// wait for return
		}

		public void Status(string s)
		{
			statusLine = s;
			display.drawStatus(statusLine);
		}

		public void Alert()
		{
			System.Console.Beep();
		}
	
		public void moveCurrentLineTo(int lineNumber)
		{
			int old = currentLineNumber;
			currentLineNumber = validateLineNumber(lineNumber);
			statusLine =":";
			if (currentLineNumber != old)
				repaintScreen();
		}

		private void forward(int move)
		{
			// special case as screen scroll upwards is natural
			//int move = Int32.Parse(moveNumber);
			currentLineNumber =  validateLineNumber(currentLineNumber + move); // validate
			string[] window = document.ReadLine(currentLineNumber+windowHeight,move);

		//Console.WriteLine("count: " + window.Length);

			display.draw(window,statusLine);
		}

		public void oneLineForward(string moveNumber)  // or multiple lines
		{
			forward(defaultInteger(moveNumber,1));
		}

		public void oneLineBackward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber - defaultInteger(moveNumber,1));
		}

		public void oneWindowForward(string moveNumber)
		{
			//moveCurrentLineTo(currentLineNumber + defaultInteger(moveNumber,windowHeight));
			forward(defaultInteger(moveNumber,windowHeight));
		}

		public void oneWindowBackward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber - defaultInteger(moveNumber,windowHeight));
		}

		public void halfWindowForward()
		{
			// moveCurrentLineTo(currentLineNumber + windowHalfHeight);
			forward (windowHalfHeight);
		}

		public void halfWindowBackward()
		{
			moveCurrentLineTo(currentLineNumber - windowHalfHeight);
		}

		public void halfWindowRight(string moveNumber)
		{
			currentColumnNumber += defaultInteger(moveNumber, windowWidth / 2);
		}

		public void halfWindowLeft(string moveNumber)
		{
			int numeric = defaultInteger(moveNumber, windowWidth / 2);
			if (currentColumnNumber - numeric > 0)
				currentColumnNumber -= numeric;
			else
				currentColumnNumber = 0;
		}

		public void findNext(string somethingNeedDoing) { ; }
		public void examineFileNext(string moveNumber) { ; }
		public void examineFilePrevious(string moveNumber) { ; }
		public void examineFile(string moveNumber) { ; }
		public void excludeCurrentFile() { ; }

		public void drawStatusCursor(string pre,string line,int pos)
		{
			pos += pre.Length;
			string status = pre + line;
			display.drawStatusCursor(status,pos);
		}
	}
}
