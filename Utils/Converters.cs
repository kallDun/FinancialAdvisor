using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace FinancialAdvisorTelegramBot.Utils
{
    public static class Converters
    {        
        public static decimal ToDecimal(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("String is null or empty");
            IFormatProvider provider = new CultureInfo("en-US");
            if (!decimal.TryParse(str, provider, out decimal result)) throw new ArgumentException("String is not a decimal");
            return result;
        }

        public static byte ToByte(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("String is null or empty");
            IFormatProvider provider = new CultureInfo("en-US");
            if (!byte.TryParse(str, provider, out byte result)) throw new ArgumentException("String is not a byte");
            return result;
        }

        public static string DateTimeFormat => "dd.MM.yyyy HH:mm:ss";

        public static DateTime ToDateTime(string str)
        {
            IFormatProvider provider = new CultureInfo("en-US");
            if (!DateTime.TryParseExact(str, DateTimeFormat, provider, DateTimeStyles.AllowWhiteSpaces, out DateTime result))
                throw new ArgumentException("String is not a date");
            return result;
        }
    }
}
