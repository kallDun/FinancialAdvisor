namespace FinancialAdvisorTelegramBot.Utils.Attributes
{
    public class CustomBackgroundServiceAttribute : CustomServiceAttribute
    {
        public CustomBackgroundServiceAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0) : base(LifeTimeServiceType.Scoped)
        {
            Delay = new TimeSpan(days, hours, minutes, seconds);
        }

        public TimeSpan Delay { get; private set; }
    }
}
