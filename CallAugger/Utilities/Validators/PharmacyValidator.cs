using System.Text.RegularExpressions;


namespace CallAugger.Utilities
{

    internal class PharmacyValidator
    {
        public static readonly PharmacyValidator Instance = new PharmacyValidator();

        public string IsZip(string input)
        {
            string pattern = @"^\d{5}$";

            if (Regex.IsMatch(input, pattern))
                return "Input is valid.";
            else
                return "Input must contain exactly 5 digits.";
        }

        public string IsNpi(string input)
        {
            string pattern = @"^\d{10}$";

            if (Regex.IsMatch(input, pattern))
                return "Input is valid.";
            else
                return "Input must contain exactly 10 digits.";
        }

        public string IsNcpdp(string input)
        {
            string pattern = @"^\d{7}$";

            if (Regex.IsMatch(input, pattern))
                return "Input is valid.";
            else
                return "Input must contain exactly 7 digits.";
        }

        public string IsDea(string input)
        {
            string pattern = @"^[a-zA-Z\d]{9}$";

            if (Regex.IsMatch(input, pattern))
                return "Input is valid.";
            else
                return "Input must contain letters and digits only and must be 9 characters in length.";
        }
        
        public string IsPharmacyNameValid(string input)
        {
            string pattern = @"^[a-zA-Z\d\s]{1,50}$";

            if (Regex.IsMatch(input, pattern))
                return "Input is valid.";
            else
                return "Input must contain letters and digits only and must be between 1 and 50 characters in length.";
        }
    }
}
