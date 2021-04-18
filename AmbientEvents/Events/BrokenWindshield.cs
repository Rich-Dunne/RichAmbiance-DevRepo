using Rage;
using RichAmbiance.Utils;
using System.Linq;

namespace RichAmbiance.AmbientEvents.Events
{
    class BrokenWindshield : AmbientEvent
    {
        private Vehicle _suspectVehicle;
        internal BrokenWindshield()
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
            _suspectVehicle = GetRandomVehicle();
            if (!_suspectVehicle)
            {
                TransitionToState(State.Ending);
                return;
            }
            Game.LogTrivial($"[Rich Ambiance]: Suspect vehicle is a {_suspectVehicle.Model.Name}");
        }

        private Vehicle GetRandomVehicle() => World.GetAllVehicles().FirstOrDefault(x => x && x != Game.LocalPlayer.Character.CurrentVehicle && x.HasDriver && x.Driver && x.IsCar && !x.HasSiren);

        private void Process()
        {
            BreakWindshield();
            TransitionToState(State.Ending);
        }

        private void BreakWindshield()
        {
            _suspectVehicle.BreakWindow(6);
            Game.LogTrivial($"[Rich Ambiance]: Breaking windshield");
        }
    }
}
