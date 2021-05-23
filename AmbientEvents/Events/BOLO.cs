using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using RichAmbiance.Utils;
using RichAmbiance.Vehicles;

namespace RichAmbiance.AmbientEvents.Events
{
    internal class BOLO : AmbientEvent
    {
        private SoundPlayer _SoundPlayer = new SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
        private Blip _StartBlip;
        private Vehicle _SuspectVehicle;
        private enum Direction
        {
            invalid = -1,
            north = 0,
            east = 1,
            south = 2,
            west = 3
        }

        private static readonly string[] boloReasons = {
            "a warrant out of Los Santos County",
            "a warrant out of Blaine County",
            "a citizen traffic complaint",
            "an individual who recently left a physical disturbance",
            "an individual who recently left a verbal disturbance",
            "an individual who recently left the scene of a crime",
            "an individual who recently shoplifted from a gas station",
            "an individual who recently shoplifted from a liquor store",
            "an individual who recently shoplifted from Binco Clothes",
            "an individual who recently menaced the reporting party",
            "suspicious circumstances in the area",
            "the registered owner is wanted for questioning by LSPD regarding a homicide",
            "the registered owner is wanted for questioning by LSSD regarding a homicide",
            "the registered owner is wanted for questioning by BCSO regarding a homicide",
            "the registered owner is wanted for questioning by LSPD regarding an assault",
            "the registered owner is wanted for questioning by LSSD regarding an assault",
            "the registered owner is wanted for questioning by BCSO regarding an assault",
            "the registered owner is wanted for questioning by LSPD regarding a burglary",
            "the registered owner is wanted for questioning by LSSD regarding a burglary",
            "the registered owner is wanted for questioning by BCSO regarding a burglary",
            "the registered owner is wanted for questioning by LSPD regarding an armed robbery",
            "the registered owner is wanted for questioning by LSSD regarding an armed robbery",
            "the registered owner is wanted for questioning by BCSO regarding an armed robbery",
            "a usual occupant wanted for questioning by LSPD regarding a homicide",
            "a usual occupant wanted for questioning by LSSD regarding a homicide",
            "a usual occupant wanted for questioning by BCSO regarding a homicide",
            "a usual occupant wanted for questioning by LSPD regarding an assault",
            "a usual occupant wanted for questioning by LSPD regarding an assault at the Vanilla Unicorn",
            "a usual occupant wanted for questioning by LSSD regarding an assault",
            "a usual occupant wanted for questioning by BCSO regarding an assault",
            "a usual occupant wanted for questioning by BCSO regarding an assault at the Hen House",
            "a usual occupant wanted for questioning by BCSO regarding an assault at the Yellow Jacket",
            "a usual occupant wanted for questioning by LSPD regarding a burglary",
            "a usual occupant wanted for questioning by LSSD regarding a burglary",
            "a usual occupant wanted for questioning by BCSO regarding a burglary",
            "a usual occupant wanted for questioning by LSPD regarding an armed robbery",
            "a usual occupant wanted for questioning by LSPD regarding an armed robbery at Vangelico",
            "a usual occupant wanted for questioning by LSSD regarding an armed robbery",
            "the vehicle being seen leaving the area of a homicide in Los Santos",
            "the vehicle being seen leaving the area of a homicide in Los Santos County",
            "the vehicle being seen leaving the area of a homicide in Blaine County",
            "the vehicle being seen leaving the area of an assault at the Yellow Jacket",
            "the vehicle being seen leaving the area of an assault at the Hen House",
            "the vehicle being seen leaving the area of an assault at the Vanilla Unicorn",
            "the vehicle being seen leaving the area of an armed robbery in Los Santos",
            "the vehicle being seen leaving the scene of an armed robbery at Dollar Pills in South LS",
            "the vehicle being seen leaving the scene of an armed robbery at the Innocence Blvd 24/7 store",
            "the vehicle being seen leaving the scene of an armed at the Pawn & Jewelry",
            "the vehicle being seen leaving the scene of an armed robbery at Vangelico",
            "the vehicle being seen leaving the scene of an armed robbery in Los Santos County",
            "the vehicle being seen leaving the scene of an armed robbery at the 24/7 store in Chumash",
            "the vehicle being seen leaving the scene of an armed robbery in Blaine County",
            "the vehicle being seen leaving the scene of an armed robbery at the Route 68 24/7 store",
            "the vehicle being seen leaving the scene of an armed robbery at Liquor Ace",
            "the vehicle reportedly driving off from the LTD station in South LS without paying for gas",
            "the vehicle reportedly driving off from the LTD station in Mirror Park without paying for gas",
            "the vehicle reportedly driving off from the LTD station in Banham Canyon without paying for gas",
            "the vehicle reportedly driving off from the Grapeseed LTD station without paying for gas",
            "the vehicle reportedly driving off from the Tataviam Truckstop without paying for gas",
            "the vehicle reportedly driving off from Perth St RON Station without paying for gas",
            "the vehicle reportedly driving off from Mcdonald St RON Station without paying for gas",
            "the vehicle reportedly driving off from Popular St RON Station without paying for gas",
            "the vehicle reportedly driving off from El Rancho Blvd  RON Station without paying for gas",
            "the vehicle reportedly driving off from Paleto Bay RON Station without paying for gas",
            "the driver reportedly stealing several items from the South LS LTD station",
            "the driver reportedly stealing several items from the Mirror Park LTD station",
            "the driver reportedly stealing several items and fled from the Banham Canyon LTD station",
            "the driver reportedly stealing several items from the Grapeseed LTD station",
            "the driver reportedly stealing several items from the Perth St RON Station",
            "the driver reportedly stealing several items from the Mcdonald St RON Station",
            "the driver reportedly stealing several items from the Popular St RON Station",
            "the driver reportedly stealing several items from the El Rancho Blvd RON Station",
            "the driver reportedly stealing several items from the Paleto Bay RON Station",
            "the driver reportedly stealing several items from the Vespucci Canals Rob’s Liquor",
            "the driver reportedly stealing several items from the Prosperity Street Rob’s Liquor",
            "the driver reportedly stealing several items from the El Rancho Boulevard Rob’s Liquor",
            "the driver reportedly stealing several items from the Ace Liquor in Sandy Shores",
            "the driver reportedly stealing several items from Vangelico in Rockford Hills",
            "the driver reportedly stealing several items from the Paleto Bay RON Station",
            "the driver reportedly stealing several items from the Dollar Pills in South LS",
            "the driver reportedly stealing several items from the Dollar Pills on Route 68 in Harmony",
            "the driver reportedly stealing several items from the Discount Store in South LS",
            "the driver reportedly stealing several items from the Paleto Boulevard Discount Store",
            "the driver reportedly stealing several items from the Grapeseed Discount Store",
            "the vehicle was recently reported stolen out of Los Santos",
            "the vehicle was recently reported stolen out of Los Santos County",
            "the vehicle was recently reported stolen out of Blaine County",
            "the vehicle being involved in a road rage incident",
            "the vehicle was involved in a hit and run accident in Los Santos",
            "the vehicle was involved in a hit and run accident in Los Santos County",
            "the vehicle was involved in a hit and run accident in Blaine County",
            "the vehicle was involved in a hit and run accident on the Los Santos Freeway",
            "the vehicle was involved in a hit and run accident on the Del Perro Freeway",
            "the vehicle was involved in a hit and run accident on the Olympic Freeway",
            "the vehicle was involved in a hit and run accident on the La Puerta Freeway",
            "the vehicle fled from LSPD during a traffic stop",
            "the vehicle fled from LSSD during a traffic stop",
            "the vehicle fled from BCSO during a traffic stop",
            "the vehicle fled from SAHP during a traffic stop",
            "the vehicle fled from LSPD during a traffic stop and nearly struck an officer",
            "the vehicle fled from LSSD during a traffic stop and nearly struck a deputy",
            "the vehicle fled from BCSO during a traffic stop and nearly struck a deputy",
            "the vehicle fled from SAHP during a traffic stop and nearly struck a trooper",
            "the vehicle fled from LSPD during a traffic stop and struck an officer",
            "the vehicle fled from LSSD during a traffic stop and struck a deputy",
            "the vehicle fled from BCSO during a traffic stop and struck a deputy",
            "the vehicle fled from SAHP during a traffic stop and struck a trooper",
            "the vehicle fled from LSPD during a traffic stop, shots fired at officers",
            "the vehicle fled from LSSD during a traffic stop, shots fired at deputies",
            "the vehicle fled from BCSO during a traffic stop, shots fired at deputies",
            "the vehicle fled from SAHP during a traffic stop, shots fired at troopers",
            "the driver reportedly discharged a weapon from the vehicle",
            "the driver left from the scene of a domestic in Los Santos",
            "the driver left from the scene of a domestic in Los Santos County",
            "the driver left from the scene of a domestic in Blaine County",
            "the driver left from the scene of a domestic in Los Santos and may be armed",
            "the driver left from the scene of a domestic in Los Santos County and may be armed",
            "the driver left from the scene of a domestic in Blaine County and may be armed",
            "a caller stating the occupant has active warrants",
            "a caller stating the occupant may be suicidal",
            "a caller stating the occupant may be intoxicated",
            "a caller stating the occupant made threats against a third party",
            "a caller stating the occupant made threats against themselves and others",
            "a caller stating the vehicle was driving recklessly"
        };

