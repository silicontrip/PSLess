using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
 
 // Get-ChildItem ...  Get-Item ... Get-Content  
 // I really do think that they went overboard with the verb-noun paradigm
    [Cmdlet(VerbsData.Out, "Less")]
    public class conui : PSCmdlet
    {


        public
        conui()
        {
            // empty, provided per design guidelines.
        }

        protected override void BeginProcessing()
        {

			PSHostUserInterface ui = Host.UI;
			PSHostRawUserInterface rui = ui.RawUI;

			Console.WriteLine(ui);
			Console.WriteLine(rui);

			Console.WriteLine(rui.BackgroundColor);
			Console.WriteLine(rui.CursorPosition);
			Console.WriteLine(rui.ForegroundColor);
			Console.WriteLine(rui.KeyAvailable);
			Console.WriteLine(rui.WindowSize);

			KeyInfo ki = rui.ReadKey();

			Console.WriteLine(ki);

		}
	}
 }
