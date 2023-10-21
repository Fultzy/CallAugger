using System.Linq;
using System.Text.RegularExpressions;

static class PhoneNumberValidator
{
    public static bool IsPhoneNumber(string phoneNumber)
    {
        Regex regex = new Regex(@"^\d{10}$");

        if (regex.IsMatch(phoneNumber) && phoneNumber.Length == 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
