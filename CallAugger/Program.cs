using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CallAugger.Utilities;
using CallAugger.Controllers.DataImporters;
using CallAugger.Controllers.Parsers;
using CallAugger.Controllers.Generators;
using System.Net.Sockets;

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
    // output a formatted excel file containing several sheets.
    // Each sheet will show a different set of data.

    // The files will be in as CSV and or Excel format.

    // Being able to add phone numbers to a pharmacy should be
    // a feature of the program, Removal as well. 

    // An addition that can be made is the tracking of support rep
    // calls and statistics. these reports can be included in the
    // output excel file.

    ///////////////////////////////////////////////////////////////

    
    class Program
    {

        static void Main(string[] args)
        {
            // Get path
            string path = Directory.GetCurrentDirectory();


            // configure console window
            Console.SetWindowSize(115, 45);


            // Import Data and Create Data Stores
            var IDataDoer = new DataImporter();
            List<CallRecord> allCallRecords = IDataDoer.ImportCallData(path);
            List<Pharmacy> allPharmacies = IDataDoer.ImportPharmacyData(path);


            // Parce Data
            var IParser = new Parse();
            
            // a list of all phone numbers                            - sorted by TotalDuration
            List<PhoneNumber> allPhoneNumbers = 
                IParser.ParseCallRecordData(allCallRecords).OrderByDescending(pn => pn.TotalDuration).ToList();

            // a list of pharmacies that matched call records         - sorted by TotalDuration
            List<Pharmacy> matchingPharmacies = 
                IParser.ParsePharmacyData(allPharmacies).OrderByDescending(ph => ph.TotalDuration).ToList();
           
           

            // create a new Excel file
            //var IExcelGenerator = new ExcelGenerator(path);

            //IExcelGenerator.CreateNewExcelFile();

         
            /////////////////////// BEGIN MAIN PROGRAM LOOP ///////////////////////////
            bool GameOver = false;
            while (GameOver == false)
            {
                // creat a  CLI main menu
                var MainMenu = new CliMenu
                {
                    OptionPadding = 3,
                    Exit     = "Close Program",
                    Options  = {
                        "Assign Unassigned Phone Numbers",
                        "Remove Phone Number from Pharmacy\n",
                        "View Top Callers",
                        "View Support Rep Metrics\n",
                        "Generate Reports"

                    }
                };
               

                int SelectedOption = MainMenu.CaseOptionNumber(MainMenu.WriteMenu());
                switch (SelectedOption)
                {
                    case 1: // View Unassigned Phone Numbers
                        
                        Console.Clear();
                        MainMenu.ShowSelected(1);


                        PhoneNumber selectedPhoneNumberToAdd = UnassignedPhoneNumberSelectionLoop(IParser);

                        if (selectedPhoneNumberToAdd == null) break;

                        Pharmacy selectedPharmacyToAddTo = PharmacySearchSelectionLoop(IParser, allPharmacies);


                        // add the new phone number to the selected pharmacy
                        if (selectedPharmacyToAddTo != null && selectedPhoneNumberToAdd != null)
                        {
                            bool confirm = MainMenu.ConfirmationPrompt(
                               $"Are You Sure {selectedPhoneNumberToAdd.FormatedPhoneNumber()} Should Be Added To {selectedPharmacyToAddTo.Name}?"
                               );

                            if (confirm == true)
                            {
                                selectedPharmacyToAddTo.AddPhoneNumber(selectedPhoneNumberToAdd);
                                IParser.UnassignedPhoneNumbers.Remove(selectedPhoneNumberToAdd);

                                Console.Clear();
                                selectedPharmacyToAddTo.WritePharmacyInfo();
                            
                                Console.WriteLine("\n Added {0} to {1}", selectedPhoneNumberToAdd.FormatedPhoneNumber(), selectedPharmacyToAddTo.Name);
                                Console.Write("\nPress the AnyKey to Continue...");
                                Console.ReadKey();
                            }
                        }

                        break;
                    case 2: // Remove Phone Number from Pharmacy

                        Console.Clear();
                        MainMenu.ShowSelected(2);

                        Pharmacy selectedPharmacyToRemoveFrom = PharmacySearchSelectionLoop(IParser, allPharmacies);

                        if (selectedPharmacyToRemoveFrom == null) break;

                        PhoneNumber selectedPhoneNumberToRemove = PhoneNumberRemovalLoop(selectedPharmacyToRemoveFrom);


                        if (selectedPharmacyToRemoveFrom != null && selectedPhoneNumberToRemove != null)
                        {
                            if (selectedPharmacyToRemoveFrom.PhoneNumbers.Count() > 1)
                            {
                                bool confirm = MainMenu.ConfirmationPrompt(
                                    $"(Y/N)  Are You Sure {selectedPhoneNumberToRemove.FormatedPhoneNumber()} Should Be Removed From {selectedPharmacyToRemoveFrom.Name}?"
                                    );

                                if (confirm == true)
                                {
                                    // remove this phone number from the pharmacy and add it back to the list of unassigned
                                    selectedPharmacyToRemoveFrom.PhoneNumbers.Remove(selectedPhoneNumberToRemove);
                                    IParser.UnassignedPhoneNumbers.Add(selectedPhoneNumberToRemove);

                                    Console.Clear();
                                    selectedPharmacyToRemoveFrom.WritePharmacyInfo();

                                    Console.WriteLine("\n Removed {0} from {1}", selectedPhoneNumberToRemove.FormatedPhoneNumber(), selectedPharmacyToRemoveFrom.Name);
                                }

                            }
                            else if (selectedPharmacyToRemoveFrom.PhoneNumbers.Count() == 1)
                            {
                                Console.WriteLine("\n Cannot Remove Only Phone Number");
                            }
                            else if (selectedPharmacyToRemoveFrom.PrimaryPhoneNumber == selectedPhoneNumberToRemove.Number)
                            {
                                Console.WriteLine("\n Cannot Remove Primary PhoneNumber");
                            }

                            Console.Write("\nPress the AnyKey to Continue...");
                            Console.ReadKey();
                        }

                        break;
                    case 3: // View Top Pharmacies
                        Console.Clear();
                        MainMenu.ShowSelected(3);

                        Console.Write(" Number of Pharmacies to Show? ");
                        string strOfPharmacies = Console.ReadLine();
                        
                        if (strOfPharmacies.Length < 1) strOfPharmacies = "0";
                        int numOfPharmacies = Convert.ToInt32(strOfPharmacies);

                        Console.Write(" Number of Unassigned Callers to show? ");
                        string strOfUnassignedPhoneNumbers = Console.ReadLine();

                        if (strOfUnassignedPhoneNumbers.Length < 1) strOfUnassignedPhoneNumbers = "0";

                        int numOfUnassignedPhoneNumbers = Convert.ToInt32(strOfUnassignedPhoneNumbers);

                        int numOfCallRecords = 3;

                        Console.Clear();
                        MainMenu.ShowSelected(3);

                        WriteTopScores(numOfPharmacies, numOfUnassignedPhoneNumbers, numOfCallRecords, matchingPharmacies, IParser.UnassignedPhoneNumbers);

                        Console.Write("\nPress the AnyKey to Continue...");
                        Console.ReadKey();

                        break;
                    case 4: // View Rep Metrics
                        Console.Clear();
                        MainMenu.ShowSelected(4);


                        //UserSearchLoop();


                        // show all users and their metrics
                        foreach (var user in IParser.ParsedUsers.OrderBy(pu => pu.Name))
                        {
                            Console.WriteLine("\n\n");
                            user.WriteUserStats();
                        }

                        Console.Write("\nPress the AnyKey to Continue...");
                        Console.ReadKey();

                        break;
                    case 5: // Generate Reports
                        MainMenu.ShowSelected(4);
                        break;
                }
            }
            /////////////////////// END MAIN LOOP ///////////////////////////
        }

        //////////////////////////////////////  MAIN METHODS  ///////////////
        
        static PhoneNumber PhoneNumberRemovalLoop(Pharmacy selectedPharmacyToRemoveFrom)
        {
            PhoneNumber selectedPhoneNumber = null;
            bool didSelectPhoneNumber = false;

            while (didSelectPhoneNumber == false)
            {

                List<PhoneNumber> phoneNumbers = selectedPharmacyToRemoveFrom.PhoneNumbers;

                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var selectPharmacyPhoneNumber = new CliMenu()
                {
                    MenuName = $" Select a PhoneNumber to remove from {selectedPharmacyToRemoveFrom.Name}",
                    Exit     = " Back to Main Menu",
                    Options  = {
                        phoneNumbers.Count > 0 ? phoneNumbers[0].InlineCallerStatString() : null,
                        phoneNumbers.Count > 1 ? phoneNumbers[1].InlineCallerStatString() : null,
                        phoneNumbers.Count > 2 ? phoneNumbers[2].InlineCallerStatString() : null,
                        phoneNumbers.Count > 3 ? phoneNumbers[3].InlineCallerStatString() : null,
                        phoneNumbers.Count > 4 ? phoneNumbers[4].InlineCallerStatString() : null,
                        phoneNumbers.Count > 5 ? phoneNumbers[5].InlineCallerStatString() : null,
                        phoneNumbers.Count > 6 ? phoneNumbers[6].InlineCallerStatString() : null,
                        phoneNumbers.Count > 7 ? phoneNumbers[7].InlineCallerStatString() : null,
                        phoneNumbers.Count > 8 ? phoneNumbers[8].InlineCallerStatString() : null,
                     }
                };

                string removalInput = selectPharmacyPhoneNumber.WriteMenu();



                if (removalInput.ToLower() == "exit")
                {
                    break;
                }
                else if (removalInput.Length == 1)
                {
                    int optionSelection = selectPharmacyPhoneNumber.CaseOptionNumber(removalInput);

                    // End search loop and return the selected PhoneNumber
                    if (optionSelection > 0)
                    {
                        selectedPhoneNumber = phoneNumbers[optionSelection - 1];
                        didSelectPhoneNumber = true;
                    }
                }
            }

            return selectedPhoneNumber;
        }
      
        
        static PhoneNumber UnassignedPhoneNumberSelectionLoop(Parse IParser)
        {
            string searchTerm = "";
            SearchUtilities ISearchUtility = new SearchUtilities();
            PhoneNumber selectedPhoneNumber = null;
            bool didSelectPhoneNumber = false;

            while (didSelectPhoneNumber == false)
            {
                // Use the searchTerm to find matching pharmacies && sort them by total duration
                if (searchTerm.Length < 1) searchTerm = "";
                var searchResults = ISearchUtility.ListMatchingPhoneNumbers(searchTerm, IParser.UnassignedPhoneNumbers).OrderByDescending(pn => pn.TotalDuration).ToList();


                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var searchPhoneNumberMenu = new CliMenu()
                {
                    MenuName = $" Top Unassigned Phone Numbers by Total Duration...",
                    Searching = true,
                    Prompt = "Type or Select a Phone Number to Assign to a Pharmacy: ",
                    Exit = " Back to Main Menu",
                    Options = {
                        searchResults.Count > 0 ? searchResults[0].InlineCallerStatString() : null,
                        searchResults.Count > 1 ? searchResults[1].InlineCallerStatString() : null,
                        searchResults.Count > 2 ? searchResults[2].InlineCallerStatString() : null,
                        searchResults.Count > 3 ? searchResults[3].InlineCallerStatString() : null,
                        searchResults.Count > 4 ? searchResults[4].InlineCallerStatString() : null,
                        searchResults.Count > 5 ? searchResults[5].InlineCallerStatString() : null,
                        searchResults.Count > 6 ? searchResults[6].InlineCallerStatString() : null,
                        searchResults.Count > 7 ? searchResults[7].InlineCallerStatString() : null,
                        searchResults.Count > 8 ? searchResults[8].InlineCallerStatString() : null,
                    },
                    OptionPadding = 5
                };

                string searchInput = searchPhoneNumberMenu.WriteMenu();


                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit")
                {
                    break;
                }
                else if (searchInput.Length == 1)
                {
                    int optionSelection = searchPhoneNumberMenu.CaseOptionNumber(searchInput);

                    // End search loop and return the selected pharmacy
                    if (optionSelection > 0)
                    {
                        selectedPhoneNumber = searchResults[optionSelection - 1];

                        selectedPhoneNumber.WriteCallerStats(5);

                        Console.Write("\nPress the AnyKey to Assign to a Pharmacy..");
                        Console.ReadKey();
                        didSelectPhoneNumber = true;
                    }
                }
                else if (searchInput.Length > 1)
                {
                    searchTerm = searchInput;
                }
            }

            return selectedPhoneNumber;
        }

        
        static Pharmacy PharmacySearchSelectionLoop(Parse IParser, List<Pharmacy> allPharmacies)

        {
            var searchTerm = "";
            SearchUtilities ISearchUtility = new SearchUtilities();

            Pharmacy selectedPharmacy = null;
            bool didSelectPharmacy = false;
            while (didSelectPharmacy == false)
            {
                // Use the searchTerm to find matching pharmacies
                if (searchTerm.Length < 1) searchTerm = "";

                var searchResults = ISearchUtility.ListMatchingPharmacies(searchTerm, allPharmacies);

                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var searchPharmacyMenu = new CliMenu()
                {
                    MenuName = $" Search Pharmacies: {searchTerm}...",
                    Searching = true,
                    Prompt = "Enter a search term or select a pharmacy: ",
                    Exit = " Back to Main Menu",
                    Options = {
                        searchResults.Count > 0 ? searchResults[0].PharmacyInfoString() : null,
                        searchResults.Count > 1 ? searchResults[1].PharmacyInfoString() : null,
                        searchResults.Count > 2 ? searchResults[2].PharmacyInfoString() : null,
                        searchResults.Count > 3 ? searchResults[3].PharmacyInfoString() : null,
                        searchResults.Count > 4 ? searchResults[4].PharmacyInfoString() : null,
                        searchResults.Count > 5 ? searchResults[5].PharmacyInfoString() : null,
                        searchResults.Count > 6 ? searchResults[6].PharmacyInfoString() : null,
                        searchResults.Count > 7 ? searchResults[7].PharmacyInfoString() : null,
                        searchResults.Count > 8 ? searchResults[8].PharmacyInfoString() : null,

                    }
                };

                string searchInput = searchPharmacyMenu.WriteMenu();

                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit")
                {
                    break;
                }
                else if (searchInput.Length == 1)
                {
                    int optionSelection = searchPharmacyMenu.CaseOptionNumber(searchInput);

                    // End search loop and return the selected pharmacy
                    if (optionSelection > 0)
                    {
                        selectedPharmacy = searchResults[optionSelection - 1];
                        didSelectPharmacy = true;
                    }
                }
                else if (searchInput.Length > 1)
                {
                    searchTerm = searchInput;
                }
            }

            return selectedPharmacy;
        }


        static void UserSearchLoop(Parse IParser)
        {
            var searchTerm = "A";
            SearchUtilities ISearchUtility = new SearchUtilities();

            User selectedUser = null;
            bool didSelectUser = false;
            while (didSelectUser == false)
            {
                // Use the searchTerm to find matching pharmacies
                if (searchTerm.Length < 1) searchTerm = "A";

                var searchResults = ISearchUtility.ListMatchingUsers(searchTerm, IParser.ParsedUsers.ToList());

                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var searchPharmacyMenu = new CliMenu()
                {
                    MenuName = $" Search Pharmacies: {searchTerm}...",
                    Searching = true,
                    Prompt = "Enter a search term or select a pharmacy: ",
                    Exit = " Back to Main Menu",
                    Options =
                    {
                        searchResults.Count > 0 ? searchResults[0].InlineUserInfo() : null,
                        searchResults.Count > 1 ? searchResults[1].InlineUserInfo() : null,
                        searchResults.Count > 2 ? searchResults[2].InlineUserInfo() : null,
                        searchResults.Count > 3 ? searchResults[3].InlineUserInfo() : null,
                        searchResults.Count > 4 ? searchResults[4].InlineUserInfo() : null,
                        searchResults.Count > 5 ? searchResults[4].InlineUserInfo() : null,
                    }
                };

                string searchInput = searchPharmacyMenu.WriteMenu();

                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit")
                {
                    break;
                }
                else if (searchInput.Length == 1)
                {
                    int optionSelection = searchPharmacyMenu.CaseOptionNumber(searchInput);

                    // End search loop and return the selected pharmacy
                    if (optionSelection > 0)
                    {
                        selectedUser = searchResults[optionSelection - 1];
                        didSelectUser = true;
                    }
                }
                else if (searchInput.Length > 1)
                {
                    searchTerm = searchInput;
                }
            }
        }

        
        // Write the top n pharmacies and top n phone numbers to the console
        private static void WriteTopScores(int topPharmacyCount, int topPhoneNumberCount, int topCallsCount, List<Pharmacy> matchingPharmacies, List<PhoneNumber> unassignedPhoneNumbers)
        {
            // Write the top n pharmacies sorted by total call time
            List<Pharmacy> topPharmacies = matchingPharmacies.OrderByDescending(pharmacy => pharmacy.TotalDuration).ToList();

            Console.WriteLine("\n Top {0} Pharmacys Sorted by Call Duration:\n", topPharmacyCount);
            foreach (var ph in topPharmacies.Take(topPharmacyCount))
            {
                // will write Ph info and each phone number's stats
                ph.WritePharmacyInfo(topCallsCount);
                Console.WriteLine("\n\n\n");
            }


            // Write the top n phone numbers sorted by total call time
            var topPhoneNumbers = unassignedPhoneNumbers.OrderByDescending(phoneNumber => phoneNumber.TotalDuration).ToList();

            if (topPhoneNumberCount != 0) Console.WriteLine("\n top {0} Unassigned PhoneNumbers:", topPhoneNumberCount);
            foreach (var pn in topPhoneNumbers.OrderByDescending(pn => pn.TotalDuration).Take(topPhoneNumberCount))
            {
                // will just write phone number's stats
                pn.WriteCallerStats(topCallsCount);
            }
        }

    }
}