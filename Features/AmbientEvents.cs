using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;
using RichAmbiance.AmbientEvents;

namespace RichAmbiance.Features
{
    internal class AmbientEvents
    {
        internal static List<string> CommonEvents { get; private set; } = new List<string>();
        internal static List<string> UncommonEvents { get; private set; } = new List<string>();
        internal static List<string> RareEvents { get; private set; } = new List<string>();
        internal static AmbientEvent ActiveEvent { get; set; } = null;

        internal enum EventFrequency
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2
        }

        internal static void Main()
        {
            CommonEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == "common").Select(x => x.Key).ToList());
            UncommonEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == "uncommon").Select(x => x.Key).ToList());
            RareEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == "rare").Select(x => x.Key).ToList());
            Game.LogTrivial($"[RPE Ambient Event]: Common events: {CommonEvents.Count}, Uncommon events: {UncommonEvents.Count}, Rare events: {RareEvents.Count}");
            Game.LogTrivial($"[RPE Ambient Event]: Common Frequency: {Settings.CommonEventFrequency}, Uncommon Frequency: {Settings.UncommonEventFrequency}, Rare Frequency: {Settings.RareEventFrequency}");

            BeginLoopingForEvents();
        }

        private static void BeginLoopingForEvents()
        {
            Game.LogTrivial($"[RPE Ambient Event]: Pre-event loop initialized.");
            while (true)
            {
                GameFiber.Sleep(Settings.EventCooldownTimer); //20000 for testing or Settings.EventCooldownTimer for release
                if (PlayerIsBusy())
                {
                    Game.LogTrivial($"[RPE Ambient Event]: The player is busy, try again later.");
                    continue;
                }
                if (ActiveEvent != null)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: An event is already running.");
                    continue;
                }

                SelectEvent();
            }

            bool PlayerIsBusy()
            {
                if (Functions.IsCalloutRunning())
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy, callout running/being dispatched.");
                    return true;
                }
                else if (Functions.GetActivePursuit() != null)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy, pursuit active.");
                    return true;
                }
                else
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy state not recognized.");
                    return false;
                }
            }
        }

        private static void SelectEvent()
        {
            string newEvent = null;
            var randomValue = new Random().Next(1, 100); // 40 for testing
            Game.LogTrivial($"[RPE Ambient Event]: Choosing random event ({randomValue}).");

            if (randomValue <= Settings.CommonEventFrequency && CommonEvents.Count > 0)
            {
                newEvent = CommonEvents[new Random().Next(CommonEvents.Count)];
                if (string.IsNullOrEmpty(newEvent))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No common event found.");
                    return;
                }
            }
            else if (randomValue > Settings.CommonEventFrequency && randomValue <= Settings.CommonEventFrequency + Settings.UncommonEventFrequency && UncommonEvents.Count > 0)
            {
                newEvent = UncommonEvents[new Random().Next(UncommonEvents.Count)];
                if (string.IsNullOrEmpty(newEvent))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No uncommon event found.");
                    return;
                }
            }
            else if (randomValue > Settings.CommonEventFrequency + Settings.UncommonEventFrequency && RareEvents.Count > 0)
            {
                newEvent = RareEvents[new Random().Next(RareEvents.Count)];
                if (string.IsNullOrEmpty(newEvent))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No rare event found.");
                    return;
                }
            }

            EventType eventType = (EventType)Enum.Parse(typeof(EventType), newEvent);
            InitializeNewEvent(eventType);
        }

        internal static void InitializeNewEvent(EventType newEvent)
        {
            //Game.LogTrivial($"[RPE Ambient Event]: Starting {newEvent} event.");

            switch (newEvent)
            {
                case EventType.DrugDeal:
                    GameFiber.StartNew(() => new DrugDeal(), "RPE DrugDeal Event Fiber");
                    break;
                case EventType.DriveBy:
                    GameFiber.StartNew(() => new DriveBy(), "RPE DriveBy Event Fiber");
                    break;
                case EventType.CarJacking:
                    GameFiber.StartNew(() => new CarJacking(), "RPE CarJacking Event Fiber");
                    break;
                case EventType.Assault:
                    GameFiber.StartNew(() => new Assault(), "RPE Assault Event Fiber");
                    break;
                case EventType.Prostitution:
                    GameFiber.StartNew(() => new Prostitution(), "RPE Prostitution Event Fiber");
                    break;
                default:
                    Game.LogTrivial($"{newEvent} is not implemented yet.");
                    break;
            }

            if (ActiveEvent == null)
            {
                // start mini event: cell phone driver, speed, intoxicated, wreckless
            }
        }
    }
}
