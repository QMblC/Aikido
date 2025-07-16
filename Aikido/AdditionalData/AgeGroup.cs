using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum AgeGroup
    {
        [EnumMember(Value = "Взрослая")]
        Adult,
        [EnumMember(Value = "Детская")]
        Child
    }
}
