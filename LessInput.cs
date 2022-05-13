using System.Collections.Generic;
// using System.Management.Automation;
using System.Management.Automation.Host;
using System;

namespace net.ninebroadcast 
{

	public abstract class LessInput
	{
		protected LessController controller;
		// protected PSHostUserInterface hostui;
		public virtual LessInput beginParse() { return null; }
	}

// Handle ZZ
	public class ExitInput : LessInput 
	{

		public ExitInput (LessController lc, string n) {
			controller = lc;
			//hostui = ui;
			controller.Status(" Z");		
		}

		public override LessInput beginParse () { 
			// state logic goes here;
			KeyInfo ki = controller.ReadKey();
			if (ki.Character == 'Z')
				return null;

			controller.Alert();
			return new DefaultInput(controller,"");
		}
	}

	public class DefaultInput : LessInput 
	{

		private string numericInput;

		public DefaultInput (LessController lc, string n) 
		{
			controller = lc;
			//hostui = ui;
			numericInput = n;
			controller.Status(":");
		}

		public override LessInput beginParse () 
		{ 

// of course some of the first character conditional changes use control-keys
			Dictionary<string, string> searchPrompts = new Dictionary<string, string>
			{
				{ "!", "Non-match" },
				{ "*", "EOF-ignore" },
				{ "@", "First-file" }
			};

			// state logic goes here;
			// thinking that this might come from the LessController (which inturn passes to Display)
			KeyInfo ki = controller.ReadKey();

			// we stay here until a non numeric key is pressed.
			// include delete and ^U
			// there is a whole section at the bottom of the help document
			// detailing the available line editing commands.
			// implementation in the LineInput class

// handle backspace
			while (ki.Character >='0' && ki.Character <='9') 
			{
				numericInput = numericInput + ki.Character;
				controller.Status(":"+numericInput);
				ki = controller.ReadKey();
			}

			char key = ki.Character;
			int code = ki.VirtualKeyCode;
			ControlKeyStates control = ki.ControlKeyState;  // this returns no data on Mac (or mono maybe?)

			if (key>=' ')
				code = '\0';

			//Host.UI.WriteDebugLine("key: " + key +"(" + (int)key + ") code: "+code+" control: "+control);

			// change state option

			// check for control key and modify code appropriately
			// if (ki.modifierFlags)
			//Console.WriteLine("key: " + key +"(" + (int)key + ") code: "+code+" control: "+control);

			if (key == ':') {
				return new ColonInput(controller,numericInput);
			}
			if (key == 'q' || key == 'Q') 
				return null;

			if (key == 'Z')
				return new ExitInput(controller,numericInput); // not sure if we should make the constructors the same.

			if (code==27)
				return new EscapeInput(controller,numericInput);

			if (key=='=' || key == 7 || code == 'G')  // or ^G :f
			{
				// example line:
				// less-help.txt lines 78-141/236 byte 7335/11925 61%  (press RETURN)
				controller.displayCurrentFileDetails();
				// TODO: Press return Input
				// return new ReturnInput(new DefaultInput(controller,""));
				// seems like less doesn't follow this requirement
				return new DefaultInput(controller,"");
			}	

// these commands take an optional numeric argument.
// their behaviour changes when a number is involved.
//  e  ^E  j  ^N  CR  *  Forward  one line   (or N lines).                                                             

			if (key=='e' || key=='j' || code==13 || code==40 || code=='e' || code=='n')
			{
				//controller.resetStatus();
				// Console.WriteLine("Foreword: " + numericInput);
				controller.oneLineForward(numericInput);
				return new DefaultInput(controller,"");
			}

//  y  ^Y  k  ^K  ^P  *  Backward one line   (or N lines).                                                             
			if (key=='y' || key=='k' || code=='y' || code=='k' || code == 'p' || code==38)
			{
				//controller.resetStatus();
				controller.oneLineBackward(numericInput);
				return new DefaultInput(controller,"");
			}

//  f  ^F  ^V  SPACE  *  Forward  one window (or N lines).                                                             
			if (key == ' ' || key == 'f' || code=='f' || code=='v')
			{
				//controller.resetStatus();
				controller.oneWindowForward(numericInput);
				return new DefaultInput(controller,"");
			}

//  b  ^B  ESC-v      *  Backward one window (or N lines).                                                             
			if (key == 'b' || code =='b') {
				//controller.resetStatus();
				controller.oneWindowBackward(numericInput);
				return new DefaultInput(controller,"");
			}

//  z                 *  Forward one window (and set window to N).                                                    
			if (key=='z')
			{
				// reset numericInput 
				// clear status Line
				//  controller.resetStatus();
				controller.setWindowHeight(numericInput); // check this for sanitizing
				controller.oneWindowForward(numericInput);
				return new DefaultInput(controller,"");
			}

//  w                 *  Backward one window (and set window to N).                                                    
			if (key=='w')
			{
				// reset numericInput 
				// clear status Line
				//  controller.resetStatus();
				controller.setWindowHeight(numericInput); // check this for sanitizing
				controller.oneWindowBackward("");
				return new DefaultInput(controller,"");
			}

//   d  ^D             *  Forward  one half-window (and set half-window to N).                                          
			if (key=='d' || code=='d')
			{
				controller.setWindowHalfHeight(numericInput); // half window?
				controller.halfWindowForward();
				return new DefaultInput(controller,"");
			}
//   u  ^U             *  Backward  one half-window (and set half-window to N).                                          

			if (key=='u' || code=='u')
			{
				controller.setWindowHalfHeight(numericInput); // half window?
				controller.halfWindowBackward();
				return new DefaultInput(controller,"");
			}

//  ESC-)  RightArrow *  Right one half screen width (or N positions).                                                 
			if (code==39)
			{
				controller.halfWindowRight(numericInput);
				return new DefaultInput(controller,"");
			}
//  ESC-(  LeftArrow  *  Left one half screen width (or N positions).
			if (code==37)
			{
				controller.halfWindowLeft(numericInput);
				return new DefaultInput(controller,"");
			}
// Unable to check for ctrl-cursor
//  ESC-}  ^RightArrow   Right to last column displayed.                                                                 
//  ESC-{  ^LeftArrow    Left  to first column.
//   F                    Forward forever; like "tail -f".                                                                

			if (key=='F')
			{
				// swith to tail mode
				//new DefaultInput(controller,"");

				// Waiting for data... (interrupt to abort)
				// ctrl-c returns to less control.
				// controller.foreverForward();
				;
			}
//  r  ^R  ^L            Repaint screen.                                                                                 
//  R                    Repaint screen, discarding buffered input. 
// RawUI cannot use screen buffer over PSRemoting:
// System.Management.Automation.Host.BufferCell                                               
	        if (key=='R' || key=='r' || code=='r' || code=='l')
			{
				controller.repaintScreen();
				return new DefaultInput(controller,"");
			}

// please sir I want some more less commands

			if (key == '/') 
			{
				LineInput readSearch = new LineInput(controller,"/");
				readSearch.SetConditionalPrefix(searchPrompts);

				readSearch.beginParse();

				string search = readSearch.getLine();

				// works Console.WriteLine("\n\n searching for: " + search + " \n\n");

				controller.SearchForward(numericInput,search);
				return new DefaultInput(controller,"");
			}

			if (key == '?') 
			{
				LineInput readSearch = new LineInput(controller,"?");
				readSearch.SetConditionalPrefix(searchPrompts);

				readSearch.beginParse();

				string search = readSearch.getLine();
				controller.SearchBackward(numericInput,search);
				return new DefaultInput(controller,"");
			}
// n                 *  Repeat previous search (for N-th occurrence).
			if (key == 'n')
			{
				controller.SearchForwardAgain(numericInput);
				return new DefaultInput(controller,"");
			}

			if (key == 'N')
			{
				controller.SearchBackwardAgain(numericInput);
				return new DefaultInput(controller,"");
			}
// g  <  ESC-<       *  Go to first line in file (or line N).
			if (key == 'g' || key == '<')
			{
				controller.moveToStart(numericInput);
				return new DefaultInput(controller,"");
			}
// G  >  ESC->       *  Go to last line in file (or line N).
			if (key == 'G' || key == '>')
			{
				controller.moveToEnd(numericInput);
				return new DefaultInput(controller,"");
			}
// p  %              *  Go to beginning of file (or N percent into file).
			if (key == 'p' || key == '%')
			{
				controller.movePercent(numericInput);
				return new DefaultInput(controller,"");
			}
// t                 *  Go to the (N-th) next tag.
			if (key == 't')
			{
				controller.gotoNextTag(numericInput);
				return new DefaultInput(controller,"");
			}
// T                 *  Go to the (N-th) previous tag.
			if (key == 'T')
			{
				controller.gotoPreviousTag(numericInput);
				return new DefaultInput(controller,"");
			}
// {  (  [           *  Find close bracket } ) ].
			if (key == '{' || key == '(' || key == '[')
			{
				controller.findClose(numericInput, key);
				return new DefaultInput(controller,"");
			}
// }  )  ]           *  Find open bracket { ( [.
			if (key == '}' || key == ')' || key == ']')
			{
				controller.findOpen(numericInput,key);
				return new DefaultInput(controller,"");
			}
// m<letter>            Mark the current top line with <letter>.
			if (key =='m')
			{
				// get key letter.
				char letter = '\0';
				KeyInfo mm = controller.ReadKey();
				letter = mm.Character;
				if (letter >= 'a' && letter <= 'z')
					controller.setMark (letter);
				return new DefaultInput(controller,"");
			}
// M<letter>            Mark the current bottom line with <letter>.
//   '<letter>            Go to a previously marked position.
//  ''                   Go to the previous position.
			if (key =='\'')
			{
				// get key letter.
				char letter = '\0';
				KeyInfo mm = controller.ReadKey();
				letter = mm.Character;
				if ((letter >= 'a' && letter <= 'z') || letter == '\'' || letter == '^' || letter == '$')
					controller.gotoMark (letter);  // special case for ' letter
				return new DefaultInput(controller,"");

			}

// controller.Status ( "" + key +"(" + code + ") / " +control);

			controller.Alert();

			string keyDecode = "" +(int)code + " ";
			keyDecode = keyDecode + "" + (int)key + " ";
			if (key >= 32 && key < 127)
				keyDecode = keyDecode + "" + key;

			controller.Status(keyDecode);
			return new DefaultInput(controller,"");

		}
	}

