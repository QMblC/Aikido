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
        [EnumMember(Value = "Аттестация с 5 по 2 Кю")]
        Certification5to2Kyu,
        [EnumMember(Value = "Аттестация на 1 Кю")]
        Certification1Kyu,
        [EnumMember(Value = "Аттестация на Дан")]
        CertificationDan
    }
}
