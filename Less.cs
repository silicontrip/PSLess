using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
 
 // Get-ChildItem ...  Get-Item ... Get-Content  
 // I really do think that they went overboard with the verb-noun paradigm
    [Cmdlet(VerbsData.Out, "Less")]
    public class conui : PSCmdlet
    {

		int sizex;
		int sizey;

        public
        conui()
        {
            // empty, provided per design guidelines.
        }

        [Alias("FullName")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path="";

//https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.host.pshostrawuserinterface

        protected override void BeginProcessing()
        {

			PSHostUserInterface ui = Host.UI;
			PSHostRawUserInterface rui = ui.RawUI;

			// ui.WriteLine(ui);
			// ui.WriteLine(rui);
			// ui.WriteLine(rui.BackgroundColor);
			//ui.WriteLine(rui.CursorPosition.ToString());
			//ui.WriteLine(rui.ForegroundColor);
			//ui.WriteLine(rui.KeyAvailable.ToString());

// HELLO EVERYONE. now that I have your attention, The Text Document class is 1 indexed.
// but maybe it shouldn't be because the screen buffer is 0 indexed.

		// start peeling these off into a UI class
		// started

			TextDocument doc = new TextDocument(path);
			LessController lc = new LessController(doc,Host.UI.RawUI);

			// Looking a bit empty in here now.
			// maybe space to add some more command line options

			//bool continuing = true;
			while (lc.command) { ; }

			doc.Close();

		}
	}
 }
