using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum SeminarMemberStatus
    {
        [EnumMember(Value = "Ожидает")]
        Waiting,
        [EnumMember(Value = "Пропуск")]
        Missed,
        [EnumMember(Value = "Не аттестован")]
        NotCertified,
        [EnumMember(Value = "Аттестован")]
        Certified
    }
}
