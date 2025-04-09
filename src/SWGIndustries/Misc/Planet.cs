using JetBrains.Annotations;

namespace SWGIndustries;

[PublicAPI]
[Flags]
public enum Planet
{
    Undefined   = 0,
    Corellia    = 0x0001,
    Dantooine   = 0x0002,
    Dathomir    = 0x0004,
    Endor       = 0x0008,
    Kashyyyk    = 0x0010,
    Lok         = 0x0020,
    Mustafar    = 0x0040,
    Naboo       = 0x0080,
    Rori        = 0x0100,
    Talus       = 0x0200,
    Tatooine    = 0x0400,
    Yavin4      = 0x0800
}