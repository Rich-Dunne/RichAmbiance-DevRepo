using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using RichAmbiance.Utils;

namespace RichAmbiance.AmbientEvents.Events
{
    internal class CarJacking : AmbientEvent
    {
        private List<Ped> _usablePeds;
        private EventPed _suspect, _victim;
        private float _oldDistance;

        internal CarJacking()
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

        private void GetUsablePeds() => _usablePeds = HelperMethods.GetReleventPedsForAmbientEvent();

        private void SelectPedPair()
        {
            foreach (Ped ped in _usablePeds.Where(p => p.IsOnFoot))
            {
                if (ped.RelationshipGroup == RelationshipGroup.Fireman || ped.RelationshipGroup == RelationshipGroup.Medic || ped.RelationshipGroup.Name == "UBCOP")
                {
                    continue;
                }

                var victim = _usablePeds.FirstOrDefault(p => p != ped && p.CurrentVehicle && p.Speed < 1f && Math.Abs(ped.Position.Z - p.Position.Z) <= 3f && p.DistanceTo2D(ped) <= 10f);
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

            _suspect.Tasks.Clear();
            _suspect.Tasks.EnterVehicle(_victim.CurrentVehicle, -1, -1, 5f, EnterVehicleFlags.AllowJacking).WaitForCompletion();

            while (State != State.Ending && !_suspect.IsInVehicle(_victim.LastVehicle, false))
            {
                GameFiber.Yield();
                CheckSuspectTaskStatus();
            }

            if (State == State.Ending)
            {
                return;
            }

            if (Settings.EventBlips && _suspect.Blip)
            {
                _suspect.Blip.Alpha = 100;
            }

            Game.LogTrivial($"[Rich Ambiance]: Jacker is in the vehicle and driving away.");
            _suspect.Tasks.CruiseWithVehicle(30f, VehicleDrivingFlags.Emergency);
            TransitionToState(State.Ending);
        }

        private void CheckSuspectTaskStatus()
        {
            if (_suspect.Tasks.CurrentTaskStatus == TaskStatus.NoTask)
            {
                Game.LogTrivial($"[Rich Ambiance]: Suspect [{_suspect.Model}, {_suspect.Handle}] has no task.  Reassiging task.");
                _suspect.Tasks.EnterVehicle(_victim.CurrentVehicle, -1, -1, 5f, EnterVehicleFlags.AllowJacking);
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

                if (!_victim.LastVehicle)
                {
                    Game.LogTrivial($"[Rich Ambiance]: Victim's vehicle is invalid.  Ending event.");
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
