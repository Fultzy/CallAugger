using System.Text.RegularExpressions;

static class PhoneNumberValidator
{
    public static bool IsPhoneNumber(string phoneNumber)
    {
        if (phoneNumber == null) return false;
        if (phoneNumber.Length > 10 || phoneNumber.Length < 10) return false;

        Regex regex = new Regex(@"^\d{10}$");

        if (regex.IsMatch(phoneNumber))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}