using System.Collections.Generic;
using Rage;
using System.Linq;

namespace RichAmbiance.AmbientEvents
{ 
    using RichAmbiance.Features;

    internal class AmbientEvent
    {
        internal State State { get; private set; } = State.Uninitialized;
        internal EventType EventType { get; set; }
        internal List<EventPed> EventPeds { get; private set; } = new List<EventPed>();
        internal List<Blip> EventBlips { get; private set; } = new List<Blip>();

        internal AmbientEvent() { }

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
