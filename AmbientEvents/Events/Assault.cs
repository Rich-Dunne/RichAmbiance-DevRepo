using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using RichAmbiance.Utils;

namespace RichAmbiance.AmbientEvents.Events
{
    internal class Assault : AmbientEvent
    {
        private List<Ped> _usablePeds;
        private EventPed _suspect, _victim;
        private float _oldDistance;

        internal Assault()
        {
            Prepare();
            if (State == State.Ending)
            {
                return;
            }

            Process();
            if (State == State.Ending)
            {
                return;
            }
        }

        private void Prepare()
        {
            TransitionToState(State.Preparing);
            FindEventPeds();
        }

        private void FindEventPeds()
        {
            for (int i = 0; i < 100; i++)
            {
                GameFiber.Sleep(500);
                if (GuardClauses.CalloutOrPursuitActive())
                {
                    TransitionToState(State.Ending);
                    return;
                }

                if (GuardClauses.EventPedsFound(EventPeds, 2, i))
                {
                    return;
                }

                GetUsablePeds();
                SelectPedPair();
            }

            Game.LogTrivial($"[Rich Ambiance]: Unable to find suitable event peds after 100 attempts.  Ending event.");
            TransitionToState(State.Ending);
            return;
        }

        private void GetUsablePeds() => _usablePeds = HelperMethods.GetReleventPedsForAmbientEvent().Where(p => p.IsOnFoot).ToList();

        private void SelectPedPair()
        {
            foreach (Ped ped in _usablePeds)
            {
                var victim = _usablePeds.FirstOrDefault(p => p != ped && Math.Abs(ped.Position.Z - p.Position.Z) <= 3f && p.DistanceTo2D(ped) <= 10f);
                if (victim)
                {
                    _suspect = new EventPed(ped, Role.PrimarySuspect, this, true);
                    _victim = new EventPed(victim, Role.Victim, this, false);
                    return;
                }
            }
        }

        private void Process()
        {
            TransitionToState(State.Running);
            GameFiber.StartNew(() => CheckEndConditions(), "RPE End Conditions Fiber");

            _suspect.Tasks.FightAgainst(_victim);
            if (new Random().Next(1) == 1 || _victim.Inventory.Weapons.Count > 0)
            {
                Game.LogTrivial($"[Rich Ambiance]: Victim is fighting suspect.");
                _victim.Tasks.FightAgainst(_suspect);
            }
            else
            {
                _victim.Tasks.Wander();
                GameFiber.SleepUntil(() => _victim.HasBeenDamagedBy(_suspect), 10000);
                if (State == State.Ending)
                {
                    return;
                }
                Game.LogTrivial($"[Rich Ambiance]: Victim is fleeing.");
                _victim.SetMaxSpeed(4f);
                _victim.Tasks.ReactAndFlee(_suspect);
            }
        }

        private void CheckEndConditions()
        {
            _oldDistance = Game.LocalPlayer.Character.DistanceTo2D(_suspect);

            while (State == State.Running)
            {
                GameFiber.Yield();
                if (_suspect == null || _victim == null || !_suspect || !_victim || !_suspect.IsAlive || !_victim.IsAlive || Functions.IsPedGettingArrested(_suspect))
                {
                    Game.LogTrivial($"[Rich Ambiance]: Suspect or victim is null or dead, or driver is arrested.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(_suspect) > 150f)
                {
                    Game.LogTrivial($"[Rich Ambiance]: Player is too far away.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (_suspect.DistanceTo2D(_victim) > 50f)
                {
                    Game.LogTrivial($"[Rich Ambiance]: Victim got away from suspect.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Functions.GetActivePursuit() != null && Functions.IsPedInPursuit(_suspect))
                {
                    Game.LogTrivial($"[Rich Ambiance]: Player is in pursuit of suspect.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                FadeBlips();
            }
        }

        private void FadeBlips()
        {
            if (Settings.EventBlips && _suspect.Blip)
            {
                if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(_suspect) - _oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(_suspect) > _oldDistance && _suspect.Blip.Alpha > 0f)
                {
                    _suspect.Blip.Alpha -= 0.001f;
                }
                else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(_suspect) - _oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(_suspect) < _oldDistance && _suspect.Blip.Alpha < 1.0f)
                {
                    _suspect.Blip.Alpha += 0.01f;
                }
                _oldDistance = Game.LocalPlayer.Character.DistanceTo2D(_suspect);
            }
        }
    }
}
