using System.Runtime.Serialization;

namespace Aikido.AdditionalData.Enums
{
    public enum Role
    {
        [EnumMember(Value = "Ученик")]
        User,
        [EnumMember(Value = "Тренер")]
        Coach,
        [EnumMember(Value = "Руководитель")]
        Manager,
        [EnumMember(Value = "Администратор")]
        Admin
    }
}
