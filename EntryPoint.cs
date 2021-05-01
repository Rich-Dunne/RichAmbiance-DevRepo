using LSPD_First_Response.Mod.API;
using Rage;
using System.Linq;
using System.Reflection;
using System.Threading;

[assembly: Rage.Attributes.Plugin("Rich Ambiance", Author = "Rich", Description = "Ambient events for LSPDFR", PrefersSingleInstance = true)]

namespace RichAmbiance
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Settings.LoadSettings();
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                CheckRPEVersion();
                InitializeFeatures();
                GetAssemblyVersion();
            }
        }


        public override void Finally()
        {
            Features.AmbientEvents.ActiveEvents.ForEach(x => x.TransitionToState(AmbientEvents.State.Ending));
        }

        private static void CheckRPEVersion()
        {
            if (Functions.GetAllUserPlugins().ToList().Any(a => a.FullName.Contains("RichsPoliceEnhancements")))
            {
                var assembly = Thread.GetDomain().GetAssemblies().First(x => x.FullName.Contains("RichsPoliceEnhancements"));
                Game.LogTrivial($"[RichAmbiance]: RPE version is {assembly.GetName().Version}");
                if (assembly.GetName().Version < new System.Version(1, 5, 2))
                {
                    Game.DisplayNotification($"~o~Rich Ambiance ~r~[Warning]\n~w~In order to avoid conflicts, ~b~Rich's Police Enhancements ~w~needs to be updated.");
                }
            }
        }

        private static void InitializeFeatures()
        {
            Game.AddConsoleCommands();
            if (Settings.EnableAmbientEvents)
            {
                Game.LogTrivial("[RichAmbiance]: AmbientEvents are enabled.");
                GameFiber.StartNew(() => Features.AmbientEvents.Main(), "RichAmbiance AmbientEvents Fiber");
            }
            else
            {
                Game.LogTrivial("[RichAmbiance]: AmbientEvents are disabled.");
            }
        }

        private static void GetAssemblyVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.LogTrivial($"Rich Ambiance V{version} is ready.");
        }
    }
}
