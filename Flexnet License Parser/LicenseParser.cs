﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace LicenseParser
{
    public class LicenseParser
    {
        public static int newIndex = 0;
        private bool plist = false;
        // The heavy lifter of the parser -- parses our processed input text into LicenseChunks, which contain
        // the full details of each parsed license
        public ArrayList CleanedLic(ArrayList parsedFile)
        {
            // if the file's too short to even have the mandatory Flexnet reserved lines, skip parsing
            // FOR FUTURE REFERENCE: SHOULD DEFINE # OF RESERVED WORDS AS STATIC CONSTANT
            if (parsedFile.Count > 3)
            {
                ArrayList lics = InputParser.Licenses(); // list of valid licenses as found in our lookup XML
                ArrayList chunks = new ArrayList(); // our list of fully parsed and documented licenses; will be our returned variable

                // check if XML read worked
                if (lics.Count == 0)
                {
                    if (PrefsForm.LicenseMetaXMLPath.Equals(""))
                    {
                        throw new LicensesNotFoundException("Error: No path to the XML license lookup file has been specified.");
                    }
                    throw new LicensesNotFoundException("Error: could not find valid XML file at location " + PrefsForm.LicenseMetaXMLPath + " .");
                }

                // Check if junk text accidentally got typed at the start of the file; see if we can remove it and still
                // proceed with parsing or if the file is just invalid, in which case we hit our catch statement
                try
                {
                    while (!FlexNetWords.StartsOK(parsedFile[0].ToString(), parsedFile[1].ToString(), parsedFile[2].ToString()))
                    {
                        if (parsedFile.Count >= 1)
                        {
                            parsedFile.RemoveAt(0);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    MainMenu.DisplayFailure("This license file does not begin with the mandatory SERVER, USE_SERVER, and VENDOR lines, and is therefore invalid.");
                    return new ArrayList();
                }

                // loop through the lines in our processed input file
                for (int i = 0; i < parsedFile.Count; i++)
                {
                    String line = parsedFile[i].ToString(); // single line from our processed file
                    String trimmedLine = line.Trim(); // use this to check if a line starts with important words/chars without modifying it
                    if (FlexNetWords.StartsWithComment(trimmedLine))
                    {
                        // save comments if set in prefs
                        if (PrefsForm.KeepComments)
                        {
                            chunks.Add(line);
                        }
                    }
                    // ensures only one copy of certain keywords exists. Important area for future code softening.
                    else if (FlexNetWords.StartsWithReserved(trimmedLine))
                    {
                        chunks.Add(line);
                    }

                    else
                    {
                        // save line breaks if requested
                        if (PrefsForm.KeepBreaks)
                        {
                            if (String.IsNullOrWhiteSpace(line))
                            {
                                chunks.Add(line);
                            }
                        }
                        // if we hit one our of license keywords, parse the block for info
                        if (trimmedLine.StartsWith("PACKAGE") || trimmedLine.StartsWith("INCREMENT"))
                        {
                            String text = "";
                            String licName = "";
                            String featCode = "";
                            String licType = "";
                            String featureType = "";
                            String timeType = "";
                            String expirationDate = "";
                            String numSeats = "";
                            ArrayList comps = new ArrayList(); // license components

                            // figure out license type
                            if (trimmedLine.StartsWith("PACKAGE"))
                            {
                                licType = "PACKAGE";
                            }
                            else if (trimmedLine.StartsWith("INCREMENT PLIST"))
                            {
                                licType = "INCREMENT PLIST";
                            }
                            else if (trimmedLine.StartsWith("INCREMENT"))
                            {
                                licType = "INCREMENT";
                            }

                            // !!!! CONSTRUCTION ZONE !!!!

                            // grab the block of text for the license
                            text += Utilities.CheckEnd(i, parsedFile);

                            // The above line calls a method which currently determines what constitutes a license by running
                            // until an empty line is hit. However, this method should be changed to A: count the quotes,
                            // and perform other validation checks, as well as B: read until a Package+Increment or lone Increment
                            // is read, since some suites have a bunch of Package/Increment comments listed in sequence with no
                            // empty lines in between to hit.

                            // !!!! END CONSTRUCTION ZONE !!!!

                            // Find match for main featureCode first
                            foreach (License license in lics)
                            {
                                // we found a match!
                                if (line.Contains(license.FeatureCode))
                                {
                                    // the only time we wouldn't jump on a feature code match is if it's listed as a component, not
                                    // the license code itself
                                    if (!(line.Contains("COMPONENTS") && line.IndexOf(license.FeatureCode) >= line.IndexOf("COMPONENTS")))
                                    {
                                        licName = license.LicenseName;
                                        featCode = license.FeatureCode;
                                        lics.Remove(license);
                                        break;
                                    }
                                }
                            }
                            // should've found a match by now
                            if (licName.Equals(""))
                            {
                                licName = "Unknown License";
                            }
                            // handle plist
                            if (text.Contains("INCREMENT PLIST"))
                            {
                                if (!plist)
                                {
                                    plist = true;
                                }
                                else
                                {
                                    text = text.Remove(text.IndexOf("INCREMENT PLIST"));
                                }
                            }
                            // Grab some info from main declaration line
                            if (licType.Equals("PACKAGE"))
                            {
                                featureType = "Subscription Package";
                            }
                            else if (licType.Equals("INCREMENT"))
                            {
                                featureType = "Standalone";
                            }
                            else if (licType.Equals("INCREMENT PLIST"))
                            {
                                featureType = "Legacy - ignore";
                            }
                            if (text.Contains("permanent"))
                            {
                                timeType = "Perpetual";
                            }
                            else if (text.Contains("temporary") || text.Contains("extendable"))
                            {
                                timeType = "Term";
                            }
                            // find expiration date if the license isn't perpetual
                            if (!timeType.Equals("Perpetual"))
                            {
                                int dateSpot = text.IndexOf("INCREMENT");
                                if (text.Contains("1.000") && text.IndexOf("1.000", dateSpot) > dateSpot)
                                {
                                    dateSpot = text.IndexOf("1.000", dateSpot);
                                    for (int pos = 0; pos < "01-jan-2000".Length; pos++)
                                    {
                                        expirationDate += text[dateSpot + "1.000 ".Length + pos];
                                    }
                                }
                            }

                            // jump over the expiration date to grab to the number of seats
                            if (!expirationDate.Equals("") && text.Contains(expirationDate))
                            {
                                // verify that we have a number here
                                if (text[text.IndexOf(expirationDate) + expirationDate.Length + 1] >= '0' && 
                                    text[text.IndexOf(expirationDate) + expirationDate.Length + 1] <= '9')
                                {
                                    int numIndex = 1; // if the number is multiple digits long, gotta loop one by one
                                    while (text[text.IndexOf(expirationDate) + expirationDate.Length + numIndex] >= '0' &&
                                        text[text.IndexOf(expirationDate) + expirationDate.Length + numIndex] <= '9')
                                    {
                                        numSeats += text[text.IndexOf(expirationDate) + expirationDate.Length + numIndex];
                                        numIndex++;
                                    }
                                }
                            }
                            else if (text.Contains("permanent"))
                            {
                                int index = text.IndexOf("permanent") + "permanent".Length + 1; // index of # seats
                                while (text[index] >= '0' && text[index] <= '9')
                                {
                                    numSeats += text[index];
                                    index++;
                                }
                            }
                            // parse components section
                            if (text.Contains("COMPONENTS"))
                            {
                                String mysteryCode = ""; // will hold a given component feature code
                                bool added = false; // tracks whether or not we ID a given component
                                int startIndex = text.IndexOf("COMPONENTS"); // start from COMPONENTS

                                // indicates we have a single component
                                if (text[startIndex + "COMPONENTS".Length + 1] != '\"')
                                {
                                    // ensure it's a single component and not a missing quote
                                    if (line.Contains("COMPONENTS") && parsedFile[i + 1].ToString().Trim().StartsWith("OPTIONS"))
                                    {
                                        added = false;
                                        mysteryCode = "";

                                        // if we have 1 component just go until we hit a space
                                        while (!text[startIndex].ToString().Equals(" "))
                                        {
                                            // handle the EOL character and the tab when COMPONENTS list wraps around
                                            if (!text[startIndex].ToString().Trim().Equals("") && !text[startIndex].ToString().Equals("\\"))
                                            {
                                                mysteryCode += text[startIndex];
                                            }
                                            else
                                            {
                                                break;
                                            }
                                            startIndex++;
                                        }

                                        foreach (License lic in lics)
                                        {
                                            if (mysteryCode.Equals(lic.FeatureCode))
                                            {
                                                if (!comps.Contains(lic.LicenseName))
                                                {
                                                    comps.Add(lic.LicenseName);
                                                }
                                                // we mark the license as added as long as there's a match; if it wasn't added after
                                                // the match, it's a dupe and we don't need the extra print
                                                added = true;
                                                break;
                                            }
                                        }
                                        if (!added)
                                        {
                                            comps.Add("Unknown License");
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidLicenseException("Error: missing quotes around components section detected " +
                                            "at line " + i + ". Leading line was " + Environment.NewLine + line);
                                    }
                                }
                                else
                                {
                                    // go from the first quote to the second one (quotes indicate the block)
                                    startIndex = text.IndexOf("\"", startIndex) + 1;
                                    while (!text[startIndex].ToString().Equals("\""))
                                    {
                                        added = false;
                                        mysteryCode = "";
                                        // eat characters until we hit a space to get the feature code
                                        while (!text[startIndex].ToString().Equals(" ") && !text[startIndex].ToString().Equals("\""))
                                        {
                                            // handle the EOL character and the tab when COMPONENTS list wraps around
                                            if (!text[startIndex].ToString().Trim().Equals("") && !text[startIndex].ToString().Equals("\\"))
                                            {
                                                mysteryCode += text[startIndex];
                                            }
                                            startIndex++;
                                        }
                                        // make sure we didnt just run off the end of the license
                                        if (text[startIndex].ToString().Equals(" "))
                                        {
                                            // make sure there weren't just two spaces in between components
                                            if (mysteryCode.Trim().Length > 0)
                                            {
                                                // search our lookup file for matching name for the feature code we have
                                                foreach (License lic in lics)
                                                {
                                                    if (mysteryCode.Equals(lic.FeatureCode))
                                                    {
                                                        if (!comps.Contains(lic.LicenseName))
                                                        {
                                                            comps.Add(lic.LicenseName);
                                                        }
                                                        // we mark the license as added as long as there's a match; if it wasn't added after
                                                        // the match, it's a dupe and we don't need the extra print
                                                        added = true;
                                                        break;
                                                    }
                                                }
                                                // we didn't get a match
                                                if (!added)
                                                {
                                                    comps.Add("Unknown License");
                                                }
                                            }
                                            startIndex++;
                                        }
                                        // catch for when we hit the last component in the block
                                        else if (text[startIndex].ToString().Equals("\""))
                                        {
                                            foreach (License lic in lics)
                                            {
                                                if (mysteryCode.Equals(lic.FeatureCode))
                                                {
                                                    if (!comps.Contains(lic.LicenseName))
                                                    {
                                                        comps.Add(lic.LicenseName);
                                                    }
                                                    added = true;
                                                    break;
                                                }
                                            }
                                            if (!added)
                                            {
                                                comps.Add("Unknown License");
                                            }
                                        }
                                    }
                                }
                            }
                            // Remove duplicate components in the list
                            ArrayList removes = comps;
                            foreach (String comp in comps)
                            {
                                for (int loop = 0; loop < comps.Count; loop++)
                                {
                                    String otherComp = comps[loop].ToString();
                                    if (comp.ToString().Equals(otherComp.ToString()))
                                    {
                                        removes.Remove(comps.IndexOf(otherComp));
                                    }
                                }
                            }
                            LicenseChunk c = new LicenseChunk(text.Trim(), featCode, licName, featureType, timeType, expirationDate, numSeats, licType, removes);
                            chunks.Add(c);

                            // i automatically gets incremented at the next iteration of the loop;
                            // decrement by one here to ensure we end up on the correct line
                            i = newIndex - 1;
                        }
                    }

                }
                return chunks;
            }
            else
            {
                MainMenu.DisplayFailure("Error: The file is either blank or too short and cannot be cleaned.");
                return new ArrayList();
            }
        }
    }
}
