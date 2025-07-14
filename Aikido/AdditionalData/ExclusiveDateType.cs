using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum ExclusiveDateType
    {
        [EnumMember(Value = "Extra")]
        Extra,
        [EnumMember(Value = "Minor")]
        Minor
    }
}
