using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace net.ninebroadcast {
 
 // Get-ChildItem ...  Get-Item ... Get-Content  
 // I really do think that they went overboard with the verb-noun paradigm
    [Cmdlet(VerbsData.Out, "Less")]
    public class less : PSCmdlet
    {

		LessDocumentPipeline _document;

        public
        less()
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

		[Parameter(ValueFromPipeline = true)]
        public PSObject InputObject
		{
			get { return inputObject; }
			set { inputObject = value; }
		}
		private PSObject inputObject=null;

//https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.host.pshostrawuserinterface



			//PSHostUserInterface ui = Host.UI;
			// 
			
			//PSHostRawUserInterface rui = ui.RawUI;


// I don't think BufferCells work over PSRemoting
// going to need to test all the RawUI methods for availability

//			ui.WriteLine("I see a little rectangletto of a screen");

//			Rectangle rect = new Rectangle(0,0,10,10);

//			ui.WriteLine("Get Buffer contents can you do the fandango");


// is this implemented
//			BufferCell[,]buffrect = rui.GetBufferContents(rect);


//			Size bsize = rui.WindowSize;


//			ui.WriteLine("buffer cell and buffer rect very very crashful me");

			//ConsoleColor cc = 

//			BufferCell bc = new BufferCell('!',ConsoleColor.Black,ConsoleColor.Blue,0);

//			BufferCell[,]nrect = rui.NewBufferCellArray(20,1,bc);
			
//			Rectangle statusRect = new Rectangle (0,bsize.Height-1,bsize.Width-1,bsize.Height-1);

//			ui.WriteLine("bsize, bsize, bsize buffero");

//			ui.WriteLine("bing, there is stuff");

			//ui.WriteLine(ui.ToString());
			//ui.WriteLine(rui.ToString());

//			ui.WriteLine("width " + bsize.Width);
//			ui.WriteLine("height "+ bsize.Height);

//			ui.WriteLine("bkg "+rui.BackgroundColor);
//			ui.WriteLine("cursor pos "+rui.CursorPosition.ToString());
//			ui.WriteLine("cursor size "+rui.CursorSize.ToString());
//			ui.WriteLine("fgc "+rui.ForegroundColor);
//			ui.WriteLine("key? "+rui.KeyAvailable.ToString());

//			ui.WriteLine("title: "+rui.WindowTitle.ToString());

			
//			ui.WriteLine(""+bc.BackgroundColor);
//			ui.WriteLine(""+bc.ForegroundColor);
//			ui.WriteLine(""+bc.Character);
//			ui.WriteLine(""+bc.BufferCellType);

	// is this implemented
//		rui.SetBufferContents(statusRect,bc);

			// test for overwrite mode

//			ui.Write("this is a lonely line");
//			ui.Write("\r so lonely that you may not see it.\r\n");

// HELLO EVERYONE. now that I have your attention, The Text Document class is 1 indexed.
// but maybe it shouldn't be because the screen buffer is 0 indexed.

		// start peeling these off into a UI class
		// started

        protected override void BeginProcessing()
        {

			// TextDocument[] documentList = new TextDocument[];

			// needs a document controller
			// less can skip forward and back between Files

			// LessDocumentController docControl = new LessDocumentController();

//			LessDocument doc = new LessDocument();

			if (!String.IsNullOrEmpty(path))
			{
				_document = null;
				try {

					string current = Directory.GetCurrentDirectory();
					SessionState ss = new SessionState();
					Directory.SetCurrentDirectory(ss.Path.CurrentFileSystemLocation.Path);

					// multiple TextDocument instances for multiple paths
					// foreach (string thisPath in path)

					// document handling in the Controller class.
					// doc = new LessDocument(path);

					// docControl.AddDocument(doc);

					LessDisplay lcd = new LessDisplay(Host.UI);
					LessController lc = new LessController(lcd);  // TextDocument as array

					lc.AddDocument(path);
					//LessController lc = new LessController(docControl,lcd);  // TextDocument as array

					LessInput lkc = new DefaultInput(lc,"");

					lc.firstDocument();
					// looks like we need more UI methods than RawUI

					// lc.displayDocument();

					// Looking a bit empty in here now.
					// maybe space to add some more command line options

					while (lkc != null) {
						 lkc = lkc.beginParse(); 
					}
					Directory.SetCurrentDirectory(current);

				//	doc.Close();
				} catch (Exception e) {
					Host.UI.WriteLine(e.Message);
					Host.UI.WriteLine(e.StackTrace);
				}

			} else {
				// set up for PipeLine Input
				_document = new LessDocumentPipeline();
			}

		}

		protected override void ProcessRecord()
		{
			if (_document != null && inputObject != null)
			{
				_document.AddLine(inputObject.ToString());
			}
		}

		protected override void EndProcessing()
		{
			if (_document != null)
			{
				LessDisplay lcd = new LessDisplay(Host.UI);
				LessController lc = new LessController(lcd);
				lc.SetDocument(_document);
				LessInput lkc = new DefaultInput(lc,"");
				while (lkc != null) {
					lkc = lkc.beginParse(); 
				}
			}

		}

	}

    [Cmdlet(VerbsData.Out, "test")]
	public class outtest : PSCmdlet
    {

		//int sizex;
		//int sizey;
		ArrayList stringl;

        public
        outtest()
        {
            // empty, provided per design guidelines.
        }

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject
		{
			get { return inputObject; }
			set { inputObject = value; }
		}
		private PSObject inputObject=null;

        [Parameter(Position = 0)]

        public string Path
		{
			get { return p; }
			set { p = value; }
		}
		private string p="";

		protected override void BeginProcessing()
		{
			Host.UI.WriteLine("BeginProcessing");
			Host.UI.WriteLine("path: " + p);

			if (p.Length ==0)	
				stringl = new ArrayList();

/*
			if (inputObject != null)
			{
				Host.UI.WriteLine("begin: " + inputObject.ToString());
			}
*/
		}

		protected override void ProcessRecord()
		{
			if (p.Length == 0)
			{
				Host.UI.WriteLine("process record");
				string line = InputObject.ToString();
				stringl.Add(line);
			}
			// Host.UI.WriteLine(line);
		}

		protected override void EndProcessing()
		{
			if (p.Length ==0)
				if (stringl.Count >0)
					foreach (string li in stringl)
						Host.UI.WriteLine(li);
		}
	}

 }
