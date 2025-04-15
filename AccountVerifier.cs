using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.DirectoryServices;

namespace AccountVerifier
{
    class AVMain
    {
        static DirectorySearcher funcCreateDSSearcher()
        {
            // [Comment] Get local domain context
            string rootDSE;

            System.DirectoryServices.DirectorySearcher objrootDSESearcher = new System.DirectoryServices.DirectorySearcher();
            rootDSE = objrootDSESearcher.SearchRoot.Path;
            // [DebugLine]Console.WriteLine(rootDSE);

            // [Comment] Construct DirectorySearcher object using rootDSE string
            System.DirectoryServices.DirectoryEntry objrootDSEentry = new System.DirectoryServices.DirectoryEntry(rootDSE);
            System.DirectoryServices.DirectorySearcher objDSSearcher = new System.DirectoryServices.DirectorySearcher(objrootDSEentry);
            // [DebugLine]Console.WriteLine(objDSSearcher.SearchRoot.Path);
            return objDSSearcher;
        }

        static bool funcLicenseCheck()
        {
            string strLicenseString = "";
            bool bValidLicense = false;

            try
            {
                TextReader tr = new StreamReader("sotfwlic.dat");

                try
                {
                    strLicenseString = tr.ReadLine();

                    if (strLicenseString.Length > 0 & strLicenseString.Length < 29)
                    {
                        // [DebugLine] Console.WriteLine("if: " + strLicenseString);
                        Console.WriteLine("Invalid license");

                        tr.Close(); // close license file

                        return bValidLicense;
                    }
                    else
                    {
                        tr.Close(); // close license file
                        // [DebugLine] Console.WriteLine("else: " + strLicenseString);

                        string strMonthTemp = ""; // to convert the month into the proper number
                        string strDate;

                        //Month
                        strMonthTemp = strLicenseString.Substring(7, 1);
                        if (strMonthTemp == "A")
                        {
                            strMonthTemp = "10";
                        }
                        if (strMonthTemp == "B")
                        {
                            strMonthTemp = "11";
                        }
                        if (strMonthTemp == "C")
                        {
                            strMonthTemp = "12";
                        }
                        strDate = strMonthTemp;

                        //Day
                        strDate = strDate + "/" + strLicenseString.Substring(16, 1);
                        strDate = strDate + strLicenseString.Substring(6, 1);

                        // Year
                        strDate = strDate + "/" + strLicenseString.Substring(24, 1);
                        strDate = strDate + strLicenseString.Substring(4, 1);
                        strDate = strDate + strLicenseString.Substring(1, 2);

                        // [DebugLine] Console.WriteLine(strDate);
                        // [DebugLine] Console.WriteLine(DateTime.Today.ToString());
                        DateTime dtLicenseDate = DateTime.Parse(strDate);
                        // [DebugLine]Console.WriteLine(dtLicenseDate.ToString());

                        if (dtLicenseDate >= DateTime.Today)
                        {
                            bValidLicense = true;
                        }
                        else
                        {
                            Console.WriteLine("License expired.");
                        }

                        return bValidLicense;
                    }

                } //end of try block on tr.ReadLine

                catch
                {
                    // [DebugLine] Console.WriteLine("catch on tr.Readline");
                    Console.WriteLine("Invalid license");
                    tr.Close();
                    return bValidLicense;

                } //end of catch block on tr.ReadLine

            } // end of try block on new StreamReader("sotfwlic.dat")

            catch (System.Exception ex)
            {
                // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());

                // [DebugLine] System.Console.WriteLine(ex.Message);

                if (ex.Message.StartsWith("Could not find file"))
                {
                    Console.WriteLine("License file not found.");
                }

                return bValidLicense;

            } // end of catch block on new StreamReader("sotfwlic.dat")

        } // LicenseCheck

        static void funcLogToEventLog(string strAppName, string strEventMsg, int intEventType)
        {
            string sLog;

            sLog = "Application";

            if (!EventLog.SourceExists(strAppName))
                EventLog.CreateEventSource(strAppName, sLog);

            //EventLog.WriteEntry(strAppName, strEventMsg);
            EventLog.WriteEntry(strAppName, strEventMsg, EventLogEntryType.Information, intEventType);
        } // LogToEventLog

        static void funcPrintParameterSyntax()
        {
            Console.WriteLine("AccountVerifier v1.0 (c) 2011 SystemsAdminPro.com");
            Console.WriteLine();
            Console.WriteLine("Parameter syntax:");
            Console.WriteLine();
            Console.WriteLine("Use the following for the first parameter:");
            Console.WriteLine("[filename]          specify a filename to use as the input file");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("AccountVerifier testinputfile.txt");
        }

        static void funcReadInput(string strInputFile)
        {
            using (TextReader tr = new StreamReader(strInputFile))
            {
                string line1;
                string[] strItems = new string[2];

                char[] charSplit = {','};

                string strQueryFilterPrefix = "(&(objectCategory=person)(objectClass=user)(name=";
                string strQueryFilterSuffix = "))";

                string objAccountDEvalues;
                string objAccountNameValue;
                int intStrPosFirst = 3;
                int intStrPosLast;

                System.DirectoryServices.DirectorySearcher objAccountObjectSearcher = funcCreateDSSearcher();
                // [DebugLine]Console.WriteLine(objAccountObjectSearcher.SearchRoot.Path);

                while ((line1 = tr.ReadLine()) != null)
                {
                    strItems = line1.Split(charSplit , StringSplitOptions.None);
                    if (strItems[1] == "Terminated")
                    {
                        Console.WriteLine("{0} should not have an active account.", strItems[0]);

                        // [Comment] Add filter to DirectorySearcher object
                        objAccountObjectSearcher.Filter = (strQueryFilterPrefix + strItems[0] + strQueryFilterSuffix);

                        // [Comment] Execute query, return results, display name and path values
                        System.DirectoryServices.SearchResultCollection objAccountResults = objAccountObjectSearcher.FindAll();

                        foreach (System.DirectoryServices.SearchResult objAccount in objAccountResults)
                        {
                            System.DirectoryServices.DirectoryEntry objAccountDE = new System.DirectoryServices.DirectoryEntry(objAccount.Path);
                            intStrPosLast = objAccountDE.Name.Length;
                            objAccountNameValue = objAccountDE.Name.Substring(intStrPosFirst, intStrPosLast - intStrPosFirst);

                            objAccountDEvalues = objAccountNameValue + "\t" + objAccountDE.Path;
                            Console.WriteLine("AD account exists for: {0}", objAccountDEvalues);
                        }
                    }
                }
            }

        }

        static void Main(string[] args)
        {
            if (funcLicenseCheck())
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Parameters must be specified to run AccountVerifier.");
                    Console.WriteLine("Run AccountVerifier -? to get the parameter syntax.");
                }
                else
                {
                    if (args[0] == "-?")
                    {
                        funcPrintParameterSyntax();
                    }
                    else
                    {
                        funcReadInput(args[0]);
                    }
                }
            }

        }
    }
}
