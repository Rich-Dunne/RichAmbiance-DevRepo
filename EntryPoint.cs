﻿using LSPD_First_Response.Mod.API;
using Rage;
using System.Reflection;

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
                InitializeFeatures();
                GetAssemblyVersion();
            }
        }


        public override void Finally()
        {

        }

        private static void InitializeFeatures()
        {
            if (Settings.EnableAmbientEvents)
            {
                Game.LogTrivial("[RichAmbiance]: AmbientEvents are enabled.");
                GameFiber.StartNew(() => AmbientEvents.Main(), "RichAmbiance AmbientEvents Fiber");
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
