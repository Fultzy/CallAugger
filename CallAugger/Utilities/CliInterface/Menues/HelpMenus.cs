using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities
{
    static class HelpMenus
    {
        static public void MainMenu()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Main Help Menu ~~~~\n");
            Console.WriteLine("    Select an option by typing the corresponding number and pressing enter.\n");
            Console.WriteLine(" !!!Learn more by viewing the help menues within each option for more details!!!");
            Console.WriteLine("\n Initial comment:");
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

            menu.AnyKey();
        }

        static public void PhoneNumberRemoval()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Remove PhoneNumber Help Menu ~~~~\n");
            Console.WriteLine("    This menu allows you to remove a phone number from a pharmacy.");
            Console.WriteLine("    Select a phone number to remove by typing the corresponding number and pressing enter.");

            menu.AnyKey();
        }

        static public void PhoneNumberAddition()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Assign PhoneNumber Help Menu ~~~~\n");
            Console.WriteLine("    This menu allows you to assign an unassigned phone number to a pharmacy.");
            Console.WriteLine("    Select a phone number to assign by typing the corresponding number and pressing enter.");

            menu.AnyKey();
        }

        static public void PharmacySelection()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Search Pharmacies Help Menu ~~~~\n");
            Console.WriteLine("    This menu allows you to search for a pharmacy.");
            Console.WriteLine("    Select a pharmacy by typing the corresponding number and pressing enter.");
            Console.WriteLine("    You can also search for a pharmacy by typing a search term and pressing enter.");

            menu.AnyKey();
        }

        static public void ChangeDateRange()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Change Date Range Help Menu ~~~~\n");
            Console.WriteLine("    This menu allows you to change the date range for the reports generated in the program.");
            Console.WriteLine("    Select one of the predefined date ranges by typing the corresponding number and pressing enter.");
            Console.WriteLine("    You can also type a custom date range in the format (mm/dd/yyyy - mm/dd/yyyy).");

            menu.AnyKey();
        }

        static public void GenerateReports()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

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

            menu.AnyKey();
        }

        static public void Debugging()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Debugging Help Menu ~~~~\n");

            menu.AnyKey();
        }

        internal static void PharmacyEdit()
        {
            var menu = new CliMenu();
            menu.WriteProgramTitle();

            Console.WriteLine("\n     ~~~~ Edit Pharmacy Help Menu ~~~~\n");
            Console.WriteLine("    This menu allows you to edit a pharmacy.");
            Console.WriteLine("    Select a property to edit it! ");

            menu.AnyKey();
        }
    }
}
