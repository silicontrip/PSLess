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
		private int windowHalfWidth;
		private Dictionary<char,string>marks;
		public LessController (LessDocument doc, LessDisplay ld)
		{
			this.document = doc;
			this.display = ld;

			//string[] statusString = new string[] {this.document.getBaseName()};

			currentLineNumber = 1;
			currentColumnNumber = 0;

			marks = new Dictionary<char, string>();

			//statusInputCount = 0;
			//statusLine = this.document.getBaseName()

			Status(this.document.getBaseName());

			windowWidth = display.WindowWidth();
			windowHeight = display.WindowHeight();
			windowHalfHeight = windowHeight / 2;
			windowHalfWidth = windowWidth / 2;
			repaintScreen();

		}

		private string[] ReadLine(int cln, int wh)
		{
			string[] window = document.ReadLine(cln,wh);

			if (currentColumnNumber == 0)
				return window;

			string[] padWindow = new string[window.Length];

			int count = 0 ;
			foreach (string l in window)
				padWindow[count++] = l.Substring(currentColumnNumber);

			return padWindow;
		}

		private string[] ReadLine()
		{
			return ReadLine(currentLineNumber,windowHeight);
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
	
		private void moveCurrentLineTo(int lineNumber)
		{
			int old = currentLineNumber;
			currentLineNumber = validateLineNumber(lineNumber);
			statusLine =":";

			int dirLines = currentLineNumber - old;

			if (dirLines==0)
				return;
			// automatically handle foreward and backward movement optimally

			if (dirLines > 0 && dirLines < windowHeight)
			{
				string[] windowLines = ReadLine(currentLineNumber+windowHeight,dirLines);
				display.draw(windowLines,statusLine);
				return;
			}

			string[] window = ReadLine();
			display.draw(window,statusLine);
		}


		public void oneLineForward(string moveNumber)  // or multiple lines
		{
			moveCurrentLineTo (currentLineNumber + defaultInteger(moveNumber,1));
		}

		public void oneLineBackward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber - defaultInteger(moveNumber,1));
		}

		public void oneWindowForward(string moveNumber)
		{
			//moveCurrentLineTo(currentLineNumber + defaultInteger(moveNumber,windowHeight));
			moveCurrentLineTo (currentLineNumber +defaultInteger(moveNumber,windowHeight));
		}

		public void oneWindowBackward(string moveNumber)
		{
			moveCurrentLineTo(currentLineNumber - defaultInteger(moveNumber,windowHeight));
		}

		public void halfWindowForward()
		{
			// moveCurrentLineTo(currentLineNumber + windowHalfHeight);
			moveCurrentLineTo (currentLineNumber + windowHalfHeight);
		}

		public void halfWindowBackward()
		{
			moveCurrentLineTo(currentLineNumber - windowHalfHeight);
		}

		public void halfWindowRight(string moveNumber)
		{
			windowHalfWidth = defaultInteger(moveNumber, windowHalfWidth);
			currentColumnNumber += windowHalfWidth;
		}

		public void halfWindowLeft(string moveNumber)
		{
			windowHalfWidth = defaultInteger(moveNumber, windowHalfWidth);
			if (currentColumnNumber - windowHalfWidth > 0)
				currentColumnNumber -= windowHalfWidth;
			else
				currentColumnNumber = 0;
		}

		public void findNext(string somethingNeedDoing) {
			//  ESC-n             *  Repeat previous search, spanning files.
			; 
		}
		public void examineFileNext(string moveNumber) { ; }
		public void examineFilePrevious(string moveNumber) { ; }
		public void examineFile(string moveNumber) { ; }
		public void excludeCurrentFile() { ; }

	
		public void SearchForward(string stimes, string search)
		{
			int times = defaultInteger(stimes, 1);
			lastSearch = search;
			string firstChar = search.Substring(0,1);

			if (firstChar == "!")
			{
				;
			} else if (firstChar == "*") { ; }
			else if (firstChar == "@") { ; }
			else { ; }

		}

		public void SearchForwardAgain(string stimes)
		{
			SearchForward(stimes,lastSearch);
		}

		public void SearchBackward(string stimes, string search)
		{
			int times = defaultInteger(stimes, 1);
			lastSearch = search;
			string firstChar = search.Substring(0,1);

			if (firstChar == "!")
			{
				;
			} else if (firstChar == "*") { ; }
			else if (firstChar == "@") { ; }
			else { ; }

		}

		public void SearchBackwardAgain(string stimes)
		{
			SearchForward(stimes,lastSearch);
		}


		public void drawStatusCursor(string pre,string line,int pos)
		{

		Console.WriteLine("\n\n pre: " + pre + " line: " + line + " position: " + pos + "\n\n");


			pos += pre.Length;
			string status = pre + line;
			display.drawStatusCursor(status,pos);
		}

		public void setMark(char mark)
		{
			marks[mark] = ""+currentLineNumber;
			// clear off prompt
		}
		public void gotoMark(char mark)
		{
			string newLine = marks[mark];
			int line = defaultInteger(newLine,currentLineNumber);
			moveCurrentLineTo(line);
		}

		public void moveToStart(string ll)
		{
			int lineNumber = defaultInteger(ll,0);
			moveCurrentLineTo(lineNumber);
		}

		public void moveToEnd(string ll)
		{
			int lineNumber = defaultInteger(ll,0);
			moveCurrentLineTo(document.Length());
		}

		public void movePercent(string pp)
		{
			int per = defaultInteger(pp,0);
			int lineNumber = document.Length() * per / 100;
			moveCurrentLineTo(lineNumber);
		}

