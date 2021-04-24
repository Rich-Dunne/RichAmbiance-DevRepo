using Rage;
using System.Drawing;

namespace RichAmbiance.AmbientEvents
{
    using RichAmbiance.Features;

    enum Role
    {
        PrimarySuspect = 0,
        SecondarySuspect = 1,
        Victim = 2,
    }

    internal class EventPed : Ped
    {
        internal AmbientEvent Event { get; private set; }

        internal Role Role { get; private set; }

        internal Blip Blip { get; private set; }

        internal EventPed(Ped ped, Role role, bool giveBlip, BlipSprite sprite = BlipSprite.StrangersandFreaks)
        {
            Event = AmbientEvents.ActiveEvent;
            Handle = ped.Handle;
            SetPersistence();
            Role = role;
            if (Settings.EventBlips && giveBlip)
            {
                CreateBlip(sprite);
            }
            Event.EventPeds.Add(this);
        }

        private void SetPersistence()
        {
            IsPersistent = true;
            BlockPermanentEvents = true;
        }

        private void CreateBlip(BlipSprite sprite)
        {
            Blip = AttachBlip();
            Blip.Sprite = sprite;
            if (Role == Role.PrimarySuspect)
            {
                Blip.Color = Color.Red;
                if (Event.EventType == EventType.DriveBy)
                {
                    Blip.Alpha = 0;
                }
            }
            if (Role == Role.Victim)
            {
                Blip.Color = Color.White;
            }
            Blip.Scale = 0.75f;
            Event.EventBlips.Add(Blip);
        }
    }
}
