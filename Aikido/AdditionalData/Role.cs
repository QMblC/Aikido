using System.Runtime.Serialization;

namespace Aikido.AdditionalData
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
