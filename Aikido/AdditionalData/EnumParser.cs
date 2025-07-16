using System.Reflection;
using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public static class EnumParser
    {
        public static T ConvertStringToEnum<T>(string value) where T : Enum
        {
            if (value == null || value == "")
            {
                value = "None";
            }

            if (Enum.TryParse(typeof(T), value, ignoreCase: true, out var result))
            {
                return (T)result!;
            }

            throw new Exception($"Данные о {typeof(T).Name} из строки некорректны. Входное значение: {value}");
        }

        public static string ConvertEnumToString<T>(T enumValue) where T : Enum
        {
            return enumValue.ToString();
        }

        public static Dictionary<string, string> GetEnumNames<T>() where T : Enum
        {
            return typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.GetCustomAttribute<EnumMemberAttribute>() != null)
                .ToDictionary(
                    f => f.Name,
                    f => f.GetCustomAttribute<EnumMemberAttribute>()!.Value!
                );
        }

    }
}
