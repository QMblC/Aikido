using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum PaymentType
    {
        [EnumMember(Value = "Неопределено")]
        None,
        [EnumMember(Value = "Будо-Пасспорт")]
        BudoPassport,
        [EnumMember(Value = "Ежегодный взнос")]
        AnnualFee,
        [EnumMember(Value = "Семинар")]
        Seminar,
        [EnumMember(Value = "Аттестация")]
        Certification,

    }
}
