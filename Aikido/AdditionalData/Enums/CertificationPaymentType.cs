using System.Runtime.Serialization;

namespace Aikido.AdditionalData.Enums
{
    public enum CertificationPaymentType
    {
        [EnumMember(Value = "Аттестация 5 Кю (детский)")]
        Certification5KyuChild,
        [EnumMember(Value = "Аттестация 4 Кю (детский)")]
        Certification4KyuChild,
        [EnumMember(Value = "Аттестация 3 Кю (детский)")]
        Certification3KyuChild,
        [EnumMember(Value = "Аттестация 2 Кю (детский)")]
        Certification2KyuChild,
        [EnumMember(Value = "Аттестация 1 Кю (детский)")]
        Certification1KyuChild,
        [EnumMember(Value = "Аттестация 5 Кю")]
        Certification5Kyu,
        [EnumMember(Value = "Аттестация 4 Кю")]
        Certification4Kyu,
        [EnumMember(Value = "Аттестация 3 Кю")]
        Certification3Kyu,
        [EnumMember(Value = "Аттестация 2 Кю")]
        Certification2Kyu,
        [EnumMember(Value = "Аттестация 1 Кю")]
        Certification1Kyu,
        [EnumMember(Value = "Аттестация 1 Дан")]
        Certification1Dan,
        [EnumMember(Value = "Аттестация 2 Дан")]
        Certification2Dan,
        [EnumMember(Value = "Аттестация 3 Дан")]
        Certification3Dan,
        [EnumMember(Value = "Аттестация 4 Дан")]
        Certification4Dan,
        [EnumMember(Value = "Аттестация 5 Дан")]
        Certification5Dan,
        [EnumMember(Value = "Аттестация 6 Дан")]
        Certification6Dan,
        [EnumMember(Value = "Аттестация 7 Дан")]
        Certification7Dan,
        [EnumMember(Value = "Аттестация 8 Дан")]
        Certification8Dan,
        [EnumMember(Value = "Аттестация 9 Дан")]
        Certification9Dan,
        [EnumMember(Value = "Аттестация 10 Дан")]
        Certification10Dan
    }
}
