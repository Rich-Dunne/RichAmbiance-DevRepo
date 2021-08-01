using Rage;
using RichAmbiance.Utils;
using System;
using System.Linq;

namespace RichAmbiance.AmbientEvents.Events
{
    class Speeding : AmbientEvent
    {
        private Vehicle _suspectVehicle;
        internal Speeding()
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

            _suspectVehicle = FindEventPed();
            if (!_suspectVehicle)
            {
                TransitionToState(State.Ending);
                return;
            }

            new EventPed(_suspectVehicle.Driver, Role.PrimarySuspect, this, true);
            Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect vehicle is a {_suspectVehicle.Model.Name}");
        }

        private Vehicle FindEventPed() => World.GetAllVehicles().FirstOrDefault(x => x && x != Game.LocalPlayer.Character.CurrentVehicle && (x.IsCar || x.IsBike) && !x.HasSiren && x.HasDriver && x.Driver && x.Driver.IsAmbient());

        private void Process()
        {
            TransitionToState(State.Running);
            GameFiber.StartNew(() => CheckEndConditions(), "Rich Ambiance RecklessDriver End Conditions Fiber");

            _suspectVehicle.Driver.Tasks.Clear();
            if (MathHelper.GetChance(3))
            {
                Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect is on their phone.");
                Rage.Native.NativeFunction.Natives.TASK_USE_MOBILE_PHONE_TIMED(_suspectVehicle.Driver, 100000);
            }

            _suspectVehicle.Driver.Tasks.DriveToPosition(new Vector3(new Random().Next(1000), new Random().Next(1000), 0), 35f, (VehicleDrivingFlags)524430);
            Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect is speeding.");
        }

        private void CheckEndConditions()
        {
            while (State != State.Ending)
            {
                GameFiber.Yield();
                if (!_suspectVehicle || !_suspectVehicle.Driver || !_suspectVehicle.Driver.IsAlive)
                {
                    Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect or suspect vehicle no longer valid.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }
                if (!Game.LocalPlayer.Character.IsAlive)
                {
                    Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Player died.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }
                if (LSPD_First_Response.Mod.API.Functions.IsPlayerPerformingPullover() || LSPD_First_Response.Mod.API.Functions.IsPedInPursuit(_suspectVehicle.Driver))
                {
                    Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Player is performing pullover or suspect is in a pursuit.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }
                if (_suspectVehicle.Driver.DistanceTo2D(Game.LocalPlayer.Character) > 150f)
                {
                    Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Player is too far from suspect.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }
            }
        }
    }
}
