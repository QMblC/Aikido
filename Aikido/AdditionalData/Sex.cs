using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum Sex
    {
        [EnumMember(Value = "Male")]
        Male,
        [EnumMember(Value = "Female")]
        Female
    }
}
