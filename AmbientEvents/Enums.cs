namespace RichAmbiance.AmbientEvents
{
    internal enum EventType
    {
        None = 0,
        DrugDeal = 1, // Implemented
        DriveBy = 2, // Implemented
        CarJacking = 3, // Implemented
        Assault = 4, // Implemented
        RoadRage = 5,
        PublicIntoxication = 6,
        DUI = 7,
        Prostitution = 8, // Implemented
        Protest = 9,
        SuspiciousCircumstances = 10,
        CriminalMischief = 11,
        OfficerAmbush = 12,
        CitizenAssist = 13,
        MentalHealth = 14,
        TrafficStopAssist = 15,
        OpenCarry = 16,
        CarVsAnimal = 17,
        NoVehicleLights = 18, // Implemented
        BrokenLight = 19, // Implemented
        BrokenWindshield = 20, // Implemented
        RecklessDriver = 21, // Implemented
        Speeding = 22, // Implemented
        BOLO = 23, // Implemented
        Fire = 24,
        ResistingArrest = 25
    }

    internal enum State
    {
        Uninitialized = 0,
        Preparing = 1,
        Running = 2,
        Ending = 3
    }

    internal enum EventFrequency
    {
        On = -1,
        Off = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 3
    }
}
