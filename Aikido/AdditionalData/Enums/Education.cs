using System.Runtime.Serialization;

namespace Aikido.AdditionalData.Enums
{
    public enum Education
    {
        [EnumMember(Value = "Неизвестно")]
        None,
        [EnumMember(Value = "1 класс")]
        SchoolClass1,
        [EnumMember(Value = "2 класс")]
        SchoolClass2,
        [EnumMember(Value = "3 класс")]
        SchoolClass3,
        [EnumMember(Value = "4 класс")]
        SchoolClass4,
        [EnumMember(Value = "5 класс")]
        SchoolClass5,
        [EnumMember(Value = "6 класс")]
        SchoolClass6,
        [EnumMember(Value = "7 класс")]
        SchoolClass7,
        [EnumMember(Value = "8 класс")]
        SchoolClass8,
        [EnumMember(Value = "9 класс")]
        SchoolClass9,
        [EnumMember(Value = "10 класс")]
        SchoolClass10,
        [EnumMember(Value = "11 класс")]
        SchoolClass11,
        [EnumMember(Value = "Колледж")]
        College,
        [EnumMember(Value = "Вуз")]
        University,
    }
}
