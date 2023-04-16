using FinancialAdvisorTelegramBot.Bot.Commands;
using System.Reflection;
using System.Text;

namespace FinancialAdvisorTelegramBot.Utils.CommandSerializing
{
    public static class CommandSerializer
    {
        public static string Serialize<T>(T command) where T : ICommand
        {
            Type type = command.GetType();
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder serialized = new();
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(typeof(CommandPropertySerializableAttribute), true);
                if (attributes.Length == 0) continue;
                object? value = property.GetValue(command);
                //if (value is null) continue;
                serialized.Append($"{property.Name}={value};");
            }
            return serialized.ToString();
        }

        public static void Deserialize<T>(string data, T command) where T : ICommand
        {
            Type type = command.GetType();
            var properties = type.GetProperties();
            string[] serializedProperties = data.Split(';');
            foreach (var serializedProperty in serializedProperties)
            {
                string[] serializedPropertyData = serializedProperty.Split('=');
                string propertyName = serializedPropertyData[0];
                string? propertyValue = serializedPropertyData.Length > 1 ? serializedPropertyData[1] : null;
                PropertyInfo? property = properties.FirstOrDefault(p => p.Name == propertyName);
                if (property is null) continue;
                object? value = Convert.ChangeType(propertyValue, property.PropertyType);
                property.SetValue(command, value);
            }
        }
    }
}
