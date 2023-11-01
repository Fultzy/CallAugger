using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CallAugger.Utilities;
using CallAugger.Controllers.DataImporters;
using CallAugger.Controllers.Parsers;
using CallAugger.Controllers.Generators;
using CallAugger.Utilities.DataBase;
using System.Data.SQLite;
using System.Configuration;
using CallAugger.Settings;
using System.Net.Http.Headers;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using CallAugger.Utilities.Validators;
using System.Security.Cryptography;
using Microsoft.Office.Interop.Excel;
using System.Security.Policy;
using System.Diagnostics;

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
    // added notes:
    // - internal weekend calls are only counted as internal. 

    
    class Program
    {
        static void Main(string[] args)
        {

            //////////////////////// Main Setup ////////////////////////
            // Get path
            string path = Directory.GetCurrentDirectory();

            // Load in settings
            AppSettings ISettings = new AppSettings();

            // Begin DataBase
            SQLiteHandler dbHandle = new SQLiteHandler();

            // Set date range
            DateRange dateRange = new DateRange();


            //////////////////////// Load Data ////////////////////////
            // If these lists are null then no files were found
            var IDataDoer = new DataImporter();
            List<CallRecord> newCallRecords = IDataDoer.ImportCallData(path);
            List<Pharmacy> newPharmacies    = IDataDoer.ImportPharmacyData(path);


            //////////////////// Process New Data /////////////////////
            // Parce any new Data if any. (CallRecords need to be first)
            var IParser = new Parse();
            if (newCallRecords != null) IParser.ParseCallRecordData(newCallRecords);
            if (newPharmacies  != null) IParser.ParsePharmacyData();


            // if the db is empty inform the user
            if (dbHandle.GetAllPharmacies().Count() == 0 || dbHandle.GetAllCallRecords().Count() == 0)
            {
                new CliMenu().WriteProgramTitle();
                Console.WriteLine("\nEMPTY DATABASE, CHECK THE README FOR IMPORTING INSTRUCTIONS\n");
                Console.Write("Press the AnyKey to Continue...");
                Console.ReadKey();
            }

            /////////////////////// BEGIN MAIN PROGRAM LOOP ///////////////////////////
            bool GameOver = false;
            while (GameOver == false)
            {
                // create a  CLI main menu
                var MainMenu = new CliMenu
                {
                    OptionPadding = 3,
                    MenuName  = "Main Menu:",
                    Help      = "About",
                    Exit      = "Close Program",
                    Options   = {
                        "Assign Unassigned PhoneNumbers",
                        "Remove PhoneNumbers from Pharmacy\n",
                        "Search Pharmacies\n",
                        "Change Date Range",
                        "Generate Reports\n",
                        "Debugging Menu\n"

                    }
                };

                // Begin Write
                MainMenu.WriteProgramTitle();


                // Write the menu and get the user input
                var MainMenuInput = MainMenu.WriteMenu();
                int SelectedOption = MainMenu.CaseOptionNumber(MainMenuInput);

                if (MainMenuInput.ToLower() == "exit") break;

                if (MainMenuInput.ToLower() == "help" || MainMenuInput.ToLower() == "about")
                {
                    MainMenu.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Main Help Menu ~~~~\n");
                    Console.WriteLine("    Select an option by typing the corresponding number and pressing enter.\n");
                    Console.WriteLine("!!!Learn more by viewing the help menues within each option for more details!!!");
                    Console.WriteLine("\nInitial comment:");
                    Console.WriteLine(@"      /////////////////////////////////////////////////////////////// ");
                    Console.WriteLine(@"      //         Project: CallAugger");
                    Console.WriteLine("");
                    Console.WriteLine(@"      // This is a program that will take in 2 sets of data.");
                    Console.WriteLine(@"      // One set of data is a list of phone calls. The other set");
                    Console.WriteLine(@"      // of data is a list Pharmacies. The program will then");
                    Console.WriteLine(@"      // group the phone calls by phone number and if they");
                    Console.WriteLine(@"      // belong to a pharmacy on file it will add that calls");
                    Console.WriteLine(@"      // data to the pharmacy's collection. The program will then");
                    Console.WriteLine(@"      // output a formatted excel file detailing this data.");
                    Console.WriteLine("");
                    Console.WriteLine(@"      // Imported files will be CSV and or Excel format.");
                    Console.WriteLine("");
                    Console.WriteLine(@"      // Being able to add phone numbers to a pharmacy should be");
                    Console.WriteLine(@"      // a feature of the program, Removal as well.");
                    Console.WriteLine("");
                    Console.WriteLine(@"      ///////////////////////////////////////////////////////////////");

                    Console.WriteLine("\n  Added Features:");
                    Console.WriteLine("  - a selectable date rage to focus in on periods in time but also allows for larger time frames");
                    Console.WriteLine("  - an Awesome, custom, CLI interface");

                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }

                switch (SelectedOption)
                {
                    case 1: // View Unassigned Phone Numbers

                        AddPhoneNumberMenu(dbHandle);

                        break;
                    case 2: // Remove Phone Number from Pharmacy

                        RemovePhoneNumberMenu(dbHandle);

                        break;
                    case 3: // Search Pharmacies
                        bool done = false;
                        while (done == false)
                        {
                            Pharmacy selectedPharmacy = PharmacySearchSelectionLoop(dbHandle.GetAllPharmacies());
                            if (selectedPharmacy == null) break;

                            Console.Clear();
                            MainMenu.WriteProgramTitle();
                            selectedPharmacy.WritePharmacyInfo();
                            Console.Write("Press the AnyKey to Continue...");
                            Console.ReadKey();
                            done = true;
                        }

                        break;
                    case 4: // Change Date Range

                        if (ChangeDateRangeMenu(dateRange))
                        {
                            MainMenu.WriteProgramTitle();
                            Console.WriteLine("\n\nDate Range Changed!");
                            Console.Write("Press the AnyKey to Continue...");
                            Console.ReadKey();
                        }


                        break;
                    case 5: // Generate Reports

                        GenerateReportsMenu(dbHandle, path);

                        break;        
                    case 6: // Debugging Menu

                        DebuggingMenu(dbHandle);

                        break;
                }
            }
            /////////////////////// END MAIN LOOP ///////////////////////////
        }




        ////////////////////////  MENU METHODS  ////////////////////////
        private static bool AddPhoneNumberMenu(SQLiteHandler dbHandle)
        {
            // these two loops allow the user to find and select a pharmacy and a phone number
            PhoneNumber selectedPhoneNumberToAdd = UnassignedPhoneNumberSelectionLoop(dbHandle.GetUnassignedPhoneNumbers());
            if (selectedPhoneNumberToAdd == null) return false;
            Pharmacy selectedPharmacyToAddTo = PharmacySearchSelectionLoop(dbHandle.GetAllPharmacies(), selectedPhoneNumberToAdd.FormatedPhoneNumber());
            if (selectedPharmacyToAddTo == null) return false;


            // Make sure the user is sure
            bool confirmAdd = new CliMenu().ConfirmationPrompt(
                $"Are You Sure {selectedPhoneNumberToAdd.FormatedPhoneNumber()} Should Be Added To {selectedPharmacyToAddTo.Name.Trim()}? (y/n)");


            // add the new phone number to the selected pharmacy
            if (confirmAdd != true) return false;
            selectedPharmacyToAddTo.AddPhoneNumber(selectedPhoneNumberToAdd);
            dbHandle.UpdatePhoneNumberPharmacyID(selectedPhoneNumberToAdd, selectedPharmacyToAddTo.id);


            // present confirmation screen
            new CliMenu().WriteProgramTitle();
            selectedPharmacyToAddTo.WritePharmacyInfo();
            Console.WriteLine("\n Added {0} to {1}", selectedPhoneNumberToAdd.FormatedPhoneNumber(), selectedPharmacyToAddTo.Name.Trim());
            Console.Write("\nPress the AnyKey to Continue...");
            Console.ReadKey();

            return true;
        }

        private static bool RemovePhoneNumberMenu(SQLiteHandler dbHandle)
        {

            // these two loops allow the user to find and select a pharmacy and a phone number
            Pharmacy selectedPharmacyToRemoveFrom = PharmacySearchSelectionLoop(dbHandle.GetAllPharmacies());
            if (selectedPharmacyToRemoveFrom == null) return false;
            PhoneNumber selectedPhoneNumberToRemove = PhoneNumberRemovalLoop(selectedPharmacyToRemoveFrom);
            if (selectedPhoneNumberToRemove == null) return false;


            // Prevent the user from removing the primary phone number
            if (selectedPharmacyToRemoveFrom.PrimaryPhoneNumber == selectedPhoneNumberToRemove.Number)
            {
                Console.WriteLine("\n Cannot Remove Primary PhoneNumber");
                Console.Write("\nPress the AnyKey to Continue...");
                Console.ReadKey();
                
                return false;
            }


            // remove the selected phone number from the selected pharmacy
            if (selectedPharmacyToRemoveFrom.PhoneNumbers.Count() > 0)
            {
                // Make sure the user is sure
                bool confirmRemove = new CliMenu().ConfirmationPrompt(
                    $"Are You Sure {selectedPhoneNumberToRemove.FormatedPhoneNumber()} Should Be Removed From {selectedPharmacyToRemoveFrom.Name.Trim()}? (y/n)");
                if (confirmRemove != true) return false;

                dbHandle.UpdatePhoneNumberPharmacyID(selectedPhoneNumberToRemove, 0);
                selectedPharmacyToRemoveFrom.PhoneNumbers.Remove(selectedPhoneNumberToRemove);
            }

            // present confirmation screen
            new CliMenu().WriteProgramTitle();
            selectedPharmacyToRemoveFrom.WritePharmacyInfo();
            Console.WriteLine("\n Removed {0} from {1}",
                selectedPhoneNumberToRemove.FormatedPhoneNumber(),
                selectedPharmacyToRemoveFrom.Name.Trim());
            Console.Write("\nPress the AnyKey to Continue...");
            Console.ReadKey();

            return true;
        }

        private static bool ChangeDateRangeMenu(DateRange dateRange)
        {
            var DateRangeMenu = new CliMenu
            {
                OptionPadding = 3,
                MenuName = $"Change Date Range:\n",
                Prompt = "(mm/dd/yyyy - mm/dd/yyyy)\nSelect a Date Range or Type one in: ",
                Help = "Learn More",
                Exit = "Back to Main Menu",
                Options ={
                    "Last Week",
                    "Last Month",
                    "Last Year\n",
                    "This Week",
                    "This Month",
                    "This Year\n",
                    "All Time\n"
                    }
            };


            bool changedDateRange = false;
            while (changedDateRange == false)
            {
                DateRangeMenu.WriteProgramTitle();
                
                string dateRangeInput = DateRangeMenu.WriteMenu();
                if (dateRangeInput.ToLower() == "exit") return false;

                if (dateRangeInput.ToLower() == "help")
                {
                    DateRangeMenu.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Change Date Range Help Menu ~~~~\n");
                    Console.WriteLine("    This menu allows you to change the date range for the reports generated in the program.");
                    Console.WriteLine("    Select one of the predefined date ranges by typing the corresponding number and pressing enter.");
                    Console.WriteLine("    You can also type a custom date range in the format (mm/dd/yyyy - mm/dd/yyyy).");
                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }
                
                if (dateRangeInput.Length == 1)
                {
                    int optionSelection = DateRangeMenu.CaseOptionNumber(dateRangeInput);
                    if (optionSelection == 0) continue;

                    switch (optionSelection)
                    {
                        case 1: // Last Week
                            dateRange.SetToLastWeek();
                            changedDateRange = true;
                            break;
                        case 2: // Last Month
                            dateRange.SetToLastMonth();
                            changedDateRange = true;
                            break;
                        case 3: // Last Year
                            dateRange.SetToLastYear();
                            changedDateRange = true;
                            break;
                        case 4: // This Week
                            dateRange.SetToThisWeek();
                            changedDateRange = true;
                            break;
                        case 5: // This Month
                            dateRange.SetToThisMonth();
                            changedDateRange = true;
                            break;
                        case 6: // This Year
                            dateRange.SetToThisYear();
                            changedDateRange = true;
                            break;
                        case 7: // All Time
                            dateRange.SetToAllTime();
                            changedDateRange = true;
                            break;

                    }
                }

                if (DateValidator.IsValidDateRangeString(dateRangeInput))
                {
                    try
                    {
                        dateRange.SetFromString(dateRangeInput);
                        changedDateRange = true;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid Date Range");
                        Console.Write("Press the AnyKey to try Again...");
                        Console.ReadLine();
                    }
                }

                if (changedDateRange == false)
                {
                    Console.WriteLine("Invalid Date Range");
                    Console.Write("Press the AnyKey to try Again...");
                    Console.ReadLine();
                }
            }
            
            return true;
        }

        private static bool GenerateReportsMenu(SQLiteHandler dbHandle, string path)
        {
            var IExcelGenerator = new ExcelGenerator(path);
            var dateRange = new DateRange().GetCurrentDateRange();

            var ReportSelectionMenu = new CliMenu
            {
                OptionPadding = 3,
                MenuName = "Generate Reports:\n",
                Prompt = "Select a Report to Generate: ",
                Help = "Learn More",
                Exit = "Back to Main Menu",
                Options ={
                    "Full Report\n",
                    "Pharmacy Report",
                    "Unassigned Number Report",
                    "Caller Report\n",
                    "Support Metrics Report",
                    "Support Rep Listing\n",
                    "Change Date Range\n"
                    }
            };


            bool didGenerateReport = false;
            while (didGenerateReport == false)
            {

                ReportSelectionMenu.WriteProgramTitle();
                string reportInput = ReportSelectionMenu.WriteMenu();

                if (reportInput.ToLower() == "exit") return false;

                if (reportInput.ToLower() == "help")
                {
                    Console.Clear();
                    ReportSelectionMenu.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Generate Reports Help Menu ~~~~\n");

                    Console.WriteLine("    This menu allows you to generate various reports based on the date range and the data in the database.");
                    Console.WriteLine("    Select a report to generate by typing the corresponding number and pressing enter.");
                    Console.WriteLine("    You can also change the date range for the reports by selecting option 6.\n");

                    Console.WriteLine("\n  ~~ Full Report:");
                    Console.WriteLine("  This report contains all other reports on seperate sheets.");

                    Console.WriteLine("\n  ~~ Pharmacy Report:");
                    Console.WriteLine("  This report shows all data for each pharamcy. This report is sorted");
                    Console.WriteLine("  by Total Duration. Graphs and other analytics coming soon..");

                    Console.WriteLine("\n  ~~ Unassigned Number Report:");
                    Console.WriteLine("  This report shows data for all the unassigned phone numbers in the");
                    Console.WriteLine("  database. This report is sorted by Total Duration. it includes a ");
                    Console.WriteLine("  mini-table that shows several call records for referencing.");

                    Console.WriteLine("\n  ~~ Caller Report:");
                    Console.WriteLine("  This report shows all the data for each individual phone number in");
                    Console.WriteLine("  the database. This report is also sorted by Total Duration.");

                    Console.WriteLine("\n  ~~ Support Metrics Report:");
                    Console.WriteLine("  This report shows all the data for all the users in the database.");
                    Console.WriteLine("  Unlike the other reports this one is sorted by Total Calls instead.\n");

                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }

                if (reportInput.Length == 1)
                {
                    int optionSelection = ReportSelectionMenu.CaseOptionNumber(reportInput);

                    // End search loop and return the selected PhoneNumber
                    if (optionSelection == 0) return false;

                    ReportSelectionMenu.WriteProgramTitle();

                    switch (optionSelection)
                    {
                        case 1: // Full Report
                            IExcelGenerator.CreateFullExcelReport(dbHandle);
                            break;
                        case 2: // Pharmacy Report
                            IExcelGenerator.CreateDetailedPharmacyReport(dbHandle);
                            break;
                        case 3: // Unassigned Number Report
                            IExcelGenerator.CreateUnassignedPhoneNumberReport(dbHandle);
                            break;
                        case 4: // Caller Report
                            IExcelGenerator.CreateDetailedCallerReport(dbHandle);
                            break;
                        case 5: // Support Metrics Report
                            IExcelGenerator.CreateSupportMetricsReport(dbHandle);
                            break;
                        case 6:
                            IExcelGenerator.CreateDetailedUserListingReport(dbHandle);
                            break;
                        case 7:
                            if (ChangeDateRangeMenu(dateRange))
                            {
                                // make this confirmation screen prettier
                                ReportSelectionMenu.WriteProgramTitle();
                                Console.Write("New ");
                                dateRange.Write();

                                Console.WriteLine("Date Range Changed!");
                                Console.Write("Press the AnyKey to Continue...");
                                Console.ReadKey();
                            }
                            continue;
                    }

                    didGenerateReport = true;
                }

            }
            
            // garbage collect this excel generator
            IExcelGenerator = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();


            Console.WriteLine("Done!");
            Console.Write("Press the AnyKey to Continue...");
            Console.ReadKey();
            
            return true;
        }

        private static bool DebuggingMenu(SQLiteHandler dbHandle)
        {
            var DebuggingMenu = new CliMenu()
            {
                OptionPadding = 3,
                MenuName = "DEBUGGING Menu:\n",
                Prompt = "Select an Action: ",
                Help = "Learn More",
                Exit = "Back to Main Menu",
                Options ={
                    "Clear Database"
                }
            };

            
            bool didDebug = false;
            while (didDebug == false)
            {
                DebuggingMenu.WriteProgramTitle();
                string reportInput = DebuggingMenu.WriteMenu();

                if (reportInput.ToLower() == "exit") return false;

                if (reportInput.ToLower() == "help")
                {
                    Console.Clear();
                    DebuggingMenu.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Debugging Help Menu ~~~~\n");



                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }

                int optionSelection = DebuggingMenu.CaseOptionNumber(reportInput);

                // End search loop and return the selected PhoneNumber
                if (optionSelection == 0) return false;

                DebuggingMenu.WriteProgramTitle();

                switch (optionSelection)
                {
                    case 1: // Clear Database

                        // Make sure the user is sure
                        bool confirmClear = new CliMenu().ConfirmationPrompt
                            ($"Are You Sure You Want To Clear The Database? (y/n)");
                        if (confirmClear != true) return false;

                        bool confirmAgain = new CliMenu().ConfirmationPrompt
                            ($"Really tho? (y/n)");
                        if (confirmAgain != true) return false;

                        // Drop Tables !!!!!! DANGER !!!!!!
                        dbHandle.DropTables();
                        break;
                }

                didDebug = true;
            }

            return true;
        }


        //////////////////////////////////////  LOOPING METHODS  ///////////////

        private static PhoneNumber PhoneNumberRemovalLoop(Pharmacy selectedPharmacyToRemoveFrom)
        {
            PhoneNumber selectedPhoneNumber = null;
            bool didSelectPhoneNumber = false;

            while (didSelectPhoneNumber == false)
            {

                List<PhoneNumber> phoneNumbers = selectedPharmacyToRemoveFrom.PhoneNumbers;

                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var selectPharmacyPhoneNumber = new CliMenu()
                {
                    OptionPadding = 3,
                    Searching = true,
                    MenuName = $" Select a PhoneNumber to remove from {selectedPharmacyToRemoveFrom.Name}:",
                    Header = "   Number:               Calls:         PhoneTime:",
                    Prompt = "Type or Select a Phone Number to Remove it from a Pharmacy: ",
                    Help = "Learn More",
                    Exit = "Back to Main Menu",
                };

                foreach (PhoneNumber pn in phoneNumbers.Take(29))
                {
                    selectPharmacyPhoneNumber.Options.Add(pn.InlineCallerStatString());
                }

                Console.Clear();
                selectPharmacyPhoneNumber.WriteProgramTitle();
                string removalInput = selectPharmacyPhoneNumber.WriteMenu();

                int optionSelection = selectPharmacyPhoneNumber.CaseOptionNumber(removalInput);

                // return the selected pharmacy or search again using a different term
                if (removalInput.ToLower() == "exit") break;
                
                if (removalInput.ToLower() == "help")
                {
                    selectPharmacyPhoneNumber.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Remove PhoneNumber Help Menu ~~~~\n");
                    

                    Console.WriteLine("    This menu allows you to remove a phone number from a pharmacy.");
                    Console.WriteLine("    Select a phone number to remove by typing the corresponding number and pressing enter.");
                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }

                if (optionSelection > 0)
                {
                    // End search loop and return the selected PhoneNumber
                    selectedPhoneNumber = phoneNumbers[optionSelection - 1];
                    didSelectPhoneNumber = true;
                }
            }

            return selectedPhoneNumber;
        }

        private static PhoneNumber UnassignedPhoneNumberSelectionLoop(List<PhoneNumber> unassignedPhoneNumbers)
        {
            string searchTerm = "";
            SearchUtilities ISearchUtility = new SearchUtilities();  // make this shit static af
            PhoneNumber selectedPhoneNumber = null;
            bool didSelectPhoneNumber = false;
            
            while (didSelectPhoneNumber == false)
            {
                // Use the searchTerm to find matching pharmacies && sort them by total duration
                if (searchTerm.Length < 1) searchTerm = "";
                var searchResults = ISearchUtility.ListMatchingPhoneNumbers(searchTerm, unassignedPhoneNumbers).OrderByDescending(pn => pn.TotalDuration).ToList();


                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var searchPhoneNumberMenu = new CliMenu()
                {
                    MenuName = $"Top Unassigned Phone Numbers by Total Duration:",
                    Header = "   Number:               Calls:         PhoneTime:",
                    Searching = true,
                    Prompt = "Type or Select a Phone Number to Assign to a Pharmacy: ",
                    Help = "Learn More",
                    Exit = "Back to Main Menu",
                    OptionPadding = 3
                };

                // find the phonenumber with the most calls
                int callsCount = 0;
                foreach (PhoneNumber pn in searchResults)
                {
                    if (pn.TotalCalls > callsCount) callsCount = pn.CallRecords.Count();
                }

                foreach (PhoneNumber pn in searchResults.Take(29))
                {
                    // find the diference in length of the call record cound and the total calls of this phone number
                    int callRecordCountDifference = callsCount.ToString().Length - pn.CallRecords.Count().ToString().Length;
                    searchPhoneNumberMenu.Options.Add(pn.InlineCallerStatString(callRecordCountDifference));
                    Console.WriteLine(callRecordCountDifference);
                }

                searchPhoneNumberMenu.WriteProgramTitle();
                string searchInput = searchPhoneNumberMenu.WriteMenu();
                int optionSelection = searchPhoneNumberMenu.CaseOptionNumber(searchInput);

                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit") break;
                
                if (searchInput.ToLower() == "help")
                {
                    searchPhoneNumberMenu.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Assign PhoneNumber Help Menu ~~~~\n");
                     
                    Console.WriteLine("    This menu allows you to assign an unassigned phone number to a pharmacy.");
                    Console.WriteLine("    Select a phone number to assign by typing the corresponding number and pressing enter.");
                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }
                
                if (optionSelection > 0)
                {

                    // End search loop and return the selected pharmacy
                    selectedPhoneNumber = searchResults[optionSelection - 1];
                    Console.Clear();
                    searchPhoneNumberMenu.WriteProgramTitle();
                    selectedPhoneNumber.WriteCallerStats(20);

                    Console.Write("\nPress the AnyKey to Assign to a Pharmacy..");
                    Console.ReadKey();
                    didSelectPhoneNumber = true;
                }
                else if (searchInput.Length > 1)
                {
                    searchTerm = searchInput;
                }
            }

            return selectedPhoneNumber;
        }

        private static Pharmacy PharmacySearchSelectionLoop(List<Pharmacy> allPharmacies, string phoneNumberstr = "")
        {
            var searchTerm = "";
            SearchUtilities ISearchUtility = new SearchUtilities();
            var formatedNamesPharmacies = PadPharmacyInfoStringLength(allPharmacies);
            Pharmacy selectedPharmacy = null;

            bool didSelectPharmacy = false;
            while (didSelectPharmacy == false)
            {
                // Use the searchTerm to find matching pharmacies
                if (searchTerm.Length < 1) searchTerm = "";

                var searchResults = ISearchUtility.ListMatchingPharmacies(searchTerm, formatedNamesPharmacies);
                string menuName = phoneNumberstr.Length > 0 ? $"Search Pharmacies for {phoneNumberstr}: {searchTerm}" : $"Search Pharmacies: {searchTerm}";

                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var searchPharmacyMenu = new CliMenu()
                {
                    OptionPadding = 3,
                    Searching = true,
                    MenuName = menuName,
                    Header = "Name:                                                 PhoneNumber:       Npi:       Calls:   Phone Time:",
                    Prompt = "Enter a search term or select a pharmacy: ",
                    Help = "Learn More",
                    Exit = "Back to Main Menu"
                };

                foreach(Pharmacy ph in searchResults.Take(29))
                {
                    searchPharmacyMenu.Options.Add(ph.PharmacyInfoString());
                }

                Console.Clear();
                searchPharmacyMenu.WriteProgramTitle();
                string searchInput = searchPharmacyMenu.WriteMenu();
                int optionSelection = searchPharmacyMenu.CaseOptionNumber(searchInput);

                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit") return null;
                
                if (searchInput.ToLower() == "help")
                {
                    searchPharmacyMenu.WriteProgramTitle();
                    Console.WriteLine("\n     ~~~~ Search Pharmacies Help Menu ~~~~\n");

                    Console.WriteLine("    This menu allows you to search for a pharmacy.");
                    Console.WriteLine("    Select a pharmacy by typing the corresponding number and pressing enter.");
                    Console.WriteLine("    You can also search for a pharmacy by typing a search term and pressing enter.");
                    Console.Write("\nPress the AnyKey to Continue...");
                    Console.ReadKey();
                    continue;
                }

                if (optionSelection > 0)
                {
                    // End search loop and return the selected pharmacy

                    selectedPharmacy = searchResults[optionSelection - 1];
                    didSelectPharmacy = true;

                }
                else if (searchInput.Length > 1)
                {
                    searchTerm = searchInput;
                }
            }

            return selectedPharmacy;
        }


        ///////////////////////////////////// Menu Formatting Helpers //////////
        static List<Pharmacy> PadPharmacyInfoStringLength(List<Pharmacy> pharmacies)
        {
            // find the longest string in the top 10 results
            int longestStringLength = 0;
            foreach (Pharmacy ph in pharmacies)
            {
                // split the string at "-"
                int nameLength = ph.PharmacyInfoString().Split('-')[0].Length;
                if (nameLength > longestStringLength) longestStringLength = nameLength;
            }

            // pad the strings to be the same length
            foreach (Pharmacy ph in pharmacies)
            {
                string[] splitString = ph.PharmacyInfoString().Split('-');
                string paddedString = splitString[0].PadRight(longestStringLength, ' ');
                ph.Name = paddedString;
            }

            return pharmacies;
        }

    }
}