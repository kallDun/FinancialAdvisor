using System.Runtime.CompilerServices;

namespace FinancialAdvisorTelegramBot.Utils
{
    public static class Validators
    {
        public static void ValidateEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    throw new ArgumentException("Invalid email address");
                }
            }
            catch
            {
                throw new ArgumentException("Invalid email address");
            }
            if (email.Length > 50) throw new ArgumentException("Email is too long!");
        }

        public static void ValidateName(string name, bool isLong = false)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 3) throw new ArgumentException("Name must contain at least 3 characters!");
            if (name.Length > (isLong ? 100 : 20)) throw new ArgumentException("Name is too long!");
            if (name.Contains('/'))
            {
                throw new ArgumentException("Name can't contain '/'");
            }
            if (name.Contains('!'))
            {
                throw new ArgumentException("Name can't contain '!'");
            }
        }
    }
}
