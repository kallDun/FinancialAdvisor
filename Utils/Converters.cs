using System.Globalization;

namespace FinancialAdvisorTelegramBot.Utils
{
    public static class Converters
    {
        public static decimal ToDecimal(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("String is null or empty");
            IFormatProvider en_format = new CultureInfo("en-US");
            if (!decimal.TryParse(str, en_format, out decimal result)) throw new ArgumentException("String is not a decimal");
            return result;
        }

        public static byte ToByte(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException("String is null or empty");
            IFormatProvider en_format = new CultureInfo("en-US");
            if (!byte.TryParse(str, en_format, out byte result)) throw new ArgumentException("String is not a byte");
            return result;
        }
    }
}
