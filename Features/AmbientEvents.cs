using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;
using RichAmbiance.AmbientEvents;
using RichAmbiance.AmbientEvents.Events;

namespace RichAmbiance.Features
{
    internal class AmbientEvents
    {
        internal static List<EventType> CommonEvents { get; private set; } = new List<EventType>();
        internal static List<EventType> UncommonEvents { get; private set; } = new List<EventType>();
        internal static List<EventType> RareEvents { get; private set; } = new List<EventType>();
        internal static List<EventType> MinorEvents { get; private set; } = new List<EventType>()
        {
            EventType.BrokenLight,
            EventType.BrokenWindshield,
            EventType.NoVehicleLights,
            EventType.RecklessDriver,
            EventType.Speeding
        };
        internal static AmbientEvent ActiveEvent { get; set; } = null;

        private const int MINOR_AMBIENT_EVENT_MINIMUM_DELAY = 60000;
        private const int MINOR_AMBIENT_EVENT_MAXIMUM_DELAY = MINOR_AMBIENT_EVENT_MINIMUM_DELAY * 3;

        internal static void Main()
        {
            CommonEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == EventFrequency.Common).Select(x => x.Key).ToList());
            UncommonEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == EventFrequency.Uncommon).Select(x => x.Key).ToList());
            RareEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == EventFrequency.Rare).Select(x => x.Key).ToList());
            Game.LogTrivial($"[Rich Ambiance]: Common events: {CommonEvents.Count}, Uncommon events: {UncommonEvents.Count}, Rare events: {RareEvents.Count}");
            Game.LogTrivial($"[Rich Ambiance]: Common Frequency: {Settings.CommonEventFrequency}, Uncommon Frequency: {Settings.UncommonEventFrequency}, Rare Frequency: {Settings.RareEventFrequency}");

            GameFiber.StartNew(() => BeginLoopingForMinorEvents(), "RichAmbiance Minor Event Loop Fiber");
            GameFiber.StartNew(() => BeginLoopingForEvents(), "RichAmbiance Major Event Loop Fiber");
        }

        private static void BeginLoopingForEvents()
        {
            Game.LogTrivial($"[Rich Ambiance]: Ambient event loop initialized.");
            while (true)
            {
                GameFiber.Sleep(Settings.EventCooldownTimer); //20000 for testing or Settings.EventCooldownTimer for release
                if (Settings.DisableEventsWhilePlayerIsBusy && PlayerIsBusy())
                {
                    Game.LogTrivial($"[Rich Ambiance]: The player is busy, try again later.");
                    continue;
                }
                if (ActiveEvent != null)
                {
                    Game.LogTrivial($"[Rich Ambiance]: An event is already running.");
                    continue;
                }

                SelectEvent();
            }
        }

        private static void BeginLoopingForMinorEvents()
        {
            Game.LogTrivial($"[Rich Ambiance]: Minor ambient event loop initialized.");
            while (true)
            {
                GameFiber.Sleep(new Random().Next(MINOR_AMBIENT_EVENT_MINIMUM_DELAY, MINOR_AMBIENT_EVENT_MAXIMUM_DELAY));
                TryStartMinorEvent();
            }
        }

        private static bool PlayerIsBusy()
        {
            if (Functions.IsCalloutRunning())
            {
                Game.LogTrivial($"[Rich Ambiance]: Player busy, callout running/being dispatched.");
                return true;
            }
            else if (Functions.GetActivePursuit() != null)
            {
                Game.LogTrivial($"[Rich Ambiance]: Player busy, pursuit active.");
                return true;
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance]: Player busy state not recognized.");
                return false;
            }
        }

        private static void SelectEvent()
        {
            EventType newEvent = EventType.None;
            var randomValue = new Random().Next(1, 100); // 40 for testing
            Game.LogTrivial($"[Rich Ambiance]: Choosing random event ({randomValue}).");

            if (randomValue <= Settings.CommonEventFrequency && CommonEvents.Count > 0)
            {
                newEvent = CommonEvents[new Random().Next(CommonEvents.Count)];
                //if (string.IsNullOrEmpty(newEvent))
                //{
                //    Game.LogTrivial($"[Rich Ambiance]: No common event found.");
                //    return;
                //}
            }
            else if (randomValue > Settings.CommonEventFrequency && randomValue <= Settings.CommonEventFrequency + Settings.UncommonEventFrequency && UncommonEvents.Count > 0)
            {
                newEvent = UncommonEvents[new Random().Next(UncommonEvents.Count)];
                //if (string.IsNullOrEmpty(newEvent))
                //{
                //    Game.LogTrivial($"[Rich Ambiance]: No uncommon event found.");
                //    return;
                //}
            }
            else if (randomValue > Settings.CommonEventFrequency + Settings.UncommonEventFrequency && RareEvents.Count > 0)
            {
                newEvent = RareEvents[new Random().Next(RareEvents.Count)];
                //if (string.IsNullOrEmpty(newEvent))
                //{
                //    Game.LogTrivial($"[Rich Ambiance]: No rare event found.");
                //    return;
                //}
            }

            //EventType eventType = (EventType)Enum.Parse(typeof(EventType), newEvent);
            InitializeNewEvent(newEvent);
        }

        internal static void InitializeNewEvent(EventType newEvent)
        {
            switch (newEvent)
            {
                case EventType.DrugDeal:
                    GameFiber.StartNew(() => new DrugDeal(), "Rich Ambiance DrugDeal Event Fiber");
                    break;
                case EventType.DriveBy:
                    GameFiber.StartNew(() => new DriveBy(), "Rich Ambiance DriveBy Event Fiber");
                    break;
                case EventType.CarJacking:
                    GameFiber.StartNew(() => new CarJacking(), "Rich Ambiance CarJacking Event Fiber");
                    break;
                case EventType.Assault:
                    GameFiber.StartNew(() => new Assault(), "Rich Ambiance Assault Event Fiber");
                    break;
                case EventType.Prostitution:
                    GameFiber.StartNew(() => new Prostitution(), "Rich Ambiance Prostitution Event Fiber");
                    break;
                default:
                    Game.LogTrivial($"{newEvent} is not implemented yet.");
                    break;
            }
        }

        private static void TryStartMinorEvent()
        {
            switch(MathHelper.Choose(MinorEvents).First())
            {
                case EventType.BrokenLight:
                    GameFiber.StartNew(() => new BrokenLight(), "Rich Ambiance BrokenLight Event Fiber");
                    break;
                case EventType.BrokenWindshield:
                    GameFiber.StartNew(() => new BrokenWindshield(), "Rich Ambiance BrokenWindshield Event Fiber");
                    break;
                case EventType.NoVehicleLights:
                    GameFiber.StartNew(() => new NoVehicleLights(), "Rich Ambiance NoVehicleLights Event Fiber");
                    break;
                case EventType.RecklessDriver:
                    GameFiber.StartNew(() => new RecklessDriver(), "Rich Ambiance RecklessDriver Event Fiber");
                    break;
                case EventType.Speeding:
                    GameFiber.StartNew(() => new Speeding(), "Rich Ambiance Speeding Event Fiber");
                    break;
            }
        }
    }
}
