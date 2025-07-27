using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum SeminarMemberStatus
    {
        [EnumMember(Value = "Нет")]
        None,
        [EnumMember(Value = ("Тренировка"))]
        Training,
        [EnumMember(Value = "Не аттестован")]
        NotCertified,
        [EnumMember(Value = "Аттестован")]
        Certified
    }
}
