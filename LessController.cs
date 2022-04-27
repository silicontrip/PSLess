using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
	public class LessController {

		private TextDocument document;
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

		public LessController (TextDocument doc, LessDisplay ld)
		{
			this.document = doc;
			this.display = ld;
			// this.hostui = h;

			//string[] statusString = new string[] {this.document.getBaseName()};

			//commandLine = ""; 
			//prefix = "";
			currentLineNumber = 1;
			//statusInputCount = 0;

			Status(this.document.getBaseName());

			windowWidth = display.WindowWidth();
			windowHeight = display.WindowHeight();
		}

		public void setWindowHeight(string moveNumber)
		{
			int newHeight = Int32.Parse(moveNumber);
			if (newHeight >= 1 && newHeight <= document.Length())
				windowHeight = newHeight;
		}

		private void displayDocument(string s) 
		{ 
			// display.StatusLine = s;

			string[] window = document.ReadLine(currentLineNumber,windowHeight);
			string[] padWindow = new string[window.Length];

			int count = 0 ;

			foreach (string l in window)
				padWindow[++count] = padLine(l);

			display.draw(window,s);
		}

		private string padLine(string line) 
		{

			// do something smart with currentColumnNumber

			if (line.Length > windowWidth)
				return line.Substring(windowWidth);

			return line.PadRight(windowWidth);
		}

// this should only take a single line of text
/*
		private void scroll(string s) { 
			string line = document.ReadLine(currentLineNumber+windowHeight,1);
			display.draw(line,s);
		}
*/
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
			return currentLineNumber;
		}
	
		private int defaultInteger (string moveNumber, int d)
		{
			// weird ass conditional behaviour, numeric overrides its primary function
			if (moveNumber.Length > 0)
				return Int32.Parse(moveNumber);
			return d;
		}
		public void moveCurrentLineTo(int lineNumber)
		{
			int old = currentLineNumber;
			currentLineNumber = validateLineNumber(lineNumber);
			if (currentLineNumber != old)
				displayDocument(":");
		}

		public void oneLineForward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber + defaultInteger(moveNumber,1));
		}

		public void oneLineBackward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber - defaultInteger(moveNumber,1));
		}

		public void oneWindowForward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber + defaultInteger(moveNumber,windowHeight));
		}

		public void oneWindowBackward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber - defaultInteger(moveNumber,windowHeight));
		}

		public void findNext(string somethingNeedDoing) { ; }
		public void examineFileNext(string moveNumber) { ; }
		public void examineFilePrevious(string moveNumber) { ; }
		public void examineFile(string moveNumber) { ; }
		public void excludeCurrentFile() { ; }
	}
}
