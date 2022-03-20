using System;

namespace Units.Player
{
    [Serializable]
    public enum Archetype : byte
    {
        Base = 0,
        Runner,
        Plaqueur,
        Lanceur,
    }
}