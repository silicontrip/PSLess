
using System.Management.Automation.Host;


namespace net.ninebroadcast {

	public abstract class LessInput
	{
		protected LessController controller;
		protected PSHostUserInterface hostui;

		// public abstract void init (TextDocument td,LessDisplay ld);
		public virtual LessInput beginParse () { return null; }
		//public void init() { ; }
	}

	public class ExitInput : LessInput {

		public ExitInput (LessController lc, PSHostUserInterface ui, string n) {
			controller = lc;
			hostui = ui;
			controller.Status(" Z");		
		}

		public LessInput beginParse () { 
			// state logic goes here;
			KeyInfo ki = hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp);
			if (ki.Character == 'Z')
				return null;

			controller.Alert();
			return new DefaultInput(controller,hostui,"");
		}
	}

	public class DefaultInput : LessInput {

		private string numericInput;

		public DefaultInput (LessController lc, PSHostUserInterface ui, string n) {
			controller = lc;
			hostui = ui;
			numericInput = n;
			controller.Status(":");
		}

		public LessInput beginParse () { 
			// state logic goes here;
			// thinking that this might come from the LessController (which inturn passes to Display)
			KeyInfo ki = hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp);

			// we stay here until a non numeric key is pressed.
			// include delete and ^U
			// there is a whole section at the bottom of the help document
			// detailing the available line editing commands.

			while (ki.Character >='0' && ki.Character <='9') 
			{
				numericInput = numericInput + ki.Character;
				controller.Status(":"+numericInput);
				ki = hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp);
			}
			
			char key = ki.Character;
			int code = ki.VirtualKeyCode;
			// change state option

			// check for control key and modify code appropriately
			// if (ki.modifierFlags)


			if (ki.Character == ':') {
				return new ColonInput(controller,hostui,numericInput);
			}
			if (ki.Character == 'q' || ki.Character == 'Q') 
				return null;

			if (ki.Character == 'Z')
				return new ExitInput(controller,hostui,numericInput); // not sure if we should make the constructors the same.

			if (ki.VirtualKeyCode==27)
				return new EscapeInput(controller,hostui,numericInput);

			if (ki.Character=='=')  // or ^G :f
			{
				// less-help.txt lines 78-141/236 byte 7335/11925 61%  (press RETURN)
				controller.displayCurrentFileDetails();
				return new DefaultInput(controller,hostui,"");
			}	

// these commands take an optional numeric argument.
// their behaviour changes when a number is involved.

			if (ki.Character=='e' || 
			    ki.Character=='j' || 
				ki.VirtualKeyCode==13)
			{
				//controller.resetStatus();
				controller.oneLineForward(numericInput);
				return new DefaultInput(controller,hostui,"");
			}

			if (ki.Character=='y' ||
			 ki.Character=='k') 
			{
				//controller.resetStatus();
				controller.oneLineBackward(numericInput);
				return new DefaultInput(controller,hostui,"");
			}

			if (ki.Character == ' ' ||
				ki.Character == 'f' )
			{
				//controller.resetStatus();
				controller.oneWindowForward(numericInput);
				return new DefaultInput(controller,hostui,"");
			}

			if (ki.Character == 'b') {
				//controller.resetStatus();
				controller.oneWindowBackward(numericInput);
				return new DefaultInput(controller,hostui,"");
			}

			if (ki.Character=='z')
			{
				// reset numericInput 
				// clear status Line
				//  controller.resetStatus();
				controller.setWindowHeight(numericInput);
				controller.oneWindowForward(numericInput);
				return new DefaultInput(controller,hostui,"");
			}

			controller.Alert();
			return this;

		}
	}
	public class ColonInput : LessInput {
		private string numericInput;
		public ColonInput(LessController lc, PSHostUserInterface ui, string n) {
			controller = lc;
			hostui = ui;
			numericInput = n;
			controller.Status(" :");
			//controller.updateStatus(status);
		}
		public LessInput beginParse () { 
			KeyInfo ki = hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp);

			if (ki.Character =='Q' || ki.Character == 'q')
				return null;

			if (ki.Character == 'e')
			{
				//read filename
				string filename = "";
				while (ki.VirtualKeyCode != 13)
				{
					// is it a valid character
					filename = filename + ki.Character;
				}
				// controller.examineFileNew(filename);
				return new DefaultInput(controller,hostui,"");
			}
			if (ki.Character == 'n')
			{
				// controller.resetStatus();
				controller.examineFileNext(numericInput);
				return new DefaultInput(controller,hostui,"");
			}
			if (ki.Character == 'p')
			{
				//controller.resetStatus();
				controller.examineFilePrevious(numericInput);
				return new DefaultInput(controller,hostui,"");

			}
			if (ki.Character == 'x')
			{
				//controller.resetStatus();
				controller.examineFile(numericInput);
				return new DefaultInput(controller,hostui,"");
			}
			if (ki.Character == 'd')
			{
				//controller.resetStatus();
				controller.excludeCurrentFile();
				return new DefaultInput(controller,hostui,"");
			}
			controller.Alert();
			return new DefaultInput(controller,hostui,"");

		}
	
	}
	public class EscapeInput : LessInput {
		private string numericInput;
		public EscapeInput(LessController lc, PSHostUserInterface ui, string n) {
			controller = lc;
			hostui = ui;
			numericInput = n;
			controller.Status(" ESC");
		}

		public LessInput beginParse () { 
			KeyInfo ki = hostui.RawUI.ReadKey(ReadKeyOptions.NoEcho | ReadKeyOptions.IncludeKeyUp);

			if (ki.Character == 'n') {
				//controller.resetStatus();
				controller.findNext("some state variable");
				return new DefaultInput(controller,hostui,"");
			}
			return new DefaultInput(controller,hostui,"");
		}
	}
}