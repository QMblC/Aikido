using System.Reflection;
using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public static class EnumParser
    {
        public static T ConvertStringToEnum<T>(string value) where T : Enum
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute != null && attribute.Value == value)
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new Exception($"Данные о {typeof(T).Name} из json некорректны. На вход подаётся - {value}");
        }

        public static string ConvertEnumToString<T>(T enumValue) where T : Enum
        {
            var memberInfo = typeof(T).GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attribute = memberInfo[0].GetCustomAttribute<EnumMemberAttribute>();
                if(attribute != null)
                {
                    return attribute.Value;
                }
            }
            throw new NotImplementedException($"{typeof(T).Name} не содержит аттрибута");
        }
    }
}
