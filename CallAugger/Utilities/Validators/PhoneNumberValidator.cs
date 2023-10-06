using System.Linq;
using System.Text.RegularExpressions;

internal class PhoneNumberValidator
{
    public string IsPhoneNumber(string phoneNumber)
    {
        Regex regex = new Regex(@"^\d{10}$");
        if (regex.IsMatch(phoneNumber))
        {
            return $"{nameof(PhoneNumberValidator)} is valid";
        }
        else
        {
            return "Invalid phone number format";
        }
    }
}
