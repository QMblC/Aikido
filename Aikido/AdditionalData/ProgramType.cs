﻿using System.Runtime.Serialization;

namespace Aikido.AdditionalData
{
    public enum ProgramType
    {
        [EnumMember(Value = "Не определено")]
        None,
        [EnumMember(Value = "Взрослая")]
        Adult,
        [EnumMember(Value = "Детская")]
        Child
    }
}
