using Rage;
using RichAmbiance.Utils;
using System.Linq;
using RichAmbiance.Vehicles;
using System;

namespace RichAmbiance.AmbientEvents.Events
{
    class NoVehicleLights : AmbientEvent
    {
        private Vehicle _suspectVehicle;
        internal NoVehicleLights()
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
            if(World.TimeOfDay.Hours < 20 && World.TimeOfDay.Hours > 5)
            {
                Game.LogTrivial($"[Rich Ambiance (Minor Event)]: It is not dark enough for the {GetType().Name} event.");
                TransitionToState(State.Ending);
                return;
            }

            TransitionToState(State.Preparing);
            _suspectVehicle = GetRandomVehicle();
            if(!_suspectVehicle)
            {
                TransitionToState(State.Ending);
                return;
            }
            Game.LogTrivial($"[Rich Ambiance (Minor Event)]: Suspect vehicle is a {_suspectVehicle.Model.Name}");
        }

        private Vehicle GetRandomVehicle() => World.GetAllVehicles().FirstOrDefault(x => x && x != Game.LocalPlayer.Character.CurrentVehicle && x.HasDriver && x.Driver && x.IsCar && !x.HasSiren);

        private void Process()
        {
            _suspectVehicle.SetVehicleLights(VehicleLightsState.Off);
            if(new Random().Next(2) == 1)
            {
                _suspectVehicle.SetVehicleBrakeLights(false);
            }

            TransitionToState(State.Ending);
        }
    }
}
