using System.Reflection;
using System.Runtime.Serialization;

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

    public static string? GetEnumMemberValue<T>(T enumValue) where T : Enum
    {
        var type = typeof(T);
        var name = enumValue.ToString();
        var field = type.GetField(name);
        if (field == null) return null;

        var attr = field.GetCustomAttribute<EnumMemberAttribute>();
        return attr?.Value;
    }

    public static T? GetEnumMemberValue<T>(string enumValue) where T : Enum
    {
        var type = typeof(T);
        if (enumValue == null || enumValue == "")
            enumValue = "None";

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attr?.Value == enumValue)
                return (T)field.GetValue(null)!;

            if (field.Name == enumValue)
                return (T)field.GetValue(null)!;
        }
        throw new NotImplementedException($"Не найден такой enum {enumValue}");
    }
}
