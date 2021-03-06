﻿using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LicenseParser
{
    // This class handles the background C# code which goes hand-in-hand with the user preferences form. Its main tasks are writing
    // to the CURRENT_USER registry where the preferences are stored and proccessing the user's various changes on the page.
    public partial class PrefsForm : Form
    {
        // comment content variables
        private static bool addComments = true; // whether user wants documentation or not
        private static bool showFeatureTypes = false;
        private static bool showLicenseTypes = false;
        private static bool showNumberOfSeats = false;
        private static bool listSubComponents = false;

        // comment formatting variables
        private static char commentChar = '#';
        private static int leadingCommentSpace = 1; // # spaces from a comment character to comment content
        private static bool indentedComments = false;
        private static int indentSpaces = 4; // # spaces preceeding each comment character

        // header formatting variables
        private static char headerChar = '=';
        private static bool commentHeaders = true;
        private static bool fixedHeaderLength = false;
        private static bool variableHeaderLength = true;
        private static int fixedCommentLength = 10; // length of fixed headers (if enabled)

        // misc. variables
        private static bool keepComments = false;
        private static bool keepBreaks = false; // whether we keep line breaks or not
        private static string licenseMetaXMLPath = ""; // explicit path to the XML lookup file

        // Accessors for all of the above variables.
        // Since other classes need to know the user prefs for parsing but can't access the values of the checkboxes/etc.
        // directly from the preferences form, we need these private variables with accessors to make the prefs available.
        public static int FixedCommentLength { get => fixedCommentLength; }
        public static bool VariableHeaderLength { get => variableHeaderLength; }
        public static bool FixedHeaderLength { get => fixedHeaderLength; }
        public static bool CommentHeaders { get => commentHeaders; }
        public static bool KeepBreaks { get => keepBreaks; }
        public static bool KeepComments { get => keepComments; }
        public static string LicenseMetaXMLPath { get => licenseMetaXMLPath; }
        public static int IndentSpaces { get => indentSpaces; }
        public static bool ListSubComponents { get => listSubComponents; }
        public static bool ShowNumberOfSeats { get => showNumberOfSeats; }
        public static bool ShowLicenseTypes { get => showLicenseTypes; }
        public static bool ShowFeatureTypes { get => showFeatureTypes; }
        public static bool IndentedComments { get => indentedComments; }
        public static char CommentChar { get => commentChar; }
        public static char HeaderChar { get => headerChar; }
        public static int LeadingCommentSpace { get => leadingCommentSpace; }
        public static bool AddComments { get => addComments; }

        // Autogenerated constructor required for Designer support in Visual Studio
        public PrefsForm()
        {
            InitializeComponent();
        }

        // Recurses down to our intended save directory, assuming the paramter reg is the HKEY_CURRENT_USER registry key.
        // Our intended save directory is HKEY_CURRENT_USER/SOFTWARE/MDi/Flexnet License Parser/1.0/Prefs
        // As long as it finds the SOFTWARE registry key, this method will create any missing subkeys.
        private static RegistryKey registryRecurse(RegistryKey reg)
        {
            RegistryKey temp = null; // our returned key
            foreach (String s in reg.GetSubKeyNames())
            {
                // create registry tree
                if (s.Equals("Software") || s.Equals("SOFTWARE"))
                {
                    temp = reg.OpenSubKey(s, true);
                    temp = temp.CreateSubKey("MDi");
                    temp = temp.CreateSubKey("Flexnet License Parser");
                    temp = temp.CreateSubKey("1.0");
                    temp = temp.CreateSubKey("Prefs");
                    break;
                }
            }
            return temp;
        }

        // Restores all form objects, registry values, and class variables to their default values.
        // Called when there is a problem / missing value found when attempting to access the registry.
        public void CreateDefault()
        {
            // get our save location
            RegistryKey rkHKCU = Registry.CurrentUser;
            RegistryKey rkRun = registryRecurse(rkHKCU);

            // set class variable values
            showFeatureTypes = false;
            showLicenseTypes = false;
            showNumberOfSeats = false;
            listSubComponents = false;

            commentChar = '#';
            headerChar = '=';
            leadingCommentSpace = 1;
            indentSpaces = 4;

            licenseMetaXMLPath = "";

            indentedComments = false;
            keepComments = false;
            keepBreaks = false;
            addComments = true;
            commentHeaders = true;
            fixedHeaderLength = false;
            variableHeaderLength = true;
            fixedCommentLength = 10;

            // set registry values
            
            rkRun.SetValue("KeepComments", keepComments);
            rkRun.SetValue("KeepBreaks", keepBreaks);
            rkRun.SetValue("AddComments", addComments);
            rkRun.SetValue("FeatureTypes", showFeatureTypes);
            rkRun.SetValue("LicenseTypes", showLicenseTypes);
            rkRun.SetValue("NumOfSeats", showNumberOfSeats);
            rkRun.SetValue("SubComponents", listSubComponents);
            rkRun.SetValue("Headers", commentHeaders);
            rkRun.SetValue("FixedLength", fixedHeaderLength);
            rkRun.SetValue("VariableLength", variableHeaderLength);
            rkRun.SetValue("FixedNumber", fixedCommentLength);
            rkRun.SetValue("FixedNumberEnabled", fixedHeaderLength);
            rkRun.SetValue("HeaderChar", headerChar);
            rkRun.SetValue("CommentChar", commentChar);
            rkRun.SetValue("SpaceAfterCommentChar", leadingCommentSpace);
            rkRun.SetValue("IndentedComments", indentedComments);
            rkRun.SetValue("SpacesInIndent", indentSpaces);
            rkRun.SetValue("SpacesInIndentEnabled", indentedComments);
            rkRun.SetValue("LicenseMetaXMLPath", licenseMetaXMLPath);

            // set form object values
            featureTypes.Checked = false;
            licenseTypes.Checked = false;
            numberSeats.Checked = false;
            subComponents.Checked = false;
            commentCharBox.Text = "#";
            headerCharBox.Text = "=";
            leadingCommentSpaceVal.Value = 1;
            numberOfSpacesPerIndent.Value = 4;
            numberOfSpacesPerIndent.Enabled = false;
            indentedCommentsBox.Checked = false;
            comments2.Checked = false;
            linebreaks2.Checked = false;
            addCommentsBox.Checked = true;
            headers.Checked = true;
            fixedLength.Checked = false;
            variableLength.Checked = true;
            fixedNumber.Visible = false;
            fixedNumber.Value = 10;

            XMLPathBox.Text = "";

            rkHKCU.Close();
            rkRun.Close();
        }

        // Loads the values from the registry into our class variables. Loads default values if a value is missing.
        // Called when the program initially starts (i.e. the MainMenu is opened).
        public static void StartUp()
        {
            try
            {
                // get registry save location
                RegistryKey rkHKCU = Registry.CurrentUser;
                RegistryKey rkRun = registryRecurse(rkHKCU);

                // update class variables, but we don't need to edit form objects since the form isn't opening yet

                keepComments = Convert.ToBoolean(rkRun.GetValue("KeepComments").ToString());
                keepBreaks = Convert.ToBoolean(rkRun.GetValue("KeepBreaks").ToString());
                addComments = Convert.ToBoolean(rkRun.GetValue("AddComments").ToString());
                showFeatureTypes = Convert.ToBoolean(rkRun.GetValue("FeatureTypes").ToString());
                showLicenseTypes = Convert.ToBoolean(rkRun.GetValue("LicenseTypes").ToString());
                showNumberOfSeats = Convert.ToBoolean(rkRun.GetValue("NumOfSeats").ToString());
                listSubComponents = Convert.ToBoolean(rkRun.GetValue("SubComponents").ToString());
                commentHeaders = Convert.ToBoolean(rkRun.GetValue("Headers").ToString());
                fixedHeaderLength = Convert.ToBoolean(rkRun.GetValue("FixedLength").ToString());
                variableHeaderLength = Convert.ToBoolean(rkRun.GetValue("VariableLength").ToString());
                fixedCommentLength = Convert.ToInt32(rkRun.GetValue("FixedNumber").ToString());
                headerChar = (char)rkRun.GetValue("HeaderChar").ToString()[0];
                commentChar = (char)rkRun.GetValue("CommentChar").ToString()[0];
                leadingCommentSpace = Convert.ToInt32(rkRun.GetValue("SpaceAfterCommentChar").ToString());
                indentedComments = Convert.ToBoolean(rkRun.GetValue("IndentedComments").ToString());
                indentSpaces = Convert.ToInt32(rkRun.GetValue("SpacesInIndent").ToString());

                licenseMetaXMLPath = rkRun.GetValue("LicenseMetaXMLPath").ToString();
            }
            // set everything to defaults if there was a problem locating registry values
            catch (NullReferenceException e)
            {
                showFeatureTypes = false;
                showLicenseTypes = false;
                showNumberOfSeats = false;
                listSubComponents = false;

                commentChar = '#';
                headerChar = '=';
                leadingCommentSpace = 1;
                indentSpaces = 4;

                indentedComments = false;
                keepComments = false;
                keepBreaks = false;
                addComments = true;
                commentHeaders = true;
                fixedHeaderLength = false;
                variableHeaderLength = true;
                fixedCommentLength = 10;

                licenseMetaXMLPath = "";
            }
        }

        // This method is called whenever the user selects Preferences from the MainMenu. We update the form objects when
        // we open the form, but we don't need to change any class variables unless the user saves.
        public void Open()
        {
            try
            {
                // get registry save location
                RegistryKey rkHKCU = Registry.CurrentUser;
                RegistryKey rkRun = registryRecurse(rkHKCU);

                // update form objects
                comments2.Checked = Convert.ToBoolean(rkRun.GetValue("KeepComments").ToString());
                linebreaks2.Checked = Convert.ToBoolean(rkRun.GetValue("KeepBreaks").ToString());
                addCommentsBox.Checked = Convert.ToBoolean(rkRun.GetValue("AddComments").ToString());
                featureTypes.Checked = Convert.ToBoolean(rkRun.GetValue("FeatureTypes").ToString());
                licenseTypes.Checked = Convert.ToBoolean(rkRun.GetValue("LicenseTypes").ToString());
                numberSeats.Checked = Convert.ToBoolean(rkRun.GetValue("NumOfSeats").ToString());
                subComponents.Checked = Convert.ToBoolean(rkRun.GetValue("SubComponents").ToString());
                headers.Checked = Convert.ToBoolean(rkRun.GetValue("Headers").ToString());
                fixedLength.Checked = Convert.ToBoolean(rkRun.GetValue("FixedLength").ToString());
                variableLength.Checked = Convert.ToBoolean(rkRun.GetValue("VariableLength").ToString());
                fixedNumber.Value = Convert.ToInt32(rkRun.GetValue("FixedNumber").ToString());
                fixedNumber.Visible = Convert.ToBoolean(rkRun.GetValue("FixedNumberEnabled").ToString());
                headerCharBox.Text = rkRun.GetValue("HeaderChar").ToString();
                commentCharBox.Text = rkRun.GetValue("CommentChar").ToString();
                leadingCommentSpaceVal.Value = Convert.ToInt32(rkRun.GetValue("SpaceAfterCommentChar").ToString());
                indentedCommentsBox.Checked = Convert.ToBoolean(rkRun.GetValue("IndentedComments").ToString());
                numberOfSpacesPerIndent.Value = Convert.ToInt32(rkRun.GetValue("SpacesInIndent").ToString());
                if (addCommentsBox.Checked)
                {
                    numberOfSpacesPerIndent.Enabled = Convert.ToBoolean(rkRun.GetValue("SpacesInIndentEnabled").ToString());
                }
                else
                {
                    numberOfSpacesPerIndent.Enabled = false;
                }
                leadingCommentSpaceVal.Enabled = addCommentsBox.Checked;
                commentContent.Enabled = addCommentsBox.Checked;
                headerOptions.Enabled = addCommentsBox.Checked;
                indentedCommentsBox.Enabled = addCommentsBox.Checked;
                headerLengthBox.Enabled = headers.Checked;

                XMLPathBox.Text = rkRun.GetValue("LicenseMetaXMLPath").ToString();

                rkRun.Close();
                rkHKCU.Close();
            }
            catch (NullReferenceException e)
            {
                MessageBox.Show("One or more of the stored values for user preferences was found to be missing from the registry. Preferences have been reset to their default values.");
                this.CreateDefault();
            }
        }

        // When we save, update the registry and our class variables (since the user has now confirmed changes).
        public void Save()
        {
            // get registry save path
            RegistryKey rkHKCU = Registry.CurrentUser;
            RegistryKey rkRun = registryRecurse(rkHKCU);

            // save form values to registry
            rkRun.SetValue("KeepComments", comments2.Checked);
            rkRun.SetValue("KeepBreaks", linebreaks2.Checked);
            rkRun.SetValue("AddComments", addCommentsBox.Checked);
            rkRun.SetValue("FeatureTypes", featureTypes.Checked);
            rkRun.SetValue("LicenseTypes", licenseTypes.Checked);
            rkRun.SetValue("NumOfSeats", numberSeats.Checked);
            rkRun.SetValue("SubComponents", subComponents.Checked);
            rkRun.SetValue("Headers", headers.Checked);
            rkRun.SetValue("FixedLength", fixedLength.Checked);
            rkRun.SetValue("VariableLength", variableLength.Checked);
            rkRun.SetValue("FixedNumber", fixedNumber.Value);
            rkRun.SetValue("FixedNumberEnabled", fixedNumber.Visible);
            rkRun.SetValue("HeaderChar", headerCharBox.Text);
            rkRun.SetValue("CommentChar", commentCharBox.Text);
            rkRun.SetValue("SpaceAfterCommentChar", leadingCommentSpaceVal.Value);
            rkRun.SetValue("IndentedComments", indentedCommentsBox.Checked);
            rkRun.SetValue("SpacesInIndent", numberOfSpacesPerIndent.Value);
            rkRun.SetValue("SpacesInIndentEnabled", numberOfSpacesPerIndent.Enabled);

            rkRun.SetValue("LicenseMetaXMLPath", XMLPathBox.Text);

            rkHKCU.Close();
            rkRun.Close();

            // update class variables
            showFeatureTypes = featureTypes.Checked;
            keepComments = comments2.Checked;
            keepBreaks = linebreaks2.Checked;
            addComments = addCommentsBox.Checked;
            showLicenseTypes = licenseTypes.Checked;
            showNumberOfSeats = numberSeats.Checked;
            listSubComponents = subComponents.Checked;
            commentHeaders = headers.Checked;
            fixedHeaderLength = fixedLength.Checked;
            variableHeaderLength = variableLength.Checked;
            fixedCommentLength = (int)fixedNumber.Value;
            headerChar = (char)headerCharBox.Text[0];
            commentChar = (char)commentCharBox.Text[0];
            indentSpaces = (int)numberOfSpacesPerIndent.Value;
            indentedComments = indentedCommentsBox.Checked;
            leadingCommentSpace = (int)leadingCommentSpaceVal.Value;

            licenseMetaXMLPath = XMLPathBox.Text;

            MessageBox.Show("Preferences saved.");
        }

        // Needed to save XML rename from parse click failure without re-saving everything
        public static void SetXML(String newPath)
        {
            licenseMetaXMLPath = newPath;
            RegistryKey rkHKCU = Registry.CurrentUser;
            RegistryKey rkRun = registryRecurse(rkHKCU);
            rkRun.SetValue("LicenseMetaXMLPath", LicenseMetaXMLPath);
        }

        // Enable/disable comment customization options depending on whether the user wants comments or not
        private void addComments_Click(object sender, EventArgs e)
        {
            if (addCommentsBox.Checked)
            {
                headerOptions.Enabled = true;
                commentContent.Enabled = true;
                if (indentedCommentsBox.Checked)
                {
                    numberOfSpacesPerIndent.Enabled = true;
                }
                indentedCommentsBox.Enabled = true;
                leadingCommentSpaceVal.Enabled = true;
            }
            else
            {
                headerOptions.Enabled = false;
                commentContent.Enabled = false;
                numberOfSpacesPerIndent.Enabled = false;
                indentedCommentsBox.Enabled = false;
                leadingCommentSpaceVal.Enabled = false;
            }
            
        }

        // Allows the user to set the length of fixed length headers when that option is selected
        private void fixedLength_Click(object sender, EventArgs e)
        {
            if (fixedLength.Checked)
            {
                fixedNumber.Visible = true;
            }            
        }

        // When user switches back to variable length headers, we don't need to see the fixed length # anymore
        private void variableLength_Click(object sender, EventArgs e)
        {
            if (variableLength.Checked)
            {
                fixedNumber.Visible = false;
            }            
        }

        // Enable/disable header length box depending on whether user wants headers
        private void headers_Click(object sender, EventArgs e)
        {
            if (headers.Checked)
            {
                headerLengthBox.Enabled = true;
            }
            else
            {
                headerLengthBox.Enabled = false;
            }            
        }

        // Enable/disable the box for # of spaces before comments depending on user prefs
        private void indentedComments_Click(object sender, EventArgs e)
        {
            if (indentedCommentsBox.Checked)
            {
                numberOfSpacesPerIndent.Enabled = true;
            }
            else
            {
                numberOfSpacesPerIndent.Enabled = false;
            }            
        }

        // If the user clicks reset, change all the values of the form objects back to their defaults, 
        // but don't save anything to class variables or registries until the user hits Save.
        private void reset_Click(object sender, EventArgs e)
        {
              featureTypes.Checked = false;
              licenseTypes.Checked = false;
              numberSeats.Checked = false;
              subComponents.Checked = false;
              commentCharBox.Text = "#";
              headerCharBox.Text = "=";
              leadingCommentSpaceVal.Value = 1;
              numberOfSpacesPerIndent.Value = 4;
              numberOfSpacesPerIndent.Enabled = false;
              indentedCommentsBox.Checked = false;
              comments2.Checked = false;
              linebreaks2.Checked = false;
              addCommentsBox.Checked = true;
              headers.Checked = true;
              fixedLength.Checked = false;
              variableLength.Checked = true;
              fixedNumber.Visible = false;
              fixedNumber.Value = 10;              
        }

        // Called when the user clicks "Save" on the form
        private void savePrefs_Click(object sender, EventArgs e)
        {
            this.Save();
            this.Close();
        }

        // If the user cancels out, just exit without saving anything
        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Called when the user selects an XML lookup file on the form. Allows the user to select the file with the
        // standard Windows open file window. Doesn't save the new path, as that only should occur if the user presses "Save."
        private void selectXML_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML Files | *.xml";
            openDialog.ShowDialog();
            if (!string.IsNullOrEmpty(openDialog.FileName))
            {
                XMLPathBox.Text = openDialog.FileName;
            }
        }
    }
}