	public class ColonInput : LessInput 
	{
		private string numericInput;
		public ColonInput(LessController lc, string n) {
			controller = lc;
			numericInput = n;
			controller.Status(" :");
			//controller.updateStatus(status);
		}
		public override LessInput beginParse () 
		{ 
			KeyInfo ki = controller.ReadKey();

			if (ki.Character =='Q' || ki.Character == 'q')
				return null;

//  :e [file]            Examine a new file.
			if (ki.Character == 'e')
			{
				//read filename
				// controller.Status("Examine: ");  // needed???
				string filename = "";

// EXAMINE FILE NAME

				LineInput readFileName = new LineInput(controller,"Examine: ");

				readFileName.beginParse();

				filename = readFileName.getLine();
				//  Press (RETURN)
				// controller.examineFileNew(filename);
				return new DefaultInput(controller,"");
			}
//  :n                *  Examine the (N-th) next file from the command line.
			if (ki.Character == 'n')
			{
				// controller.resetStatus();
				controller.examineFileNext(numericInput);
				return new DefaultInput(controller,"");
			}
//  :p                *  Examine the (N-th) previous file from the command line.
			if (ki.Character == 'p')
			{
				//controller.resetStatus();
				controller.examineFilePrevious(numericInput);
				return new DefaultInput(controller,"");
			}
//  :x                *  Examine the first (or N-th) file from the command line.
			if (ki.Character == 'x')
			{
				//controller.resetStatus();
				controller.examineFile(numericInput);
				return new DefaultInput(controller,"");
			}
//  :d                   Delete the current file from the command line list.
			if (ki.Character == 'd')
			{
				//controller.resetStatus();
				controller.excludeCurrentFile();
				return new DefaultInput(controller,"");
			}
//  :d                   Delete the current file from the command line list.
			if (ki.Character == 'f')
			{
				controller.displayCurrentFileDetails();
				return new DefaultInput(controller,"");
			}
			controller.Alert();
			return new DefaultInput(controller,"");

		}
	
	}

