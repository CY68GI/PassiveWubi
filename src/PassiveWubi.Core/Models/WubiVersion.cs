using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PassiveWubi.Core;

[JsonConverter(typeof(JsonStringEnumConverter<WubiVersion>))]
public enum WubiVersion
{
    [Description("86 版")]
    V86,

    [Description("98 版")]
    V98,

    [Description("新世纪版")]
    NewCentury
}
