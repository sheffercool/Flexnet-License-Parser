﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Collections;

namespace LicenseParser
{
    // This is the main form which the user interacts with. This class contains the event handlers for all
    // the various buttons and actions the user can perform on the form.
    public partial class MainMenu : Form
    {
        private String licenseFileName = ""; // stores name of file which was opened for default save name
        private string cleanFile = ""; // stores parsed text in block form
        private ArrayList manipulatedFile = new ArrayList(); // stores processed text from InputParser to pass to LicenseParser
        private ArrayList keepThese = new ArrayList(); // stores list of lines of parsed text
        private LicenseParser parser = new LicenseParser(); // used to actually execute parsing

        public MainMenu()
        {
            InitializeComponent();
            MinimizeBox = true;
            MaximizeBox = true;
            PrefsForm.StartUp();
        }

        private void displayStatus(string message)
        {
            statusLabel.Text = message;
        }

        //creates a default Windows open window and writes the data from the selected file into the input text box when the
        //open button is pressed. The name of the file is stored in licenseFileName, but does not expose the location of the file.
        private void open_Click_1(object sender, EventArgs e)
        {                             
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "License Files | *.lic|Text Files | *.txt";
            openDialog.ShowDialog();

            if (!string.IsNullOrEmpty(openDialog.FileName))
            {
                using (StreamReader reader = new StreamReader(openDialog.FileName))
                {
                    rawFile.Text = reader.ReadToEnd();
                    licenseFileName = openDialog.SafeFileName;
                    reader.Close();
                }
                displayStatus("File opened.");
            }
        }

        private void save_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();               //creates a default Windows save window and populates some default conditions when Save button is clicked
            saveDialog.DefaultExt = "lic";
            saveDialog.AddExtension = true;
            saveDialog.FileName = "Parsed_" + licenseFileName;
            saveDialog.InitialDirectory = Environment.SpecialFolder.UserProfile + "\\Documents\\";
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = "License Files | *.lic";

            saveDialog.ShowDialog();
            using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
            {
                writer.WriteLine(cleanedFile.Text);
            }
            displayStatus("Cleaned file saved.");
        }

        private void parse_Click(object sender, EventArgs e)
        {
            manipulatedFile = InputParser.Parse(rawFile.Text);
            FlexNetWords.ResetReservedWords();
            cleanFile = "";
            cleanedFile.Text = "";
            try
            {
                keepThese = parser.CleanedLic(manipulatedFile);
            }
            catch (LicensesNotFoundException lnfe)
            {
                var result = MessageBox.Show(lnfe.Message + " Would you like to set the file path for the XML lookup file now?" +
                    " The path can be set at any time via Edit > Preferences, but you will not be able to parse any files" +
                    " without having a valid XML file path set. If you would like more details, please see the help link " +
                    "under Help > Help Documentation.", 
                    "File Could Not Be Cleaned", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.Filter = "XML Files | *.xml";
                    openDialog.ShowDialog();
                    if (!string.IsNullOrEmpty(openDialog.FileName))
                    {
                        PrefsForm.SetXML(openDialog.FileName);
                        displayStatus("XML selected.");
                    }
                }
                return;
            }
            catch(InvalidLicenseException ile)
            {
                var result = MessageBox.Show(ile.Message, "Invalid License File", MessageBoxButtons.OK);
            }
            if (keepThese.Count >= 1)
            {
                int start = 0;
                int pos = 0;
                while (start * 1000 <= keepThese.Count)                                     //checks to make sure the loop will not be starting past the end of the file,
                {                                                                           //then uses a for loop to break the ArrayList into chunks of a thousand lines each and places the chunks in a holder string. 
                    for (pos = (start * 1000); pos < ((start + 1) * 1000); pos++)           //(The last chunk will not be 1000 lines, it will be the remainder of lines left after the other chunks.)
                    {                                                                       //Adds a newLine character after each line as the for loop executes. After each chunk is created,
                        if (pos < keepThese.Count)                                          //it is added to the existing text in the output TextBox and the holder string is reset.
                        {
                            cleanFile += keepThese[pos] + Environment.NewLine;
                        }
                    }
                    cleanedFile.Text += cleanFile;
                    cleanFile = "";
                    start++;
                }
                displayStatus("File cleaned.");
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)       //this is the "Open" item from the "File" menu
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "License Files | *.lic|Text Files | *.txt";
            openDialog.ShowDialog();
            if (!string.IsNullOrEmpty(openDialog.FileName))
            {
                using (StreamReader reader = new StreamReader(openDialog.FileName))
                {
                    rawFile.Text = reader.ReadToEnd();
                    licenseFileName = openDialog.SafeFileName;
                }
                displayStatus("File opened.");
            }
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "lic";
            saveDialog.AddExtension = true;
            saveDialog.FileName = "Parsed_" + licenseFileName;
            saveDialog.InitialDirectory = Environment.SpecialFolder.UserProfile + @"\Documents\";
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = "License Files | *.lic";
            saveDialog.ShowDialog();
            using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
            {
                writer.WriteLine(cleanedFile.Text);
            }
            displayStatus("Cleaned file saved.");
        }

        private void exit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cut_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;

            if (ctrl != null)
            {
                if (ctrl is TextBox)
                {
                    TextBox tb = (TextBox)ctrl;
                    tb.Cut();
                }
            }
        }

        public static void displayFailure(String message)
        {
            MessageBox.Show(message);
        }

        private void copy_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;

            if (ctrl != null)
            {
                if (ctrl is TextBox)
                {
                    TextBox tb = (TextBox)ctrl;
                    tb.Copy();
                }
            }
        }

        private void paste_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;

            if (ctrl != null)
            {
                if (ctrl is TextBox)
                {
                    TextBox tb = (TextBox)ctrl;
                    tb.Paste();
                }
            }
        }

        private void undo_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;

            if (ctrl != null)
            {
                if (ctrl is TextBox)
                {
                    TextBox tb = (TextBox)ctrl;
                    tb.Undo();
                    tb.ClearUndo();
                }
            }
        }

        private void about_Click(object sender, EventArgs e)        //todo
        {
            String display = "Created 8/5/13";
            MessageBox.Show(display);
        }

        private void selectAll_Click(object sender, EventArgs e)
        {
            Control ctrl = this.ActiveControl;

            if (ctrl != null)
            {
                if (ctrl is TextBox)
                {
                    TextBox tb = (TextBox)ctrl;
                    tb.SelectAll();
                }
            }
        }

        private void Form1_FormClosing(object sender, CancelEventArgs ar)
        {
            var mBox = MessageBox.Show("Are you sure you want to quit?", "Confirm", MessageBoxButtons.YesNo);
            if (mBox == DialogResult.No)
            {
                ar.Cancel = true;
            }
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            cleanedFile.SelectAll();
            cleanedFile.Copy();
            displayStatus("Cleaned license copied to clipboard.");
        }

        private void preferences_Click_1(object sender, EventArgs e)
        {
            PrefsForm preferencesForm = new PrefsForm();
            RegistryKey rkHKCU = Registry.CurrentUser.OpenSubKey("Software");
            if (!rkHKCU.GetSubKeyNames().Contains("MDi"))
            {
                preferencesForm.CreateDefault();
            }
            else
            {
                preferencesForm.Open();
            }
            rkHKCU.Close();
            preferencesForm.ShowDialog();
        }
    }
}