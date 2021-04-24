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

            TransitionToState(State.Ending);
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
            Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect vehicle is a {_suspectVehicle.Model.Name}");
        }

        private Vehicle FindEventPed() => World.GetAllVehicles().FirstOrDefault(x => x && x != Game.LocalPlayer.Character.CurrentVehicle && x.HasDriver && x.Driver && x.IsCar && !x.HasSiren);

        private void Process()
        {
            _suspectVehicle.Driver.Tasks.Clear();
            _suspectVehicle.Driver.Tasks.DriveToPosition(new Vector3(new Random().Next(1000), new Random().Next(1000), 0), 35f, VehicleDrivingFlags.DriveAroundVehicles);
            Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect is speeding.");

        }
    }
}
