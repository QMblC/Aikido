using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum Grade
    {
        [EnumMember(Value = "Нет")]
        None,
        [EnumMember(Value = "5 Кю (детский)")]
        Kyu5Child,
        [EnumMember(Value = "4 Кю (детский)")]
        Kyu4Child,
        [EnumMember(Value = "3 Кю (детский)")]
        Kyu3Child,
        [EnumMember(Value = "2 Кю (детский)")]
        Kyu2Child,
        [EnumMember(Value = "1 Кю (детский)")]
        Kyu1Child,
        [EnumMember(Value = "5 Кю")]
        Kyu5,
        [EnumMember(Value = "4 Кю")]
        Kyu4,
        [EnumMember(Value = "3 Кю")]
        Kyu3,
        [EnumMember(Value = "2 Кю")]
        Kyu2,
        [EnumMember(Value = "1 Кю")]
        Kyu1,
        [EnumMember(Value = "1 Дан")]
        Dan1,
        [EnumMember(Value = "2 Дан")]
        Dan2,
        [EnumMember(Value = "3 Дан")]
        Dan3,
        [EnumMember(Value = "4 Дан")]
        Dan4,
        [EnumMember(Value = "5 Дан")]
        Dan5,
        [EnumMember(Value = "6 Дан")]
        Dan6,
        [EnumMember(Value = "7 Дан")]
        Dan7,
        [EnumMember(Value = "8 Дан")]
        Dan8,
        [EnumMember(Value = "9 Дан")]
        Dan9,
        [EnumMember(Value = "10 Дан")]
        Dan10,
    }
}