	public class EscapeInput : LessInput {
		private string numericInput;
		public EscapeInput(LessController lc, string n) {
			controller = lc;
			numericInput = n;
			controller.Status(" ESC");
		}

		public override LessInput beginParse () { 
			KeyInfo ki = controller.ReadKey();

//  b  ^B  ESC-v      *  Backward one window (or N lines).
			if (ki.Character == 'v') {
				//controller.resetStatus();
				controller.oneWindowBackward(numericInput);
				return new DefaultInput(controller,"");
			}

//  ESC-SPACE         *  Forward  one window, but don't stop at end-of-file.
			if (ki.Character == ' ') {
				//controller.resetStatus();
				controller.oneWindowForward(numericInput); // and continue to next document
				return new DefaultInput(controller,"");
			}

//  ESC-n             *  Repeat previous search, spanning files.
			if (ki.Character == 'n') {
				//controller.resetStatus();
				controller.findNext("some state variable");
				return new DefaultInput(controller,"");
			}

//  ESC-)  RightArrow *  Right one half screen width (or N positions).
			if (ki.Character == ')')
			{
				controller.halfWindowRight(numericInput);
				return new DefaultInput(controller,"");
			}
//  ESC-(  LeftArrow  *  Left  one half screen width (or N positions).

			if (ki.Character == '(')
			{
				controller.halfWindowLeft(numericInput);
				return new DefaultInput(controller,"");
			}

//  ESC-}  ^RightArrow   Right to last column displayed.
			if (ki.Character == '}')
			{
				// current column =  so that longest string is visible
				;
			}
//  ESC-{  ^LeftArrow    Left  to first column.
			if (ki.Character == '{')
			{
				// current column = 0
				;
			}
//  ESC-F                Like F but stop when search pattern is found.
//  ESC-N             *  Repeat previous search, reverse dir. & spanning files.
//  ESC-u                Undo (toggle) search highlighting.
//  g  <  ESC-<       *  Go to first line in file (or line N).
			if (ki.Character == '<')
			{
				controller.moveToStart(numericInput);
				return new DefaultInput(controller,"");
			}
// G  >  ESC->       *  Go to last line in file (or line N).
			if (ki.Character == '>')
			{
				controller.moveToEnd(numericInput);
				return new DefaultInput(controller,"");
			}

//  ESC-^F <c1> <c2>  *  Find close bracket <c2>.
//  ESC-^B <c1> <c2>  *  Find open bracket <c1> 
//  ESC-M<letter>        Clear a mark.
			if (ki.Character == 'M') 
			{
				char letter = '\0';
				KeyInfo mm = controller.ReadKey();
				letter = mm.Character;
				if (letter >= 'a' && letter <= 'z')
					controller.clearMark(letter);

				return new DefaultInput(controller,"");
			}

			return new DefaultInput(controller,"");
		}
	}
// special case for entering command line input
	public class LineInput : LessInput 
	{
		private string lineInput;
		private string linePrefix;
		private int cursorPosition;
		private Dictionary<string, string> conditions;
		private string searchMode;
		private string linePrompt;

// might need to be a singleton, for history...
		public LineInput(LessController lc, string n) 
		{
			controller = lc;
			linePrompt = n;
			lineInput = "";
			searchMode = "";
			cursorPosition = 0;
			conditions = new Dictionary<string, string>();

			controller.Status(linePrefix);
		}

