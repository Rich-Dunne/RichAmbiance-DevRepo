using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using RichAmbiance.Utils;

namespace RichAmbiance.AmbientEvents.Events
{
    internal class DriveBy : AmbientEvent
    {
        private List<Ped> _usablePeds;
        private EventPed _suspect, _victim;
        private float _oldDistance;

        internal DriveBy()
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

        new private void Prepare()
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

        private void GetUsablePeds() => _usablePeds = HelperMethods.GetReleventPedsForAmbientEvent();

        private void SelectPedPair()
        {
            foreach (Ped ped in _usablePeds.Where(p => p.IsInAnyVehicle(false) && (p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican)))
            {
                var victim = _usablePeds.FirstOrDefault(p => p != ped && ped.RelationshipGroup != p.RelationshipGroup && p.CurrentVehicle != ped.CurrentVehicle && Math.Abs(ped.Position.Z - p.Position.Z) <= 5f && p.DistanceTo2D(ped) <= 15f);
                if (victim)
                {
                    _suspect = new EventPed(ped, Role.PrimarySuspect, true, (BlipSprite)229);
                    _victim = new EventPed(victim, Role.Victim, false);
                    return;
                }
            }
        }

        new private void Process()
        {
            TransitionToState(State.Running);
            GameFiber.StartNew(() => CheckEndConditions(), "RPE End Conditions Fiber");

            Functions.SetPedResistanceChance(_suspect, 100);
            _suspect.Tasks.Clear();

            GiveSuspectWeapon();
            AssignSuspectTasks();
        }

        private void GiveSuspectWeapon()
        {
            WeaponHash[] weaponPool = { WeaponHash.MicroSMG, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.Pistol, WeaponHash.Pistol50 };
            if (_suspect.Inventory.Weapons.Count == 0)
            {
                Game.LogTrivial($"[Rich Ambiance] Giving driver random weapon from pool");
                _suspect.Inventory.GiveNewWeapon(weaponPool[new Random().Next(0, weaponPool.Length)], 50, true);
            }
            //foreach (WeaponDescriptor weapon in _suspect.Inventory.Weapons)
            //{
            //    Game.LogTrivial($"[Rich Ambiance] Weapon hash: {weapon.Hash}");
            //}
        }

        private void AssignSuspectTasks()
        {
            Game.LogTrivial($"[Rich Ambiance] Driver shooting at victim");
            _suspect.ShootFromVehicle(_victim);
            //Rage.Native.NativeFunction.Natives.x10AB107B887214D8(_suspect, _victim, 0); // vehicle shoot task
            if (_suspect.Blip)
            {
                _suspect.Blip.Alpha = 100;
            }

            GameFiber.Sleep(3000);
            _suspect.Tasks.Clear();
            _suspect.Tasks.CruiseWithVehicle(30f, VehicleDrivingFlags.Emergency);
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
                    Game.LogTrivial($"[Rich Ambiance]: Victim is too from suspect.  Ending event.");
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
