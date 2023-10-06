using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace CallAugger.Utilities
{
    public class CliMenu
    {
        public string MenuName { get; set; }
        public string Prompt { get; set; } = "Select An Option: ";
        public string Exit { get; set; } = "Exit";
        public List<string> Options { get; set; } = new List<string>();
        public int OptionPadding { get; set; } = 1;
        public bool Searching { get; set; } = false;

        public string WriteMenu()
        {
            Console.Clear();

            WriteTitle();
            WriteOptions();
            WriteExit();
            WritePrompt();

            string input = Console.ReadLine();

            if (Exit == "Close Program" && input.ToLower() == "exit")
                Environment.Exit(0);

            return input;
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

        public void WriteProgramTitle()
        {
            Console.WriteLine( @" ______   ______   __       __           ______   __  __   ______   ______   ______   ______    " );
            Console.WriteLine( @"/\  ___\ /\  __ \ /\ \     /\ \         /\  __ \ /\ \/\ \ /\  ___\ /\  ___\ /\  ___\ /\  == \   " );
            Console.WriteLine( @"\ \ \____\ \  __ \\ \ \____\ \ \____    \ \  __ \\ \ \_\ \\ \ \__ \\ \ \__ \\ \  __\ \ \  __<   " );
            Console.WriteLine( @" \ \_____\\ \_\ \_\\ \_____\\ \_____\    \ \_\ \_\\ \_____\\ \_____\\ \_____\\ \_____\\ \_\ \_\ " );
            Console.WriteLine( @"  \/_____/ \/_/\/_/ \/_____/ \/_____/     \/_/\/_/ \/_____/ \/_____/ \/_____/ \/_____/ \/_/ /_/ " );
            Console.WriteLine( "                                                      Built for BestRx     By Fultzy     V 0.4 \n" );

        }
        
        public void WriteTitle()
        {
            if (MenuName != null)
            {
                Console.WriteLine("    " + MenuName + "\n");
            }
            else
            {
                WriteProgramTitle();
            }
        }

        public void WriteOptions()
        {
            if (Options[0] != null)
            {
                string headerMessage = Searching == false ?  "" : "Results: ";
                Console.WriteLine(OPadding() + headerMessage);

                for (int i = 0; i < Options.Count; i++)
                {
                    if (Options[i] != null)
                        Console.WriteLine(OPadding() + (i + 1) + " : " + Options[i]);
                }
            } 
            else
            {
                string message = Searching == false ? "No Options Available": "No Search Results Found";
                Console.WriteLine(OPadding() + message);
            }
        }

        private string OPadding()
        {
            string padding = " ";
            // return a number of whitespaces to create padding for the options
            for (int i = 0; i < OptionPadding; i++)
            {
                padding += " ";
            }

            return padding;
        }

        public void WritePrompt()
        {
            Console.Write(" " + Prompt);
        }

        public void WriteExit()
        {
            Console.WriteLine("\n Exit : " + Exit);
        }

        public void ShowSelected(int optionNumber)
        {
            Console.WriteLine(OPadding() + Options[optionNumber - 1]);
        }

        public void UpdateMenu(int optionNumber, string optionName)
        {
            Options[optionNumber - 1] = optionName;

            Console.Clear();
            WriteMenu();
        }

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
