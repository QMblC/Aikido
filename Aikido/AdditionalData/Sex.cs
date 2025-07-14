using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum Sex
    {
        [EnumMember(Value = "Мужской")]
        Male,
        [EnumMember(Value = "Женский")]
        Female
    }
}