        internal BOLO()
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
            _SuspectVehicle = FindBOLOVehicle();
            
            if (!_SuspectVehicle)
            {
                TransitionToState(State.Ending);
                return;
            }
            new EventPed(_SuspectVehicle.Driver, Role.PrimarySuspect, this, false);

            Game.LogTrivial($"[Rich Ambiance (BOLO)]: Suspect vehicle is a {_SuspectVehicle.Model.Name}");
        }

        private Vehicle FindBOLOVehicle() => World.GetAllVehicles().FirstOrDefault(x => x && x.IsCar && !x.IsPoliceVehicle && !x.HasSiren && !x.HasTowArm && x.HasDriver && x.Driver && x.Driver.IsAlive && x.Driver != Game.LocalPlayer.Character && x.Driver.IsAmbient());

        private void CreateBOLOStartBlip()
        {
            _StartBlip = new Blip(_SuspectVehicle.GetOffsetPosition(new Vector3(new Random().Next(20), new Random().Next(20), _SuspectVehicle.Position.Z)), 100f);
            _StartBlip.Color = Color.Yellow;
            _StartBlip.Alpha = 0.75f;
            EventBlips.Add(_StartBlip);
            Game.LogTrivial($"[Rich Ambiance (BOLO)]: Start blip created.");
        }

