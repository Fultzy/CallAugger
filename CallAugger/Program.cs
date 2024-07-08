using CallAugger.DataImporters;
using CallAugger.Parsers;
using CallAugger.Settings;
using CallAugger.Utilities;
using CallAugger.Utilities.CliInterface;
using CallAugger.Utilities.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CallAugger
{
    ///////////////////////////////////////////////////////////////
    //         Project: CallAugger

    // This is a program that will take in 2 sets of data.
    // One set of data is a list of phone calls. The other set
    // of data is a list Pharmacies. The program will then
    // group the phone calls by phone number and if they
    // belong to a pharmacy on file it will add that calls
    // data to the pharmacy's collection. The program will then
    // output a formatted excel file detailing this data.

    // Imported files will be CSV and or Excel format.

    // Being able to add phone numbers to a pharmacy should be
    // a feature of the program, Removal as well. 

    ///////////////////////////////////////////////////////////////
    // added notes: (intended pain points)
    // - internal weekend calls are only counted as internal. 
    // - If a fax number calls it will be counted towards the pharmacy
    // - if the call is internal the transfer user is not created

    //////// MISSING VALUES IN DB FROM OLD DB ////////
    // - v1.1 db to v1.3~ changes:
    // - added State to CallRecord table
    // - added IsFax to PhoneNumber table
    // - added IsPrimary to PhoneNumber table
    // - added State to Pharmacy table
    // - added FaxNumber to Pharmacy table

    ///////////////////////////////////////////////////////////////
    // TODO:
    // - Finish && Polish up the Report worksheets
    // - 

    class Program
    {
        static void Main()
        {
            //////////////////////// Main Setup ////////////////////////
            // Get path
            var path = Directory.GetCurrentDirectory();

            // Load in settings
            new AppSettings();

            // Check Log Files
            Logger.CheckAllLogFiles(path);

            // Set date range
            DateRange dateRange = new DateRange();

            // Begin DataBase Singleton
            Console.WriteLine("Starting db Connection...");
            SQLiteHandler dbHandle = SQLiteHandler.Instance;


            //////////////////////// Load Data ////////////////////////
            var IDataImporter = new DataImporter() { dbHandle = dbHandle };
            IDataImporter.ImportPharmacyData();

            // if the db doesnt have any Pharmacies
            if (!dbHandle.CheckReady()) return;

            List<CallRecord> newCallRecords = IDataImporter.ImportCallData();


            //////////////////// Process New Data /////////////////////
            // Parce any new Data if any. (CallRecords need to be first)
            var IParser = new Parse() { dbHandle = dbHandle };
            if (newCallRecords != null)
            {
                IParser.ParsePharmacyData();
            }


            /////////////////////// BEGIN MAIN PROGRAM LOOP ///////////////////////////
            // create a  CLI main menu
            var MainMenu = new CliMenu
            {
                OptionPadding = 3,
                MenuName = "Main Menu:",
                Help = "About",
                Exit = "Close Program",
                Options = {
                    "Assign Unassigned PhoneNumbers",
                    "Remove PhoneNumbers from Pharmacy\n",
                    "Search Pharmacies\n",
                    "Change Date Range",
                    "Generate Reports\n",
                    "Debugging Menu\n"
                }
            };


            bool GameOver = false;
            while (GameOver == false)
            {
                // Write the menu and get the user input
                MainMenu.WriteMainProgramTitle();
                var MainMenuInput = MainMenu.WriteMenu();
                int SelectedOption = MainMenu.CaseOptionNumber(MainMenuInput);

                if (MainMenuInput.ToLower() == "exit") break;
                if (MainMenuInput.ToLower() == "help") HelpMenus.MainMenu();

                switch (SelectedOption)
                {
                    case 1: // View Unassigned Phone Numbers

                        MenuMethods.AddPhoneNumberMenu(dbHandle);

                        break;
                    case 2: // Remove Phone Number from Pharmacy

                        MenuMethods.RemovePhoneNumberMenu(dbHandle);

                        break;
                    case 3: // Search Pharmacies

                        Pharmacy selectedPharmacy = MenuMethods.PharmacySearchSelectionLoop(dbHandle);
                        if (selectedPharmacy == null) break;

                        MenuMethods.PharmacyEditMenu(dbHandle, selectedPharmacy);

                        break;
                    case 4: // Change Date Range

                        MenuMethods.ChangeDateRangeMenu(dateRange);

                        break;
                    case 5: // Generate Reports

                        MenuMethods.GenerateReportsMenu(dbHandle, path);

                        break;
                    case 6: // Debugging Menu

                        MenuMethods.DebuggingMenu(dbHandle);

                        break;
                }
            }
            /////////////////////// END MAIN LOOP ///////////////////////////
        }
    }
}