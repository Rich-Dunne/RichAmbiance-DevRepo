using Rage;
using System;
using System.Linq;

namespace RichAmbiance.AmbientEvents.Events
{
    internal class RecklessDriver : AmbientEvent
    {
        private Vehicle _suspectVehicle;
        internal RecklessDriver()
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

            TransitionToState(State.Ending, 3000);
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

        private Vehicle FindEventPed() => World.GetAllVehicles().FirstOrDefault(x => x && x != Game.LocalPlayer.Character.CurrentVehicle && x.HasDriver && x.Driver && x.IsCar && !x.HasSiren);

        private void Process()
        {
            _suspectVehicle.Driver.Tasks.Clear();
            //_suspectVehicle.Driver.Tasks.CruiseWithVehicle(new Random().Next(10, 50), VehicleDrivingFlags.Emergency);
            _suspectVehicle.Driver.Tasks.DriveToPosition(new Vector3(new Random().Next(1000), new Random().Next(1000), 0), new Random().Next(5, 31), VehicleDrivingFlags.Emergency);
            Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect is driving carelessly.");
        }
    }
}