		public void SetConditionalPrefix (Dictionary<string, string> c)
		{
			conditions = c;
		}
		private void moveWordForward()
		{
			// next character after 1 space or more
			// it's a kind of magic
			int searchPosition = cursorPosition;
			// Console.WriteLine("b4 position: " + searchPosition + " length: "+lineInput.Length);

			if (searchPosition < lineInput.Length-1)
			{
				while ((lineInput.Substring(searchPosition,1) != " ") && searchPosition < lineInput.Length-1) { searchPosition++; }
				// Console.WriteLine("new position after letter: " + searchPosition + " length: "+lineInput.Length);

				cursorPosition = searchPosition;
			}
			if (searchPosition < lineInput.Length-1)
			{
				while ((lineInput.Substring(searchPosition,1) == " ") && searchPosition < lineInput.Length-1) { searchPosition++; }
				// Console.WriteLine("new position after space: " + searchPosition + " length: "+lineInput.Length);
				cursorPosition = searchPosition;
			}
		}

		private void moveWordBackward()
		{
			int searchPosition = cursorPosition;

			if (searchPosition > 0)
			{
				while ((lineInput.Substring(searchPosition,1) != " ") && searchPosition > 0)
				 { searchPosition--; }
			;  // it's a kind of magic
			}
		}