// funny how I don't know what feature a command has until trying to implement it...
// I had no idea what a tag was and I seriously doubt I'll ever use one
/*
       -ttag or --tag=tag
              The -t option, followed immediately by a TAG, will edit the file containing that tag.  For this to work, tag information must be available; for example, there may be a file in  the
              current directory called "tags", which was previously built by ctags (1) or an equivalent command.  If the environment variable LESSGLOBALTAGS is set, it is taken to be the name of
              a command compatible with global (1), and that command is executed to find the tag.  (See http://www.gnu.org/software/global/global.html).  The -t option may also be specified from
              within less (using the - command) as a way of examining a new file.  The command ":t" is equivalent to specifying -t from within less.
*/
		public void gotoPreviousTag(string pp)
		{ 
			; // unimplemented
		}

		public void gotoNextTag(string pp)
		{ 
			; // unimplemented
		}

		private int lineFind (string line, char inc, char dec)
		{
			int count = 0;
			foreach (char  cc in line)
			{
				if (cc==inc)
					count++;
				if (cc==dec)
					count--;
			}
			return count;
		}

		private void stackedMatchMove(char b1, char b2, int start, int direction)
		{
			int searchPos = start;
			string thisLine = document.ReadLine(searchPos);
			int stack = lineFind(thisLine,b1,b2);
			while (stack>0)
			{
				searchPos += direction;
				thisLine = document.ReadLine(searchPos);
				stack += lineFind(thisLine,b1,b2);
			}
			moveCurrentLineTo(searchPos);
		}

		public void findClose(string num, char key)
		{
			// check currentLine for key
			// count instances of matching brackets

			if (key == '{')
				stackedMatchMove('{','}',currentLineNumber,1);
			if (key == '[')
				stackedMatchMove('[',']',currentLineNumber,1);
			if (key == '(')
				stackedMatchMove('(',')',currentLineNumber,1);

		}

		public void findOpen(string num, char key)
		{
			if (key == '}')
				stackedMatchMove('}','{',currentLineNumber+windowHeight,-1);
			if (key == ']')
				stackedMatchMove(']','[',currentLineNumber+windowHeight,-1);
			if (key == ')')
				stackedMatchMove(')','(',currentLineNumber+windowHeight,-1);
		}

	}
}
