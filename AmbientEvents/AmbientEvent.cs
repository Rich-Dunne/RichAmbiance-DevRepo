using System.Collections.Generic;
using Rage;
using System.Linq;

namespace RichAmbiance.AmbientEvents
{ 
    using RichAmbiance.Features;

    internal enum EventType
    {
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
        DistractedDriver = 21
    }

    internal enum State
    {
        Uninitialized = 0,
        Preparing = 1,
        Running = 2,
        Ending = 3
    }

    internal class AmbientEvent : IAmbientEvent
    {
        internal State State { get; private set; } = State.Uninitialized;
        internal EventType EventType { get; set; }
        internal List<EventPed> EventPeds { get; private set; } = new List<EventPed>();
        internal List<Blip> EventBlips { get; private set; } = new List<Blip>();

        internal AmbientEvent() { }

        public void Prepare() { }
        public void Process() { }

        internal void TransitionToState(State state)
        {
            if (State == state)
            {
                Game.LogTrivial($"State is already {state}");
                return;
            }

            State = state;

            switch (state)
            {
                case State.Preparing:
                    AmbientEvents.ActiveEvent = this;
                    Game.LogTrivial($"[Rich Ambiance]: Beginning {GetType().Name} event.");
                    break;
                case State.Running:
                    Game.LogTrivial($"[Rich Ambiance]: Preparation for {GetType().Name} event complete. Running");
                    break;
                case State.Ending:
                    Game.LogTrivial($"[Rich Ambiance]: Ending {GetType().Name} event.");
                    Cleanup();
                    break;
            }
        }

        internal void Cleanup(bool smoothEnding = true)
        {
            // Clean up EventBlips
            if (smoothEnding)
            {
                foreach (Blip blip in EventBlips.Where(b => b))
                {
                    while (blip && blip.Alpha > 0)
                    {
                        blip.Alpha -= 0.01f;
                        GameFiber.Yield();
                    }
                    if (blip)
                    {
                        blip.Delete();
                    }
                }
            }
            else
            {
                foreach (Blip blip in EventBlips.Where(b => b))
                {
                    blip.Delete();
                }
            }
            EventBlips.Clear();

            // Clean up EventPeds
            foreach (EventPed eventPed in EventPeds.Where(x => x))
            {
                foreach (Blip blip in eventPed.GetAttachedBlips().Where(b => b))
                {
                    blip.Delete();
                }
                eventPed.BlockPermanentEvents = false;
                eventPed.Dismiss();
            }
            EventPeds.Clear();

            AmbientEvents.ActiveEvent = null;
        }
    }
}