        private void Process()
        {
            if(Settings.EnableBOLOStartBlip && _StartBlip)
            {
                GameFiber.StartNew(() => FadeBOLOStartBlip(), "BOLO Start Blip Fade Fiber");
            }

            DisplayBOLOInfo();

            var startTime = Game.GameTime;
            var oldTime = startTime;
            while (State != State.Ending && _SuspectVehicle && _SuspectVehicle.Driver)
            {
                GameFiber.Sleep(1000);
                if(!_SuspectVehicle || !_SuspectVehicle.Driver || !_SuspectVehicle.Driver.IsAlive)
                {
                    _SoundPlayer.Play();
                    Game.DisplayNotification($"~r~~h~BOLO ALERT - CANCELLED~h~\n~w~The suspect is dead.");
                    Game.LogTrivial($"[Rich Ambiance (BOLO)]: Vehicle or driver are invalid, or driver is dead.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                var difference = Game.GameTime - oldTime;
                if (Functions.IsPedArrested(_SuspectVehicle.Driver) || (Functions.GetCurrentPullover() != null && Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == _SuspectVehicle.Driver))
                {
                    _SoundPlayer.Play();
                    Game.DisplayNotification($"~r~~h~BOLO ALERT - CANCELLED~h~\n~g~You located the suspect(s).");
                    Game.LogTrivial($"[Rich Ambiance (BOLO)]: BOLO ending, suspect stopped or arrested by player.");
                    TransitionToState(State.Ending);
                    return;
                }
                if (Game.GameTime - startTime >= Settings.BOLOTimer)
                {
                    _SoundPlayer.Play();
                    Game.DisplayNotification($"~r~~h~BOLO ALERT - CANCELLED~h~\n~w~You failed to locate the suspect(s) in time.");
                    Game.LogTrivial($"[Rich Ambiance (BOLO)]: BOLO ending, player failed to locate suspect(s) in time.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (difference >= 60000 && UpdateCheck(_SuspectVehicle.Driver))
                {
                    oldTime = Game.GameTime;
                    DisplayBOLOInfo(true);
                }
            }
        }

        private void FadeBOLOStartBlip()
        {
            var oldDistance = _SuspectVehicle.DistanceTo2D(_StartBlip.Position);
            while (_SuspectVehicle && _StartBlip.Alpha > 0)
            {
                GameFiber.Yield();
                if (Math.Abs(_SuspectVehicle.DistanceTo2D(_StartBlip.Position) - oldDistance) > 0.15 && _SuspectVehicle.DistanceTo2D(_StartBlip.Position) > oldDistance && _StartBlip.Alpha > 0f)
                {
                    _StartBlip.Alpha -= 0.001f;
                }
                else if (Math.Abs(_SuspectVehicle.DistanceTo2D(_StartBlip.Position) - oldDistance) > 0.15 && _SuspectVehicle.DistanceTo2D(_StartBlip.Position) < oldDistance && _StartBlip.Alpha < 1.0f)
                {
                    _StartBlip.Alpha += 0.01f;
                }
                oldDistance = _SuspectVehicle.DistanceTo2D(_StartBlip.Position);
            }

            if (_StartBlip && _StartBlip.Alpha < 1)
            {
                _StartBlip.Delete();
                Game.LogTrivial($"[Rich Ambiance (BOLO)]: Start Blip deleted.");
            }
        }

        private void DisplayBOLOInfo(bool update = false)
        {
            string boloColor;
            try
            {
                boloColor = _SuspectVehicle.GetColors().PrimaryColorName;
            }
            catch
            {
                Game.LogTrivial($"[Rich Ambiance (BOLO)]: There was a problem getting the vehicle's color.  Ending BOLO event.");
                TransitionToState(State.Ending);
                return;
            }
            var boloVehSkin = VehicleSkin.FromVehicle(_SuspectVehicle);
            var worldZone = Functions.GetZoneAtPosition(_SuspectVehicle.Position).RealAreaName;
            var streetName = World.GetStreetName(World.GetStreetHash(_SuspectVehicle.Position));
            var boloReason = boloReasons[new Random().Next(0, boloReasons.Length)];
            _SoundPlayer.Play();

            if (!update)
            {
                if (Settings.EnableBOLOStartBlip)
                {
                    CreateBOLOStartBlip();
                }
                Game.DisplayNotification($"~y~~h~BOLO ALERT - OCCUPANT(S) WANTED~h~\n~s~~w~Be on the lookout for a(n): ~o~{boloColor} {_SuspectVehicle.Model.Name} (plate {boloVehSkin.LicensePlate})~w~ last seen heading ~b~{GetSuspectDirection()} ~w~on ~b~{streetName} ~w~in ~b~{worldZone}.  ~w~This BOLO is in regards to ~r~{boloReason}.");
            }
            else
            {
                Game.DisplayNotification($"~y~~h~BOLO UPDATE - OCCUPANT(S) WANTED~h~\n~s~~o~{boloColor} {_SuspectVehicle.Model.Name} (plate {boloVehSkin.LicensePlate})~w~ last seen heading ~b~{GetSuspectDirection()} ~w~on ~b~{streetName} ~w~in ~b~{worldZone}.");
            }
        }

        private Direction GetSuspectDirection()
        {
            if (_SuspectVehicle.Heading >= 315 || _SuspectVehicle.Heading < 45)
            {
                return Direction.north;
            }
            else if (_SuspectVehicle.Heading >= 45 && _SuspectVehicle.Heading < 135)
            {
                return Direction.west;
            }
            else if (_SuspectVehicle.Heading >= 135 && _SuspectVehicle.Heading < 225)
            {
                return Direction.south;
            }
            else if (_SuspectVehicle.Heading >= 225 && _SuspectVehicle.Heading < 315)
            {

                return Direction.east;
            }
            return Direction.invalid;
        }

        private bool UpdateCheck(Ped boloSuspect)
        {
            var policeVehicleSawBOLOVehicle = boloSuspect.GetNearbyVehicles(16).FirstOrDefault(x => x && x.IsPoliceVehicle && x.HasDriver && x.Driver.IsAlive && x.DistanceTo2D(_SuspectVehicle) <= 50f);
            if (policeVehicleSawBOLOVehicle)
            {
                Game.LogTrivial("[Rich Ambiance (BOLO)]: Police vehicle saw suspect.");
                return true;
            }

            var policePedSawBOLOVehicle = boloSuspect.GetNearbyPeds(16).FirstOrDefault(x => x && x.IsAlive && x.RelationshipGroup == "COP" && x.DistanceTo2D(_SuspectVehicle) <= 50f);
            if (policePedSawBOLOVehicle)
            {
                Game.LogTrivial("[Rich Ambiance (BOLO)]: Police ped saw suspect.");
                return true;
            }
            return false;
        }
    }
}
