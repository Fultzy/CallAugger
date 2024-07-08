using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities.CliInterface
{
    static class ErrorMenu
    {
        public static void For(string message1, string message2 = "", string message3 = "")
        {
            var menu = new CliMenu()
            {
                OptionPadding = 3,
                MenuName = "Ope!!! \n",
            };

            menu.WriteProgramTitle();
            menu.WriteMenuName();

            menu.WriteMessage(message1);
            if (message2 != "") menu.WriteMessage(message2);
            if (message3 != "") menu.WriteMessage(message3);

            menu.AnyKey();

            throw new Exception();
        }
    }
}
