using System.Runtime.Serialization;

namespace Aikido.AdditionalData.Enums
{
    public enum Sex
    {
        [EnumMember(Value = "Мужской")]
        Male,
        [EnumMember(Value = "Женский")]
        Female
    }
}
