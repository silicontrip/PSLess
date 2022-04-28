using System.Management.Automation;
using System.Management.Automation.Host;
using System;

namespace net.ninebroadcast {

	public abstract class LessInput
	{
		protected LessController controller;
		// protected PSHostUserInterface hostui;
		public virtual LessInput beginParse() { return null; }
	}

	public class ExitInput : LessInput {

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

	public class DefaultInput : LessInput {

		private string numericInput;

		public DefaultInput (LessController lc, string n) {
			controller = lc;
			//hostui = ui;
			numericInput = n;
			controller.Status(":");
		}

		public override LessInput beginParse () { 
			// state logic goes here;
			// thinking that this might come from the LessController (which inturn passes to Display)
			KeyInfo ki = controller.ReadKey();

			// we stay here until a non numeric key is pressed.
			// include delete and ^U
			// there is a whole section at the bottom of the help document
			// detailing the available line editing commands.

			while (ki.Character >='0' && ki.Character <='9') 
			{
				numericInput = numericInput + ki.Character;
				controller.Status(":"+numericInput);
				ki = controller.ReadKey();
			}

			char key = ki.Character;
			int code = ki.VirtualKeyCode;
			ControlKeyStates control = ki.ControlKeyState;  // this returns no data


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
				// less-help.txt lines 78-141/236 byte 7335/11925 61%  (press RETURN)
				controller.displayCurrentFileDetails();
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

//  z                 *  Forward  one window (and set window to N).                                                    
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
			}

// please sir I want some more less commands


controller.Status ( "" + key +"(" + code + ") / " +control);

			controller.Alert();
			return new DefaultInput(controller,"");

		}
	}
	public class ColonInput : LessInput {
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
				controller.Status("Examine: ");  // needed???
				string filename = "";

				LineInput readFileName = new LineInput(controller,"Examine: ");

				readFileName.beginParse();

				filename = readFileName.getLine();

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
//  ESC-(  LeftArrow  *  Left  one half screen width (or N positions).
//  ESC-}  ^RightArrow   Right to last column displayed.
//  ESC-{  ^LeftArrow    Left  to first column.
//  F                    Forward forever; like "tail -f".
//  ESC-F                Like F but stop when search pattern is found.
//  ESC-N             *  Repeat previous search, reverse dir. & spanning files.
//  ESC-u                Undo (toggle) search highlighting.
//  g  <  ESC-<       *  Go to first line in file (or line N).
//  G  >  ESC->       *  Go to last line in file (or line N).
//  ESC-^F <c1> <c2>  *  Find close bracket <c2>.
//  ESC-^B <c1> <c2>  *  Find open bracket <c1> 
//  ESC-M<letter>        Clear a mark.

			return new DefaultInput(controller,"");
		}
	}
// special case for entering command line input
	public class LineInput : LessInput {
		private string lineInput;
		private string linePrefix;
		public LineInput(LessController lc, string n) {
			controller = lc;
			linePrefix = n;
			lineInput = "";
			controller.Status(linePrefix);
		}

		private void moveWordForward()
		{
			;  // it's a kind of magic
		}

		private void moveWordBackward()
		{
			;  // it's a kind of magic
		}

		private void backspace() { ; }
		private void delete() { ; }
		private void deleteWord() { ; }

		public override LessInput beginParse () {
			KeyInfo ki;
			char key ;
			int code;
			int escape=0;

			ControlKeyStates control = ki.ControlKeyState;  // this returns no data

			bool escapeMode = false;
			int cursorPosition = 0;


			ki = controller.ReadKey();
			key = ki.Character;
			code = ki.VirtualKeyCode;

			if (key!='\0')
				code = 0;
			if (escapeMode)
			{
				escapeMode = false;
				escape = key | code;
				key = '\0';
			}
			if (code == 27)
				escapeMode = true;

			while (code != 13)
			{


// controller.Status ( "" + key +"(" + code + ") / " +control);


//  RightArrow ..................... ESC-l ... Move cursor right one character.                                           
				if (code == 39 || escape == 'l')
				{ 
					if (cursorPosition <= lineInput.Length)
						cursorPosition++;
				}

// LeftArrow ...................... ESC-h ... Move cursor left one character.                                            
				if (code == 37 || escape == 'h')
				{
					if (cursorPosition > 0)
						cursorPosition--;
				}
// ctrl-RightArrow  ESC-RightArrow  ESC-w ... Move cursor right one word.
				if (escape == 39 || escape == 'w') 
					moveWordForward();  // there is no spoon
				 
						// to be stateful or not to be generic that is the question

// ctrl-LeftArrow   ESC-LeftArrow   ESC-b ... Move cursor left one word.                                                 
				if (escape == 37 || escape == 'b')
					moveWordBackward();
						// what she ^^^ said
// HOME ........................... ESC-0 ... Move cursor to start of line.  
				if (escape == '0')
					cursorPosition = 0;
// END ............................ ESC-$ ... Move cursor to end of line.
				if (escape == '$')
					cursorPosition = lineInput.Length;
// BACKSPACE ................................ Delete char to left of cursor.
				if (code == 8)
					backspace();
// DELETE ......................... ESC-x ... Delete char under cursor.
				if (code == '~')
					delete();
// ctrl-BACKSPACE   ESC-BACKSPACE ........... Delete word to left of cursor.
				if (escape == 8)
					deleteWord();
// ctrl-DELETE .... ESC-DELETE .... ESC-X ... Delete word under cursor.
				if (escape=='X')
// ctrl-U ......... ESC (MS-DOS only) ....... Delete entire line.
				if (escape==27 || code =='u') {
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

				// need to check where cursor is.
				if (key>=32 && key <= 127) {
					if (cursorPosition < lineInput.Length) 
						lineInput = lineInput.Substring(0,cursorPosition) + key + lineInput.Substring(cursorPosition);
						else
							lineInput += key;
					cursorPosition ++;
				}
				controller.drawStatusCursor(linePrefix,lineInput,cursorPosition);

				ki = controller.ReadKey();
				key = ki.Character;
				code = ki.VirtualKeyCode;
				escape=0;
				if (key!='\0')
					code = 0;
				if (escapeMode)
				{
					escapeMode = false;
					escape = key | code;
					key = '\0';
				}
				if (code == 27)
					escapeMode = true;
			}
			return null;
		}
		public string getLine() { return lineInput; }
	}
}