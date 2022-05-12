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

		// current line number... top or bottom of window...
		// thinking it was the top
		// document deems line 1 at top of document.

		private int currentLineNumber;
		private int currentColumnNumber;

		private string lastSearch;
		private string statusLine;
		private string highlight;

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
			highlight = "";
			repaintScreen();

		}

		private string[] ReadLine(int cln, int wh)
		{

			//Console.WriteLine("\nreading from document, line: " + cln + " height: " + wh + "\n");

			string[] window = document.ReadLine(cln,wh);

			//Console.WriteLine("\nreturns first: " + window[0] + " length: " + window.Length +"\n");


			// Console.WriteLine("\nFrom: " + cln + " to: " + wh + "\n");

//but where's the cheese, err padding
		//	if (currentColumnNumber == 0)
		//		return window;

			string[] padWindow = new string[wh];

			int count = 0 ;
			foreach (string l in window)
			{
				string subl = l;
				// string padl = l.PadLeft()
				//I was sure I added the window truncation and padding to this class somewhere.
				if (currentColumnNumber < l.Length)
					 subl= l.Substring(currentColumnNumber);

				// I have altered the view, pray I don't alter it further
				// this window view is getting worse all the time.
				if (subl.Length > windowWidth)
					subl= l.Substring(0,windowWidth);

			//	padWindow[count++]  = "" + subl.Length + " " + subl.PadRight(windowWidth,' ');
				padWindow[count++] = subl.PadRight(windowWidth,' ');
			}

			while (count < wh)
				padWindow[count++] = "~".PadRight(windowWidth,' ');
	


			// padWindow[count-1] = "Pad trunc: " + windowWidth;

			// Status("Pad trunc: " + windowWidth+" :");
//Console.WriteLine("\nPad trunc: " + display.WindowWidth() + "\n");

			return padWindow;
		}

		private string[] ReadLine()
		{
			return this.ReadLine(currentLineNumber,windowHeight-1);
		}

		public void repaintScreen() 
		{ 
			string[] window = this.ReadLine();
			this.drawWindow(window,statusLine);
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
			// weird ass conditional behaviour, numeric overrides its primary function\
			try {
				if (moveNumber.Length > 0)
					return Int32.Parse(moveNumber);
				return d;
			} catch (FormatException e) {
				return d;
			}
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
			if (s != null)
			{
				statusLine = s;
				display.drawStatus(statusLine);
			}
		}

		public void Alert()
		{
			System.Console.Beep();
		}
	
		private void drawWindow (string[] page,string status)
		{

			// Console.WriteLine("highlight: '"+highlight+"' status " + status );

			// TODO: handle multiple matches per line
			if (highlight.Length > 0)
				display.drawHighlight(highlight,page,status);
			else
				display.draw(page,status);

		}

		private void moveCurrentLineTo(int lineNumber)
		{

			// this whole method may be off by one.
			// Console.WriteLine("moveCurrentLine To " + lineNumber + " From "+currentLineNumber + "\n\n");

			int old = currentLineNumber;
			currentLineNumber = validateLineNumber(lineNumber);
			statusLine =":";

			int dirLines = currentLineNumber - old;

			if (dirLines==0)
				return;
			// automatically handle foreward and backward movement optimally
			// Console.WriteLine("moveCurrentLine direction " + dirLines + " windowHeight "+windowHeight + "\n\n");

			// the idea here is that we use lines immedately below the window, to scroll the disply up
			// so we need from the old position (+1) to the new position
			// but only if the number of lines to scroll is less than the window height

			if (dirLines > 0 && dirLines < windowHeight)
			{

				//Console.WriteLine("Scroll Current Line To " + currentLineNumber + " by "+dirLines + "\n\n");

				// why would this be currentLineNumber + windowHeight ?
				// because we add to the bottom of the window
				int cline = old+windowHeight+1;

				string[] windowLines = this.ReadLine(cline,dirLines);


				//Console.WriteLine("ReadLine: " + cline + " returns first line: "+windowLines[0] + "\n\n");


				this.drawWindow(windowLines,statusLine);
				return;
			}

			string[] window = ReadLine();
			this.drawWindow(window,statusLine);
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
			repaintScreen();
			currentColumnNumber += windowHalfWidth;
		}

		public void halfWindowLeft(string moveNumber)
		{
			windowHalfWidth = defaultInteger(moveNumber, windowHalfWidth);
			if (currentColumnNumber - windowHalfWidth > 0)
				currentColumnNumber -= windowHalfWidth;
			else
				currentColumnNumber = 0;
			repaintScreen();

		}

		public void findNext(string somethingNeedDoing) {
			//  ESC-n             *  Repeat previous search, spanning files.
			; 
		}
		public void examineFileNext(string moveNumber) { ; }
		public void examineFilePrevious(string moveNumber) { ; }
		public void examineFile(string moveNumber) { ; }
		public void excludeCurrentFile() { ; }

		private bool lineContains(int lineNumber, string find)
		{
			//Console.WriteLine("\n\n contains: '" + find + "' line: " + lineNumber + "\n\n");
			string lineString = document.ReadLine(lineNumber);
			return lineString.Contains(find);
		}

		private int searchFromTo(int fromNumber, int toNumber, string search, bool match)
		{
			//Console.WriteLine("\nSearch range from: " + fromNumber + " to: " + toNumber +"\n\n");
			int step = 1;
			if (toNumber < fromNumber)
				step = -1;

			for (int ln = fromNumber; ln != toNumber; ln+=step)
			{
				if (this.lineContains(ln,search) == match) // just like a chocolate milkshake only it's XNOR
				{
					//Console.WriteLine("\nLine contains: " + ln + "\n");
					return ln;
				}
			}
			return -1;
		}

		private int forwardSearchWhole(int numberOfTimes, string searchFor, bool match)
		{
			int foundLine = 0;

			//Console.WriteLine("forward search 1: " + numberOfTimes + ", for: " + searchFor + ", match: " + match);

			while (numberOfTimes>=1 && foundLine >= 0)
			{
				foundLine = this.searchFromTo(this.currentLineNumber+1,document.Length(),searchFor,match);
				if (foundLine >= 0)
					numberOfTimes--;
			}
			//Console.WriteLine("forward search 2: " + numberOfTimes + ", found: " + foundLine);

			if (foundLine >= 0)
				return foundLine;

			foundLine = 0;
			while (numberOfTimes>=1 && foundLine >= 0)
			{
				foundLine = this.searchFromTo(1,this.currentLineNumber-1,searchFor,match);
				if (foundLine >= 0)
					numberOfTimes--;
			}
			//Console.WriteLine("forward search 3: " + numberOfTimes + ", found: " + foundLine);

			return foundLine;

		}

		private int backwardSearchWhole(int numberOfTimes, string searchFor, bool match)
		{
			int foundLine = 0;

			while (numberOfTimes>=1 && foundLine >= 0)
			{
				foundLine = this.searchFromTo(this.currentLineNumber-1,1,searchFor,match);
				if (foundLine >= 0)
					numberOfTimes--;
			}


			if (foundLine >= 0)
				return foundLine;

			foundLine = 0;
			while (numberOfTimes>=1 && foundLine >= 0)
			{
				foundLine = this.searchFromTo(document.Length(),this.currentLineNumber+1,searchFor,match);
				if (foundLine >= 0)
					numberOfTimes--;
			}

			return foundLine;
		}

		public void SearchForward(string stimes, string search)
		{
 
			if (search.Length > 0)
			{
				//Console.WriteLine("\n\nI don't alwatys search, but when I do, I don't find anything\n");
				lastSearch = search;
				string firstChar = search.Substring(0,1);
				int numberOfTimes = this.defaultInteger(stimes,1);

				if (firstChar == "!")
				{
					// not find.
					string notSearch = search.Substring(1);
					// search from here to end, then start to here

					int fl = this.forwardSearchWhole(numberOfTimes,notSearch,false);
					if (fl > 0)
					{ 
						//this.currentLineNumber = fl;
						this.highlight = "";
						//Console.WriteLine("\n\nmove currentLineNumber to: "+fl);
						this.moveCurrentLineTo(fl);
						return;
					}
					// even if it is found less than the number of times
					statusLine = "pattern not found";
					// Console.WriteLine("\n\n not found, not moving \n\n");

					this.repaintScreen();
				} 
				else if (firstChar == "*") 
				{ 
					; 
				} 
				else if (firstChar == "@") 
				{ 
					; 
				}
				else
				{ 
					// Console.WriteLine("\nsearching forward for whole word\n");
					int fl = this.forwardSearchWhole(numberOfTimes,search,true);
					if (fl > 0)
					{ 
						// this.currentLineNumber = fl;
						this.highlight = search;
					//	Console.WriteLine("\n found at: " + fl + "\n\n");
						this.moveCurrentLineTo(fl);
						return;
					}
					// not found. even if it is found less than the number of times
					this.highlight = "";
					statusLine = "pattern not found";
					//Console.WriteLine("\n not found\n\n");

					this.repaintScreen();
				}
			}
		}

		public void SearchForwardAgain(string stimes)
		{
			SearchForward(stimes,lastSearch);
		}

		public void SearchBackward(string stimes, string search)
		{
			int numberOfTimes = defaultInteger(stimes, 1);
			lastSearch = search;
			string firstChar = search.Substring(0,1);

			if (firstChar == "!")
			{
							// not find.
				string notSearch = search.Substring(1);
				// search from here to end, then start to here

				int fl = this.backwardSearchWhole(numberOfTimes,notSearch,false);
				if (fl > 0)
				{ 
					//this.currentLineNumber = fl;
					this.highlight = "";
					//Console.WriteLine("\n\nmove currentLineNumber to: "+fl);
					this.moveCurrentLineTo(fl);
					return;
				}
				// even if it is found less than the number of times
				statusLine = "pattern not found";
				// Console.WriteLine("\n\n not found, not moving \n\n");

				this.repaintScreen();
			} else if (firstChar == "*") { ; 
			} else if (firstChar == "@") { ; 
			} else { 
				// Console.WriteLine("\nsearching forward for whole word\n");
				int fl = this.backwardSearchWhole(numberOfTimes,search,true);
				if (fl > 0)
				{ 
					// this.currentLineNumber = fl;
					this.highlight = search;
				//	Console.WriteLine("\n found at: " + fl + "\n\n");
					this.moveCurrentLineTo(fl);
					return;
				}
				// not found. even if it is found less than the number of times
				this.highlight = "";
				statusLine = "pattern not found";
				//Console.WriteLine("\n not found\n\n");

				this.repaintScreen();;
			}

		}

		public void SearchBackwardAgain(string stimes)
		{
			SearchBackward(stimes,lastSearch);
		}

		public void drawStatusCursor(string pre,string line,int pos)
		{

			//Console.WriteLine("\n\n pre: " + pre + " line: " + line + " position: " + pos + "\n\n");

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
			int lineNumber = defaultInteger(ll,1);
			moveCurrentLineTo(lineNumber);
		}

		public void moveToEnd(string ll)
		{
			int endLine = 2 + document.Length() - windowHeight ;
			int lineNumber = defaultInteger(ll,endLine);
			moveCurrentLineTo(lineNumber);
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
