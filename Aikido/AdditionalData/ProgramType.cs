using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum ProgramType
    { 
        [EnumMember(Value = "Взрослая")]
        Adult,
        [EnumMember(Value = "Детская")]
        Child
    }
}