		private void backspace() { 
			//Console.WriteLine("line input length " + lineInput.Length + " cursor position: " + cursorPosition);
			lineInput = lineInput.Substring(0,cursorPosition-1); 
		}
		private void delete() { 
			if (cursorPosition < lineInput.Length-1)
				lineInput = lineInput.Substring(0,cursorPosition); 
		}
		// TODO:
		private void deleteWord() { ; }
		private void deleteWordLeft() { ; }

		public override LessInput beginParse () 
		{
			char key ;
			int code;
			char escape=(char)0;
			KeyInfo ki;


			bool escapeMode = false;
			//int cursorPosition = 0;
			controller.Status(linePrompt);
			ki = controller.ReadKey();  // blocks, so requires prompt
			key = ki.Character;
			code = ki.VirtualKeyCode;
			ControlKeyStates control = ki.ControlKeyState;  // this returns no data on mono

			if (key>=32 && key <127)
				code = 0;

			if (code == 27)
				escapeMode = true;

			while (key != 13)
			{

				if (lineInput.Length == 0 && conditions.Count>0)
				{
					string conditionalKey = "" + key;
					if (conditions.ContainsKey(conditionalKey))
					{
						string newPrefix = conditions[conditionalKey];
						if (newPrefix.Length > 0)
						{
							linePrefix = newPrefix + " " +linePrompt;
							controller.drawStatusCursor(linePrefix,lineInput,cursorPosition);
							searchMode = conditionalKey;
						}	
					} else {
						linePrefix = linePrompt;
					}
					// and tell the calling method of our changed state
				}
				//else
				//{
				//	Console.WriteLine("\n\n TOP key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput);

				//	Console.WriteLine("key: (" + (int)key + ") code: "+code+" escape: "+escape);
	//				if (cursorPosition < lineInput.Length)
	//					Console.WriteLine("Key: (" + (int)key + ") code: "+code+" control: "+control+ " escape: "+(int)escape + " char: "+lineInput.Substring(cursorPosition,1));
	//				else
	//					Console.WriteLine("Key: (" + (int)key + ") code: "+code+" control: "+control+ " escape: "+(int)escape);

	//controller.Status ( "" + key +"(" + code + ") / " +control);

	//  RightArrow ..................... ESC-l ... Move cursor right one character.                                           
					if (code == 39 || escape == 'l')
					{ 
						if (cursorPosition < lineInput.Length)
							cursorPosition++;
					}
	//Console.WriteLine("\n\n RIGHTARROW key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");

		
	// LeftArrow ...................... ESC-h ... Move cursor left one character.                                            
					if (code == 37 || escape == 'h')
					{
						if (cursorPosition > 0)
							cursorPosition--;
					}
	//Console.WriteLine("\n\n LEFTARROW key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");


	// ctrl-RightArrow  ESC-RightArrow  ESC-w ... Move cursor right one word.
					if (escape == 119 || escape == 'w') 
						moveWordForward();  // there is no spoon
							
	//Console.WriteLine("\n\n RIGHTWORD key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");
			
							// to be stateful or not to be generic that is the question

	// ctrl-LeftArrow   ESC-LeftArrow   ESC-b ... Move cursor left one word.                                                 
					if (escape == 98 || escape == 'b')
						moveWordBackward();
							// what she ^^^ said
	//Console.WriteLine("\n\n LEFTWORD key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");
							
	// HOME ........................... ESC-0 ... Move cursor to start of line.  
					if (escape == '0')
						cursorPosition = 0;
	//Console.WriteLine("\n\n HOME key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");	

	// END ............................ ESC-$ ... Move cursor to end of line.
					if (escape == '$')
						cursorPosition = lineInput.Length;

	//Console.WriteLine("\n\n END key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");
	// BACKSPACE ................................ Delete char to left of cursor.

	// Top right key on Mac is 127
	// RDP from linux to windows is 8 (assume windows is 8)
					if (key == 8) {
						if (cursorPosition == 0) 
							return null;
						backspace();
						cursorPosition --;
					}
	//	Console.WriteLine("\n\n BACKSPACE key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");
	// DELETE ......................... ESC-x ... Delete char under cursor.
					if (key == 127 || escape=='x')
					{
						if (cursorPosition == 0) return null;
						backspace();
						cursorPosition --;
					}
						//delete();
	// ctrl-BACKSPACE   ESC-BACKSPACE ........... Delete word to left of cursor.
					if (escape == 8)
						deleteWordLeft();
	// ctrl-DELETE .... ESC-DELETE .... ESC-X ... Delete word under cursor.
					if (escape=='X')
						deleteWord();

	// ctrl-U ......... ESC (MS-DOS only) ....... Delete entire line.
					if (escape==27 || code == 85) {
						Console.WriteLine("\nerase line\n");
						cursorPosition = 0;
						lineInput = "";
					}
	// UpArrow ........................ ESC-k ... Retrieve previous command line.
	// what... history?
					if (code==38 || escape=='k') { ; }
	// DownArrow ...................... ESC-j ... Retrieve next command line.
					if (code==40 || escape=='j') { ; }

	// TAB ...................................... Complete filename & cycle.
					if (code == 9) { ; }
	// SHIFT-TAB ...................... ESC-TAB   Complete filename & reverse cycle.
					if (escape == 9) { ; }
	// ctrl-L ................................... Complete filename, list all.
					if (code == 'l') { ; }
					// display status line and move cursor... 

					// Console.WriteLine("\n\nBEFORE  key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");

					// need to check where cursor is.
					if (key>=32 && key < 127) {
						if (cursorPosition < lineInput.Length) 
							lineInput = lineInput.Substring(0,cursorPosition) + key + lineInput.Substring(cursorPosition);
						else
							lineInput = lineInput + key;
						cursorPosition ++;
					}

					//Console.WriteLine("\n\n AFTER key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");
				//}

	// wondering if this the the correct behaviour for the erasing of the line
	// why yes it is
				if (lineInput.Length == 0)
					linePrefix = linePrompt;



				controller.drawStatusCursor(linePrefix,lineInput,cursorPosition);

				//Console.WriteLine("\n\n STATUS key: " + key + " pos: " + cursorPosition + " len: " + lineInput.Length + " line: "+ lineInput+ "\n\n");


				ki = controller.ReadKey();
				key = ki.Character;
				code = ki.VirtualKeyCode;

			//Console.WriteLine("straight outta Keyton: (" + (int)key + ") code: "+code+" control: "+control+ " escape: "+escape);

				escape=(char)0;
				if (key>=' ') 
					code = 0;

				if (escapeMode)
				{
					escapeMode = false;
					escape = (char)(key | code);
					key = '\0';
				}
				if (key == 27)
					escapeMode = true;
			}
			return null;
		}
		public string getLine() { return searchMode + lineInput; }
	}
}