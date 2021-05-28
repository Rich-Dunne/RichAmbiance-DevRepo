using Rage;
using System.Drawing;

namespace RichAmbiance.AmbientEvents
{
    enum Role
    {
        PrimarySuspect = 0,
        SecondarySuspect = 1,
        Victim = 2,
    }

    internal class EventPed : Ped
    {
        internal AmbientEvent AmbientEvent { get; private set; }

        internal Role Role { get; private set; }

        internal Blip Blip { get; private set; }

        internal EventPed(Ped ped, Role role, AmbientEvent ambientEvent, bool giveBlip, BlipSprite sprite = BlipSprite.StrangersandFreaks) : base(ped.Handle)
        {
            AmbientEvent = ambientEvent;
            Handle = ped.Handle;
            SetPersistence();
            Role = role;
            if (Settings.EventBlips && giveBlip)
            {
                CreateBlip(sprite);
            }
            AmbientEvent.EventPeds.Add(this);
        }

        private void SetPersistence()
        {
            IsPersistent = true;
            BlockPermanentEvents = true;
            if(CurrentVehicle)
            {
                CurrentVehicle.IsPersistent = true;
            }
        }

        private void CreateBlip(BlipSprite sprite)
        {
            Blip = AttachBlip();
            Blip.Sprite = sprite;
            if (Role == Role.PrimarySuspect)
            {
                Blip.Color = Color.Red;
                if (AmbientEvent.EventType == EventType.DriveBy)
                {
                    Blip.Alpha = 0;
                }
            }
            if (Role == Role.Victim)
            {
                Blip.Color = Color.White;
            }
            Blip.Scale = 0.75f;
            AmbientEvent.EventBlips.Add(Blip);
        }

        internal void Dismiss()
        {
            if(Blip)
            {
                Blip.Delete();
            }

            if(LastVehicle)
            {
                LastVehicle.Dismiss();
            }

            base.Dismiss();
        }
    }
}
