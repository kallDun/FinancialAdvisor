using FinancialAdvisorTelegramBot.Bot.Commands;
using Newtonsoft.Json;
using System.Reflection;

namespace FinancialAdvisorTelegramBot.Utils.CommandSerializing
{
    public static class CommandDataSerializer
    {
        public static string Serialize<T>(T command) where T : ICommand
        {
            Dictionary<string, string> serialized = new();

            Type type = command.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(typeof(CommandPropertySerializableAttribute), true);
                if (attributes.Length == 0) continue;
                object? value = property.GetValue(command);
                serialized.Add(property.Name, value?.ToString() ?? "");
            }
            return JsonConvert.SerializeObject(serialized);
        }
        
        public static void Deserialize<T>(string data, T command) where T : ICommand
        {
            Dictionary<string, string?> serialized = JsonConvert.DeserializeObject<Dictionary<string, string?>>(data) ?? new();
            
            Type type = command.GetType();
            var properties = type.GetProperties();
            foreach (var serializedProperty in serialized)
            {
                PropertyInfo? property = properties.FirstOrDefault(p => p.Name == serializedProperty.Key);
                if (property is null) continue;
                object? value = Convert.ChangeType(serializedProperty.Value, property.PropertyType);
                property.SetValue(command, value);
            }
        }
    }
}
