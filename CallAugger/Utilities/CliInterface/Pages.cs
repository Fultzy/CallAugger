using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities.CliInterface
{
    internal class Pages
    {
        /////////////////////////////////////////
        // This class is used to display large sets of data in a paginated format
        // it takes in a list of objects and a page size and then displays the data
        // in a paginated format. The user can then navigate through the pages using
        // the arrow keys and the enter key. The user can also search through the data
        // using the search bar at the top of the page. The search bar will filter the
        // data and display only the matching results. The user can then select a result

        // this class will use both the CliMenu and the SearchUtility classes

        private int pageSize { get; set; }      // number of items per page
        private int totalPages { get; set; }    // the total number of pages
        private int totalItems { get; set; }    // the total number of items in the list

        private List<Object> Items;

        private int currentPage = 1;     // the current page the user is on
        private string searchTerm = "";  // the current search term
        private bool searching = false;  // is the user currently searching

        public Pages()
        {

        }
    }
}
