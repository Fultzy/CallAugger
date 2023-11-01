using CallAugger.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace CallAugger.Utilities
{
    public class CliMenu
    {
        public string MenuName { get; set; }
        public string Prompt { get; set; } = "Select An Option: ";
        public string Header { get; set; }
        public string Help { get; set; }
        public string Exit { get; set; } = "Exit";
        public List<string> Options { get; set; } = new List<string>();
        public int OptionPadding { get; set; } = 1;
        public bool Searching { get; set; } = false;

        // Write Methods
        public string WriteMenu()
        {
            WriteTitle();
            WriteOptions();
            WriteExit();
            WritePrompt();

            string input = Console.ReadLine();

            if (Exit == "Close Program" && input.ToLower() == "exit")
                Environment.Exit(0);

            return input;
        }
        
        public void WriteProgramTitle()
        {
            Console.Clear();

            Console.WriteLine( @" ______   ______   __       __           ______   __  __   ______   ______   ______   ______    " );
            Console.WriteLine( @"/\  ___\ /\  __ \ /\ \     /\ \         /\  __ \ /\ \/\ \ /\  ___\ /\  ___\ /\  ___\ /\  == \   " );
            Console.WriteLine( @"\ \ \____\ \  __ \\ \ \____\ \ \____    \ \  __ \\ \ \_\ \\ \ \__ \\ \ \__ \\ \  __\ \ \  __<   " );
            Console.WriteLine( @" \ \_____\\ \_\ \_\\ \_____\\ \_____\    \ \_\ \_\\ \_____\\ \_____\\ \_____\\ \_____\\ \_\ \_\ " );
            Console.WriteLine( @"  \/_____/ \/_/\/_/ \/_____/ \/_____/     \/_/\/_/ \/_____/ \/_____/ \/_____/ \/_____/ \/_/ /_/ " );
            Console.WriteLine( "                                                      Built for BestRx     By Fultzy     V 1.0 " );
            DateRange dateRange = new DateRange();
            dateRange.Write();
        }
        
        public void WriteTitle()
        {
            if (MenuName != null)
            {
                Console.WriteLine("\n    " + MenuName + "");
            }
        }

        public void WriteOptions()
        {
            if (Options.Count > 0 && Options[0] != null)
            {
                string headerMessage = Searching == false ?  "" : Header;
                
                if (headerMessage.Length > 0 ) Console.WriteLine(OPadding() + Header);

                for (int i = 0; i < Options.Count; i++)
                {
                    string padding = i > 8 ? OPadding(1) : OPadding();


                    if (Options[i] != null)
                        Console.WriteLine(padding + (i + 1) + ": " + Options[i]);
                }
            } 
            else
            {
                string message = Searching == false ? "No Options Available": "No Search Results Found";
                Console.WriteLine(OPadding() + message);
            }
        }

        public void WritePrompt()
        {
            Console.Write(" " + Prompt);
        }

        public void WriteExit()
        {
            Console.Write("\n");
            if (Help != null)
            {
                Console.WriteLine(" help: " + Help);
            }

            Console.WriteLine(" exit: " + Exit);
            Console.Write("\n");

        }

        public void ShowSelected(int optionNumber)
        {
            Console.WriteLine(OPadding() + Options[optionNumber - 1]);
        }
        
        public bool ConfirmationPrompt(string prompt)
        {
            Console.Write(prompt + " ");

            string input = Console.ReadLine();

            if (input.ToLower() == "y" || input.ToLower() == "yes")
                return true;
            else if (input.ToLower() == "n" || input.ToLower() == "no")
                return false;
            else
                return ConfirmationPrompt(prompt);
        }
        

        // Formatting Helper Methods
        private string OPadding(int additional = 0)
        {
            string padding = " ";
            // return a number of whitespaces to create padding for the options
            for (int i = additional; i < OptionPadding; i++)
            {
                padding += " ";
            }

            return padding;
        }


        // input Case and validation methods
        public int CaseOptionNumber(string input)
        {
            int optionNumber;
            if (!int.TryParse(input, out optionNumber) || optionNumber < 1 || optionNumber > Options.Count)
                return 0;

            return optionNumber;
        }

        public bool IsValidOption(string input)
        {
            return CaseOptionNumber(input) > 0;
        }


        // Get Methods
        public int GetOption()
        {
            string input = Console.ReadLine();
            int optionNumber = CaseOptionNumber(input);

            if (optionNumber == 0)
            {
                Console.WriteLine(" Invalid Option");
                Console.Write("\n " + Prompt);
                return GetOption();
            }
            else
            {
                return optionNumber;
            }
        }

        public int GetOption(string input)
        {
            int optionNumber = CaseOptionNumber(input);

            if (optionNumber == 0)
            {
                Console.WriteLine(" Invalid Option");
                Console.Write("\n " + Prompt);
                return GetOption();
            }
            else
            {
                return optionNumber;
            }
        }
    }
}
