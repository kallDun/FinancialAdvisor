namespace FinancialAdvisorTelegramBot.Utils.Attributes
{
    public class CustomServiceAttribute : Attribute
    {
        public LifeTimeServiceType LifeTimeType;

        public CustomServiceAttribute(LifeTimeServiceType lifeTimeType = LifeTimeServiceType.Scoped)
        {
            LifeTimeType = lifeTimeType;
        }
    }
}
