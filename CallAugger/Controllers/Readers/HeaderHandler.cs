using CallAugger.Utilities;
using CallAugger.Utilities.Validators;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger.Readers
{
    public static class HeaderHandler
    {
        public static Dictionary<string, int> GetHeaderFromWorksheet(Worksheet sheet)
        {
            Dictionary<string, int> headers = new Dictionary<string, int>();

            try
            {
                int MaxColumns = sheet.UsedRange.Columns.Count;
                for (int col = 1; col <= MaxColumns; col++)
                {
                    headers.Add(sheet.Cells[1, col].Value, col);
                }

                return headers;
            }
            catch (Exception e)
            {
                var message = $"Error in HeaderHandler.GetHeaderFromWorksheet(Worksheet sheet) ~ Could Not Get Headers {sheet.Name}. {e.Message} \n";

                throw new Exception(Logger.Error(Logger.Importing(message)));
            }
        }

        public static Dictionary<string, int> GetHeaderFromList(List<string> headerRow)
        {
            Dictionary<string, int> headers = new Dictionary<string, int>();

            try
            {
                for (int col = 0; col < headerRow.Count; col++)
                {
                    headers.Add(headerRow[col], col);
                }

                return headers;
            }
            catch (Exception e)
            {
                var headerRowString = string.Join(", ", headerRow);
                var message = $"Error in HeaderHandler.GetHeaderFromList(List<string> headerRow) ~ Could Not Get Headers. Row: {headerRowString}\n{e.Message}\n";

                throw new Exception(Logger.Error(Logger.Importing(message)));
            }
        }

        internal static void PrintHeader(Dictionary<string, int> header)
        {
            foreach (var item in header)
            {
                Console.WriteLine($"{item.Value} : {item.Key}");
            }
        }


        // Data Manipulation Via Headers
        /// <summary>
        /// Sorts the data in the oldData list based on the newHeader dictionary.
        /// </summary>
        /// <param name="oldData">The original data to be sorted.</param>
        /// <param name="newHeader">The dictionary containing the new header.</param>
        /// <returns>A list of lists representing the sorted data.</returns>
        public static List<List<string>> SortDataToHeader(List<List<string>> oldData, Dictionary<string, int> newHeader)
        {
            var oldheader = GetHeaderFromList(oldData[0]);
            var sortedData = new List<List<string>>();

            // remove the header row
            oldData.RemoveAt(0);

            foreach (var row in oldData)
            {
                var newRow = new List<string>();

                foreach (var item in newHeader)
                {
                    if (oldheader.ContainsKey(item.Key))
                    {
                        newRow.Add(row[oldheader[item.Key]]);
                    }
                }

                sortedData.Add(newRow);
            }

            return sortedData;
        }

        public static List<string> HeaderToList(Dictionary<string, int> header)
        {
            var list = new List<string>();

            foreach (var item in header)
            {
                list.Add(item.Key);
            }

            return list;
        }
    }
}
