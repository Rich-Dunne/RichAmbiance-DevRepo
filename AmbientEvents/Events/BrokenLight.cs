﻿using Rage;
using RichAmbiance.Utils;
using System;
using System.Linq;

namespace RichAmbiance.AmbientEvents.Events
{
    class BrokenLight : AmbientEvent
    {
        private Vehicle _suspectVehicle;
        internal BrokenLight()
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
            if (World.TimeOfDay.Hours < 20 && World.TimeOfDay.Hours > 5)
            {
                Game.LogTrivial($"[Rich Ambiance]: It is not dark enough for the {GetType().Name} event.");
                TransitionToState(State.Ending);
                return;
            }

            TransitionToState(State.Preparing);
            _suspectVehicle = GetRandomVehicle();
            if (!_suspectVehicle)
            {
                TransitionToState(State.Ending);
                return;
            }
            Game.LogTrivial($"[Rich Ambiance]: Suspect vehicle is a {_suspectVehicle.Model.Name}");
        }

        private Vehicle GetRandomVehicle() => World.GetAllVehicles().FirstOrDefault(x => x && x.HasDriver && x.Driver && x.IsCar && !x.HasSiren);

        private void Process()
        {
            string brokenLight = GetVehicleBone();
            if(State == State.Ending)
            {
                return;
            }

            Vehicle vehicleToCopy = CreateVehicleToCopyDamage();

            if(!vehicleToCopy)
            {
                Game.LogTrivial($"[Rich Ambiance]: Vehicle to copy is not valid.  Ending event.");
                TransitionToState(State.Ending);
                return;
            }

            DamageVehicles(vehicleToCopy, brokenLight);
            vehicleToCopy.Delete();
            TransitionToState(State.Ending);
        }

        private string GetVehicleBone()
        {
            string[] boneStrings = { "headlight_l", "headlight_r", "taillight_l", "taillight_r" };
            var brokenLight = MathHelper.Choose(boneStrings);
            if (!_suspectVehicle.HasBone(brokenLight))
            {
                Game.LogTrivial($"[Rich Ambiance]: Vehicle does not have bone {brokenLight}");
                TransitionToState(State.Ending);
            }

            return brokenLight;
        }

        private Vehicle CreateVehicleToCopyDamage()
        {
            var vehicleToCopy = new Vehicle("SHERIFF2", new Vector3(0, 0, 0), _suspectVehicle.Heading);
            vehicleToCopy.IsCollisionEnabled = false;
            vehicleToCopy.IsGravityDisabled = true;
            vehicleToCopy.Opacity = 0;
            return vehicleToCopy;
        }

        private void DamageVehicles(Vehicle vehicleToCopy, string brokenLight)
        {
            var boneIndex = vehicleToCopy.GetBoneIndex(brokenLight);
            var bonePosition = vehicleToCopy.GetBonePosition(boneIndex);
            switch (brokenLight)
            {
                case "taillight_l":
                case "taillight_r":
                    NativeWrappers.SetVehicleDamage(vehicleToCopy, vehicleToCopy.GetPositionOffset(bonePosition), 100, 50, true);
                    Game.LogTrivial($"[Rich Ambiance]: Breaking a taillight.");
                    break;
                case "headlight_l":
                    NativeWrappers.SetVehicleDamage(vehicleToCopy, vehicleToCopy.FrontPosition.GetOffset(vehicleToCopy.Heading, new Vector3(-2, 5, 1)), 150, 50, false);
                    Game.LogTrivial($"[Rich Ambiance]: Breaking left headlight.");
                    break;
                case "headlight_r":
                    NativeWrappers.SetVehicleDamage(vehicleToCopy, vehicleToCopy.FrontPosition.GetOffset(vehicleToCopy.Heading, new Vector3(15, 50, 3)), 110, 100, false);
                    Game.LogTrivial($"[Rich Ambiance]: Breaking right headlight.");
                    break;
            }

            GameFiber.Sleep(1000);
            if(!vehicleToCopy)
            {
                Game.LogTrivial($"[Rich Ambiance]: Vehicle to copy is invalid");
                TransitionToState(State.Ending);
                return;
            }
            NativeWrappers.CopyVehicleDamages(vehicleToCopy, _suspectVehicle);
            //Game.LogTrivial($"Damage copied to target vehicle.");
        }
    }
}