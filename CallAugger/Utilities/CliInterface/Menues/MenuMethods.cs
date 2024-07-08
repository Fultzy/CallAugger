using CallAugger.Generators;
using CallAugger.Readers;
using CallAugger.Settings;
using CallAugger.Utilities.Sqlite;
using CallAugger.Utilities.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger.Utilities.CliInterface
{
    static class MenuMethods
    {
        ////////////////////////  MENU METHODS  ////////////////////////
        public static bool AddPhoneNumberMenu(SQLiteHandler dbHandle)
        {
            // these two loops allow the user to find and select a pharmacy and a phone number
            PhoneNumber selectedPhoneNumberToAdd = UnassignedPhoneNumberSelectionLoop(dbHandle);
            if (selectedPhoneNumberToAdd == null) return false;

            Pharmacy selectedPharmacyToAddTo = PharmacySearchSelectionLoop
                (dbHandle, selectedPhoneNumberToAdd.FormattedPhoneNumber());

            if (selectedPharmacyToAddTo == null) return false;


            // Make sure the user is sure
            bool confirmAdd = new CliMenu().ConfirmationPrompt(
                $"\nAre You Sure {selectedPhoneNumberToAdd.FormattedPhoneNumber()} Should Be Added To {selectedPharmacyToAddTo.Name.Trim()}? (y/n)");


            // add the new phone number to the selected pharmacy
            if (confirmAdd != true) return false;
            selectedPharmacyToAddTo.AddPhoneNumber(selectedPhoneNumberToAdd);
            dbHandle.UpdatePhoneNumberPharmacyID(selectedPhoneNumberToAdd, selectedPharmacyToAddTo.id);


            // present confirmation screen
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            selectedPharmacyToAddTo.WritePharmacyInfo();
            Console.WriteLine("\n Added {0} to {1}", selectedPhoneNumberToAdd.FormattedPhoneNumber(), selectedPharmacyToAddTo.Name.Trim());

            menu.AnyKey();
            return true;
        }

        public static bool RemovePhoneNumberMenu(SQLiteHandler dbHandle)
        {

            // these two loops allow the user to find and select a pharmacy and a phone number
            Pharmacy selectedPharmacyToRemoveFrom = PharmacySearchSelectionLoop(dbHandle);
            if (selectedPharmacyToRemoveFrom == null) return false;
            PhoneNumber selectedPhoneNumberToRemove = PhoneNumberRemovalLoop(selectedPharmacyToRemoveFrom);
            if (selectedPhoneNumberToRemove == null) return false;

            var menu = new CliMenu();

            // Prevent the user from removing the primary phone number
            if (selectedPharmacyToRemoveFrom.PrimaryPhoneNumber == selectedPhoneNumberToRemove.Number)
            {
                Console.WriteLine("\n Cannot Remove Primary PhoneNumber");

                menu.AnyKey();
                return false;
            }


            // remove the selected phone number from the selected pharmacy
            if (selectedPharmacyToRemoveFrom.PhoneNumbers.Count() > 0)
            {
                // Make sure the user is sure
                bool confirmRemove = new CliMenu().ConfirmationPrompt(
                    $"\nAre You Sure {selectedPhoneNumberToRemove.FormattedPhoneNumber()} Should Be Removed From {selectedPharmacyToRemoveFrom.Name.Trim()}? (y/n)");
                if (confirmRemove != true) return false;

                dbHandle.UpdatePhoneNumberPharmacyID(selectedPhoneNumberToRemove, 0);
                selectedPharmacyToRemoveFrom.PhoneNumbers.Remove(selectedPhoneNumberToRemove);
            }

            // present confirmation screen
            menu.WriteProgramTitle();

            selectedPharmacyToRemoveFrom.WritePharmacyInfo();

            Console.WriteLine("\n Removed {0} from {1}",
                selectedPhoneNumberToRemove.FormattedPhoneNumber(),
                selectedPharmacyToRemoveFrom.Name.Trim());

            menu.AnyKey();
            return true;
        }

        public static bool ChangeDateRangeMenu(DateRange dateRange)
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

                if (dateRangeInput.ToLower() == "help") HelpMenus.ChangeDateRange();

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
                        DateRangeMenu.AnyKey();
                    }
                }

                if (changedDateRange == false)
                {
                    Console.WriteLine("Invalid Date Range");
                    DateRangeMenu.AnyKey();

                }
            }

            DateRangeMenu.WriteProgramTitle();
            Console.WriteLine("\n\nDate Range Changed!");

            DateRangeMenu.AnyKey();
            return true;
        }

        public static bool GenerateReportsMenu(SQLiteHandler dbHandle, string path)
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
                if (reportInput.ToLower() == "help") HelpMenus.GenerateReports();

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
                                ReportSelectionMenu.AnyKey();
                            }
                            continue;
                    }

                    didGenerateReport = true;
                }

            }

            // garbage collect this excel generator
            GC.Collect();
            GC.WaitForPendingFinalizers();

            ReportSelectionMenu.AnyKey();
            return true;
        }

        public static bool PharmacyEditMenu(SQLiteHandler dbHandle, Pharmacy selectedPharmacy)
        {
            var PharmacyEditMenu = new CliMenu
            {
                OptionPadding = 3,
                MenuName = $"Edit {selectedPharmacy.Name}:\n",
                Prompt = "Select an Action: ",
                Help = "Learn More",
                Exit = "Back to Main Menu"
            };


            bool didEdit = false;
            while (didEdit == false)
            {
                PharmacyEditMenu.Options = new List<string>
                {
                    "Edit Name",
                    "Edit Creds",
                    "Edit Details",
                    "Delete Pharmacy"
                };

                PharmacyEditMenu.WriteProgramTitle();
                selectedPharmacy.WritePharmacyInfo();
                string editInput = PharmacyEditMenu.WriteHorizontalMenu();

                if (editInput.ToLower() == "exit") return false;
                if (editInput.ToLower() == "help") HelpMenus.PharmacyEdit();

                int optionSelection = PharmacyEditMenu.CaseOptionNumber(editInput);

                switch (optionSelection)
                {
                    case 0:
                        return false;

                    case 1: // Edit Name

                        string input = "";
                        while (!(PharmacyValidator.IsName(input) == "Input is valid."))
                        {
                            input = PharmacyEditMenu.StringPrompt("Enter New Name: ");
                        }

                        selectedPharmacy.Name = input;
                        dbHandle.PharmacyRepo.Update(selectedPharmacy);

                        break;
                 
                    case 2: // Edit Creds
                        
                        PharmacyEditMenu.Options = new List<string>
                        {
                            "Edit Npi",
                            "Edit Dea",
                            "Edit Ncpdp"
                        };

                        PharmacyEditMenu.WriteProgramTitle();
                        selectedPharmacy.WritePharmacyInfo();
                        string editCredsInput = PharmacyEditMenu.WriteHorizontalMenu();

                        if (editInput.ToLower() == "exit") return false;
                        if (editInput.ToLower() == "help") HelpMenus.PharmacyEdit();

                        int credsOptionSelection = PharmacyEditMenu.CaseOptionNumber(editCredsInput);

                        switch (credsOptionSelection)
                        {
                            case 0:
                                return false;

                            case 1: // Edit Npi

                                selectedPharmacy.Npi = new CliMenu().StringPrompt("Enter New Npi: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                            case 3: // Edit Dea

                                selectedPharmacy.Dea = new CliMenu().StringPrompt("Enter New Dea: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                            case 4: // Edit Ncpdp

                                selectedPharmacy.Ncpdp = new CliMenu().StringPrompt("Enter New Ncpdp: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                        }

                        break;

                    case 3: // Edit Details

                        PharmacyEditMenu.Options = new List<string>
                        {
                            "Edit Address",
                            "Edit City",
                            "Edit State",
                            "Edit ZipCode",
                            "Edit Contact Name 1",
                            "Edit Contact Name 2",
                            "Edit Anniversary"
                        };

                        PharmacyEditMenu.WriteProgramTitle();
                        selectedPharmacy.WritePharmacyInfo();
                        string editDetailsInput = PharmacyEditMenu.WriteHorizontalMenu();

                        if (editInput.ToLower() == "exit") return false;
                        if (editInput.ToLower() == "help") HelpMenus.PharmacyEdit();

                        int detailsOptionSelection = PharmacyEditMenu.CaseOptionNumber(editDetailsInput);

                        switch (detailsOptionSelection)
                        {
                            case 0: 
                                return false;

                            case 1: // Edit Address

                                selectedPharmacy.Address = new CliMenu().StringPrompt("Enter New Street Address: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                            case 2: // Edit City

                                selectedPharmacy.City = new CliMenu().StringPrompt("Enter New City: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                            case 3: // Edit State

                                selectedPharmacy.State = new CliMenu().StringPrompt("Enter New State: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;

                            case 4: // Edit Zip
                                
                                selectedPharmacy.Zip = new CliMenu().StringPrompt("Enter New Zip: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);
    
                                break;
                            case 5: // Edit Contact Name 1

                                selectedPharmacy.ContactName1 = new CliMenu().StringPrompt("Enter New Contact Name 1: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                            case 6: // Edit Contact Name 2

                                selectedPharmacy.ContactName2 = new CliMenu().StringPrompt("Enter New Contact Name 2: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                            case 7: // Edit Anniversary

                                selectedPharmacy.Anniversary = new CliMenu().StringPrompt("Enter New Anniversary: ");
                                dbHandle.PharmacyRepo.Update(selectedPharmacy);

                                break;
                        }

                        break;

                    case 4: // Delete Pharmacy

                        bool confirmDelete = new CliMenu().ConfirmationPrompt(
                            $"\nAre You Sure {selectedPharmacy.Name} Should Be Deleted? (y/n)");
                        if (confirmDelete != true) return false;

                        dbHandle.PharmacyRepo.Delete(selectedPharmacy);
                        return true;
                }
            }

            return true;
        }

        public static bool DebuggingMenu(SQLiteHandler dbHandle)
        {

            var DebuggingMenu = new CliMenu()
            {
                OptionPadding = 3,
                MenuName = "DEBUGGING Menu:\n",
                Prompt = "Select an Action: ",
                Help = "Learn More",
                Exit = "Back to Main Menu",
                Options = {
                    "Clear Database",
                    "Rebuild db w/Newest Pharmacy and CallRecord Files\n",
                    "Export Manually Assigned PhoneNumbers to CSV",
                    "Import Manually Assinged PhoneNumbers from CSV\n",
                    "Simulate assigning PhoneNumbers to Pharmacies\n",
                    "Import from older db",
                    "do thing 2"
                }
            };


            bool didDebug = false;
            while (didDebug == false)
            {
                DebuggingMenu.WriteProgramTitle();
                string DebugInput = DebuggingMenu.WriteMenu();

                if (DebugInput.ToLower() == "exit") return false;

                if (DebugInput.ToLower() == "help")
                {
                    HelpMenus.Debugging();
                    continue;
                }

                int optionSelection = DebuggingMenu.CaseOptionNumber(DebugInput);

                if (optionSelection == 0) return false;

                DebuggingMenu.WriteProgramTitle();

                switch (optionSelection)
                {
                    case 1: // Clear Database

                        Debugging.Cleardb(dbHandle);
                        DebuggingMenu.AnyKey();

                        break;

                    case 2: // Rebuild db w/Newest Pharmacy and CallRecord Files

                        Debugging.RebuildDB(dbHandle);
                        DebuggingMenu.AnyKey();

                        break;
                    case 3: // Export Assigned PhoneNumbers to CSV

                        var csvGenerator = new CsvGenerator();

                        csvGenerator.ExportAssignedPhoneNumberCSV(dbHandle);
                        DebuggingMenu.AnyKey();

                        break;
                    case 4: // Import Assigned PhoneNumbers from CSV

                        var csvReader = new CsvReader();

                        csvReader.ImportAssignedPhoneNumbers(dbHandle);
                        DebuggingMenu.AnyKey();

                        break;
                    case 5: // Simulate assigning PhoneNumbers to Pharmacies

                        Debugging.SimulateAssignment(dbHandle);
                        DebuggingMenu.AnyKey();

                        break;
                    case 6: // Import from older db

                        Debugging.ImportAssignmentsFromv1_1db(dbHandle);
                        DebuggingMenu.AnyKey();

                        break;
                    case 7: // do thing 2


                        break;
                }

                didDebug = true;
            }
            
            return true;
        }


        //////////////////////////////////////  LOOPING METHODS  ///////////////
        public static PhoneNumber PhoneNumberRemovalLoop(Pharmacy selectedPharmacyToRemoveFrom)
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

                if (removalInput.ToLower() == "help") HelpMenus.PhoneNumberRemoval();

                if (optionSelection > 0)
                {
                    // End search loop and return the selected PhoneNumber
                    selectedPhoneNumber = phoneNumbers[optionSelection - 1];
                    didSelectPhoneNumber = true;
                }
            }

            return selectedPhoneNumber;
        }

        public static PhoneNumber UnassignedPhoneNumberSelectionLoop(SQLiteHandler dbHandle)
        {
            string searchTerm = "";
            PhoneNumber selectedPhoneNumber = null;

            bool didSelectPhoneNumber = false;
            while (didSelectPhoneNumber == false)
            {
                // Use the searchTerm to find matching pharmacies && sort them by total duration
                if (searchTerm.Length < 1) searchTerm = "";
                var results = SearchUtilities.ListMatchingUnassignedPhoneNumbers(searchTerm, dbHandle.GetUnassignedPhoneNumbers(), 29);


                // create a CLI menu to select a pharmacy showing the top 5 unassigned phonenumbers
                var searchPhoneNumberMenu = new CliMenu()
                {
                    MenuName = $"Top Unassigned Phone Numbers by Total Duration    Unassigned PhoneTime: {dbHandle.PhoneNumberRepo.TotalUnassignedPhoneTime()}",
                    Header = "   Number:               Calls:      PhoneTime:",
                    Searching = true,
                    Prompt = "Type or Select a Phone Number to Assign to a Pharmacy: ",
                    Help = "Learn More",
                    Exit = "Back to Main Menu",
                    OptionPadding = 3
                };

                // find max call and duration digits counts
                int maxCallDigits = results.Max(pn => pn.TotalCalls).ToString().Length;
                int maxDurationDigits = results.Max(pn => pn.FormattedTotalDuration()).Length;

                foreach (PhoneNumber pn in results)
                {
                    if (pn.TotalCalls == 0) continue;

                    searchPhoneNumberMenu.Options.Add(pn.InlineCallerStatString(maxCallDigits, maxDurationDigits));
                }

                searchPhoneNumberMenu.WriteProgramTitle();
                string searchInput = searchPhoneNumberMenu.WriteMenu();
                int optionSelection = searchPhoneNumberMenu.CaseOptionNumber(searchInput);

                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit") break;

                if (searchInput.ToLower() == "help") HelpMenus.PhoneNumberAddition();

                if (optionSelection > 0)
                {
                    // End search loop and return the selected pharmacy
                    searchPhoneNumberMenu.WriteProgramTitle();

                    selectedPhoneNumber = results[optionSelection - 1];
                    selectedPhoneNumber.WriteCallerStats(20);

                    Console.Write("\nPress Any Key to Assign {0} to a Pharmacy", selectedPhoneNumber.FormattedPhoneNumber());
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

        public static Pharmacy PharmacySearchSelectionLoop(SQLiteHandler dbHandle, string phoneNumberstr = "")
        {
            var searchTerm = "";
            Pharmacy selectedPharmacy = null;

            bool didSelectPharmacy = false;
            while (didSelectPharmacy == false)
            {
                // Use the searchTerm to find matching pharmacies
                if (searchTerm.Length < 1) searchTerm = "";
                var results = SearchUtilities.ListMatchingPharmacies(searchTerm, dbHandle.PharmacyRepo.Pharmacies, 29);

                // find max call and duration digits counts
                int maxCallDigits = results.Max(ph => ph.TotalCalls).ToString().Length;
                int maxNameCharacters = results.Max(ph => ph.Name.Length);

                // create the Header
                string menuName = phoneNumberstr.Length > 0 ? $"Search Pharmacies for {phoneNumberstr}: {searchTerm}" : $"Search Pharmacies: {searchTerm}";


                // create a CLI menu to select a pharmacy
                var searchPharmacyMenu = new CliMenu()
                {
                    OptionPadding = 1,
                    Searching = true,
                    MenuName = menuName,
                    Header = $"Name: {Formatter.PadString("", maxNameCharacters - 3)} PhoneNumber:       Npi:       Calls:    PhoneTime:",
                    Prompt = "Enter a search term or select a pharmacy: ",
                    Help = "Learn More",
                    Exit = "Back to Main Menu"
                };

                // add results to menu options
                foreach (Pharmacy ph in results)
                {
                    searchPharmacyMenu.Options.Add(ph.PharmacyInfoString(maxNameCharacters, maxCallDigits));
                }


                // write the menu and get the user input
                searchPharmacyMenu.WriteProgramTitle();
                string searchInput = searchPharmacyMenu.WriteMenu();
                int optionSelection = searchPharmacyMenu.CaseOptionNumber(searchInput);

                // return the selected pharmacy or search again using a different term
                if (searchInput.ToLower() == "exit") return null;

                if (searchInput.ToLower() == "help") HelpMenus.PharmacySelection();

                if (optionSelection > 0)
                {
                    // End search loop and return the selected pharmacy
                    selectedPharmacy = results[optionSelection - 1];
                    didSelectPharmacy = true;
                }
                else if (searchInput.Length > 1)
                {
                    // replace search term with new search term
                    searchTerm = searchInput;
                }
                else
                {
                    searchTerm = "";
                }
            }

            return selectedPharmacy;
        }
    }
}
