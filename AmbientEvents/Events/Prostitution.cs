using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using RichAmbiance.Utils;
using RichAmbiance.PathFind;

namespace RichAmbiance.AmbientEvents.Events
{
    using RichAmbiance.Features;

    internal class Prostitution : AmbientEvent
    {
        private List<Ped> _prostitutes;
        private List<RoadNode> RoadPoints;
        private Vector3 _roadNode;
        private float _roadNodeHeading;
        private int _roadNodeFlags;
        private EventPed _prostitute;
        private EventPed _john;
        private bool _johnFound;
        private Vector3 _pulloverPosition;
        private int _indexOfNodeToDriveTo;
        private float _oldDistance;

        internal Prostitution()
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
            FindProstitute();
        }

        private void FindProstitute()
        {
            for (int i = 0; i < 100; i++)
            {
                GameFiber.Sleep(500);
                if (GuardClauses.EventPedsFound(EventPeds, 1, i))
                {
                    return;
                }

                CollectNearbyProstitutes();
                SelectBestProstitute();
            }

            Game.LogTrivial($"[Rich Ambiance]: Unable to find suitable event peds after 100 attempts.  Ending event.");
            TransitionToState(State.Ending);
            return;
        }

        private void CollectNearbyProstitutes() => _prostitutes = HelperMethods.GetReleventPedsForAmbientEvent().Where(p => p.IsOnFoot && (p.Model.Name.Contains("HOOKER") || p.Model.Name.Contains("TRANVEST"))).ToList();

        private void SelectBestProstitute()
        {
            RoadPoints = new List<RoadNode>() { null, null, null };
            foreach (Ped prostitute in _prostitutes.Where(x => x && x.Speed < 1f && NearestRoadNodeToPedIsValid(x.Position)))
            {
                RoadPoints[1] = new RoadNode(_roadNode, _roadNodeHeading, RoadNodeIsAlley(_roadNodeFlags));
                GetFirstUsablePreviousRoadNode(prostitute, RoadPoints[1].Position, RoadPoints[1].Heading);
                RoadPoints[0] = RoadPoints[1].PreviousNode;

                _indexOfNodeToDriveTo = RoadPoints.IndexOf(RoadPoints.First(x => x != null)) + 1;
                if (!UsablePulloverPositionFound(_indexOfNodeToDriveTo))
                {
                    Game.LogTrivial($"[Rich Ambiance]: No good pullover position found for this prostitute.");
                    continue;
                }

                if (RoadPoints[0] != null || RoadPoints[2] != null)
                {
                    //Game.DisplaySubtitle($"Prostitute found.");
                    _prostitute = new EventPed(prostitute, Role.PrimarySuspect, this, true, BlipSprite.DropOffHooker);
                    NativeWrappers.TaskStartScenarioInPlace(_prostitute, "WORLD_HUMAN_PROSTITUTE_LOW_CLASS");
                    return;
                }

                RoadPoints.ForEach(x => x = null);
            }
        }

        private bool NearestRoadNodeToPedIsValid(Vector3 position)
        {
            NativeWrappers.GetClosestRoadNodeWithHeading(position, out Vector3 closestRoadNode, out float closestRoadNodeHeading);
            NativeWrappers.GetRoadNodeProperties(closestRoadNode, out int density, out int flags);
            _roadNode = closestRoadNode;
            _roadNodeHeading = closestRoadNodeHeading;
            _roadNodeFlags = flags;

            if (!position.WithinNearbyRoadNode(_roadNode, _roadNodeHeading))
            {
                var invertedNode = _roadNode.GetOffset(_roadNodeHeading.Invert(), new Vector3(0, 0, 0));
                NativeWrappers.GetClosestRoadNodeWithHeading(invertedNode, out Vector3 invertedClosestRoadNode, out float invertedClosestRoadNodeHeading);
                NativeWrappers.GetRoadNodeProperties(invertedClosestRoadNode, out int invertedDensity, out int invertedFlags);

                if (!position.WithinNearbyRoadNode(invertedClosestRoadNode, invertedClosestRoadNodeHeading))
                {
                    return false;
                }

                _roadNode = invertedClosestRoadNode;
                _roadNodeHeading = invertedClosestRoadNodeHeading;
                _roadNodeFlags = invertedFlags;
            }

            if (!RoadNodeFlagsAreValid(_roadNodeFlags))
            {
                return false;
            }

            return true;
        }

        private static bool RoadNodeFlagsAreValid(int roadNodeFlags)
        {
            var badNodeFlags = RoadNodeFlags.MinorRoad | RoadNodeFlags.SlowStopIndicator | RoadNodeFlags.Intersection | RoadNodeFlags.Stop | RoadNodeFlags.StopBeforeIntersection;
            if (badNodeFlags.HasFlag((RoadNodeFlags)roadNodeFlags) || roadNodeFlags == 130 || roadNodeFlags == 134)
            {
                return false;
            }
            return true;
        }

        private static bool RoadNodeIsAlley(int roadNodeFlags)
        {
            if (roadNodeFlags == (float)RoadNodeFlags.MinorRoad || roadNodeFlags == (float)RoadNodeFlags.Alley2 || roadNodeFlags == (float)RoadNodeFlags.Alley || roadNodeFlags == (float)RoadNodeFlags.Unknown3)
            {
                return true;
            }
            return false;
        }

        private void GetFirstUsablePreviousRoadNode(Ped ped, Vector3 closestRoadNode, float closestRoadNodeHeading)
        {
            Vector3 offsetPosition;

            for (int i = 0; i > -20; i--)
            {
                if (State == State.Ending)
                {
                    return;
                }

                offsetPosition = RoadPoints[1].IsAlley ? new Vector3(-i, i, 0) : new Vector3(0, i, 0);
                NativeWrappers.GetClosestRoadNodeWithHeading(closestRoadNode.GetOffset(closestRoadNodeHeading, offsetPosition), out Vector3 previousClosestRoadNode, out float previousClosestRoadNodeHeading);
                NativeWrappers.GetRoadNodeProperties(previousClosestRoadNode, out int previousDensity, out int previousFlags);

                if (!RoadNodeFlagsAreValid(previousFlags))
                {
                    //Game.LogTrivial($"Invalid node");
                    continue;
                }
                if (RoadNodeIsAlley(previousFlags))
                {
                    //Game.LogTrivial($"Previous node is alley");
                    continue;
                }
                if (!ped.WithinNearbyRoadNode(previousClosestRoadNode, previousClosestRoadNodeHeading))
                {
                    //Game.LogTrivial($"Previous roadnode is on opposite side");
                    if (!NearestRoadNodeToPedIsValid(previousClosestRoadNode))
                    {
                        continue;
                    }
                }
                if (previousClosestRoadNode != closestRoadNode && previousFlags != 134 && ped.WithinNearbyRoadNode(previousClosestRoadNode, previousClosestRoadNodeHeading))
                {
                    //Game.LogTrivial($"Found previous node");
                    RoadPoints[1].PreviousNode = new RoadNode(previousClosestRoadNode, previousClosestRoadNodeHeading);
                    return;
                }
            }
        }

        private void Process()
        {
            TransitionToState(State.Running);
            //GameFiber.StartNew(() => UpdateProstituteRoadNodes(), "RPE Update Prostitute Road Nodes Fiber");
            GameFiber.StartNew(() => CheckEndConditions(), "RPE End Conditions Fiber");

            CollectJohn();

            if (State == State.Ending)
            {
                return;
            }

            GiveJohnPulloverTasks();

            if (State == State.Ending)
            {
                return;
            }

            RunProstituteAnimations();

            if (State == State.Ending)
            {
                return;
            }

            // John drives to secluded area (will have to be pre-defined) or cruises

            // Peds have sex OR JOHN MURDERS HOOKER!  SICK IDEA!!!!

        }

        private void UpdateProstituteRoadNodes()
        {
            while (State != State.Ending && _prostitute && !_john)
            {
                GameFiber.Yield();

                if (_prostitute && _prostitute.Speed < 1f && (RoadPoints[0] != null || RoadPoints[2] != null))
                {
                    DrawMarkers(RoadPoints);
                    continue;
                }

                if (_prostitute && NearestRoadNodeToPedIsValid(_prostitute.Position))
                {
                    RoadPoints[1] = new RoadNode(_roadNode, _roadNodeHeading, RoadNodeIsAlley(_roadNodeFlags));
                    GetFirstUsablePreviousRoadNode(_prostitute, RoadPoints[1].Position, RoadPoints[1].Heading);
                    RoadPoints[0] = RoadPoints[1].PreviousNode;
                    DrawMarkers(RoadPoints);
                }
            }
        }

        private static void DrawMarkers(List<RoadNode> roadPoints)
        {
            foreach (RoadNode roadPoint in roadPoints.Where(x => x != null))
            {
                NativeWrappers.DrawMarker(1, roadPoint.Position, 0, 0, 0, 0, 0, 0, 1f, 1f, 3f, 255, 255, 65, 100, false, false, false, false);
                if (World.GetAllVehicles().Any(x => x.DistanceTo2D(roadPoint.RoadSidePosition) <= 5))
                {
                    NativeWrappers.DrawMarker(1, roadPoint.RoadSidePosition, 0, 0, 0, 0, 0, 0, 1f, 1f, 3f, 255, 65, 65, 100, false, false, false, false);
                    //NativeMethods.DrawMarker(1, _pulloverPosition, 0, 0, 0, 0, 0, 0, 1f, 1f, 3f, 65, 255, 65, 100, false, false, false, false);
                }
                else if (roadPoints.IndexOf(roadPoint) == 0)
                {
                    NativeWrappers.DrawMarker(1, roadPoint.RoadSidePosition, 0, 0, 0, 0, 0, 0, 1f, 1f, 3f, 65, 65, 255, 100, false, false, false, false);
                }
                else if (roadPoints.IndexOf(roadPoint) == 2)
                {
                    NativeWrappers.DrawMarker(1, roadPoint.RoadSidePosition, 0, 0, 0, 0, 0, 0, 1f, 1f, 3f, 255, 65, 255, 100, false, false, false, false);
                }
                else
                {
                    NativeWrappers.DrawMarker(1, roadPoint.RoadSidePosition, 0, 0, 0, 0, 0, 0, 1f, 1f, 3f, 65, 255, 65, 100, false, false, false, false);
                }
            }
        }

        private void CollectJohn()
        {
            var collectionNode = RoadPoints.First(y => y != null).RoadSidePosition;
            while (State != State.Ending && !_john)
            {
                GameFiber.Yield();
                var johnVehicle = World.GetAllVehicles().FirstOrDefault(x => x && x != Game.LocalPlayer.Character.CurrentVehicle && x.IsCar && x.HasDriver && x.Driver && x.Driver.IsAlive && x.FreeSeatsCount > 0 && x.IsSeatFree(0) && x.DistanceTo2D(collectionNode) <= 5); ;
                if (!johnVehicle)
                {
                    //Game.LogTrivial($"No suitable pullover vehicle found.");
                    continue;
                }

                _john = new EventPed(johnVehicle.Driver, Role.SecondarySuspect, this, false);
                Game.LogTrivial($"Pullover vehicle collected at {collectionNode}, prostitute's position is {_prostitute.Position}");
                _johnFound = true;
                break;
            }
        }

        private void GiveJohnPulloverTasks()
        {
            if (RoadPoints[_indexOfNodeToDriveTo].IsAlley)
            {
                DriveToAlleyPosition();
            }
            else
            {
                _john.Tasks.DriveToPosition(_pulloverPosition, 5f, VehicleDrivingFlags.Normal, 1f).WaitForCompletion();
            }

            if (State == State.Ending)
            {
                return;
            }

            // Straighten the vehicle if necessary
            if (_john && _john.CurrentVehicle && !RoadPoints[1].IsAlley && GetHeadingAbsDifference(_john.CurrentVehicle.Heading, RoadPoints[_indexOfNodeToDriveTo].Heading) > 5f)
            {
                StraightenVehicle(_indexOfNodeToDriveTo);
            }

            if (State == State.Ending)
            {
                return;
            }

            CompletePullover();
        }

        private void DriveToAlleyPosition()
        {
            // Drive to the roadside position because the road position is in the middle of the road.
            _john.Tasks.DriveToPosition(RoadPoints[_indexOfNodeToDriveTo - 1].RoadSidePosition, 5f, VehicleDrivingFlags.FollowTraffic, 2f).WaitForCompletion();
            if (State == State.Ending)
            {
                return;
            }

            _john.Tasks.DriveToPosition(RoadPoints[_indexOfNodeToDriveTo].Position, 5f, VehicleDrivingFlags.FollowTraffic, 1f).WaitForCompletion();
        }

        private bool UsablePulloverPositionFound(int indexOfNodeToDriveTo)
        {
            Vector3 pulloverPosition = new Vector3(0, 0, 0);
            for (int i = 0; i <= 15; i += 5)
            {
                if (World.GetAllVehicles().Any(x => x.DistanceTo2D(RoadPoints[indexOfNodeToDriveTo].RoadSidePosition.GetOffset(x.Heading, new Vector3(0, i, 0))) <= 5))
                {
                    continue;
                }
                else
                {
                    _pulloverPosition = RoadPoints[indexOfNodeToDriveTo].RoadSidePosition.GetOffset(RoadPoints[indexOfNodeToDriveTo].Heading, new Vector3(0, i, 0));
                    return true;
                }
            }

            Game.LogTrivial($"No suitable pullover position, cars in the way.");
            return false;

        }

        private void StraightenVehicle(int indexOfNodeToDriveTo)
        {
            _john.CurrentVehicle.TopSpeed = 3f;
            _john.CurrentVehicle.SteeringAngle = 15f;
            _john.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardWithCustomSteeringAngle);

            GameFiber.SleepWhile(() => _john && _john.CurrentVehicle && GetHeadingAbsDifference(_john.CurrentVehicle.Heading, RoadPoints[indexOfNodeToDriveTo].Heading) > 2f, 10000);
        }

        private static float GetHeadingAbsDifference(float heading1, float heading2)
        {
            float heading = MathHelper.NormalizeHeading(MathHelper.NormalizeHeading(heading1) - MathHelper.NormalizeHeading(heading2));
            return Math.Min(Math.Abs(360 - heading), heading);
        }

        private void CompletePullover()
        {
            _john.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
            _john.CurrentVehicle.TopSpeed = 30f;
            Rage.Native.NativeFunction.Natives.ROLL_DOWN_WINDOW(_john.CurrentVehicle, 1);
            GameFiber.SleepWhile(() => _john.Speed > 0f, 7000);
            _john.CurrentVehicle.HonkHorn(1000);
            _john.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
            GameFiber.Yield();
        }

        private void RunProstituteAnimations()
        {
            Game.LogTrivial($"Running animations on prostitute.");
            _prostitute.Tasks.Clear();
            //var desiredHeading = (_john.CurrentVehicle.Heading + 90f).Normalize();
            _prostitute.Tasks.GoStraightToPosition(_john.GetOffsetPosition(new Vector3(1.5f, 1f, 0)), 1f, _prostitute.Heading, 1f, 12000).WaitForCompletion(); //(float)Math.Truncate(desiredHeading)
            if (State == State.Ending)
            {
                return;
            }

            var animationDuration = 10000;
            _prostitute.FaceEntity(_john, animationDuration);
            _prostitute.Tasks.PlayAnimation("amb@prop_human_bum_shopping_cart@male@idle_a", "idle_b", animationDuration, 1f, 1f, 0f, AnimationFlags.Loop).WaitForCompletion();
            if (State == State.Ending)
            {
                return;
            }

            _prostitute.Tasks.EnterVehicle(_john.CurrentVehicle, 0).WaitForCompletion();
            if (State == State.Ending)
            {
                return;
            }

            _john.Tasks.Clear();
            _john.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Normal);
            _john.Dismiss();
        }

        private void CheckEndConditions()
        {
            _oldDistance = Game.LocalPlayer.Character.DistanceTo2D(_prostitute);

            while (State != State.Ending)
            {
                GameFiber.Yield();

                if (!_prostitute)
                {
                    Game.LogTrivial($"[Rich Ambiance]: Prostitute is null.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (!_prostitute.IsAlive)
                {
                    Game.LogTrivial($"[Rich Ambiance]: Prostitute is dead.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(_prostitute) > 150f)
                {
                    Game.LogTrivial($"[Rich Ambiance]: Player is too far away.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(_prostitute.Position) < 10f)
                {
                    _prostitute.Tasks.Clear();
                    Game.LogTrivial($"[Rich Ambiance]: Player is near prostitute, triggering response.");
                    if (new Random().Next(10) < 2)
                    {
                        StartPursuit();
                    }
                    else
                    {
                        _prostitute.Tasks.Wander();

                        //foreach (Blip blip in AmbientEvents.ActiveEvent.EventBlips.Where(b => b))
                        foreach (Blip blip in AmbientEvents.ActiveEvents.First(x => x == this).EventBlips.Where(b => b))
                        {
                            while (blip && blip.Alpha > 0)
                            {
                                blip.Alpha -= 0.01f;
                                GameFiber.Yield();
                                if (State == State.Ending)
                                {
                                    return;
                                }
                            }
                            if (blip)
                            {
                                blip.Delete();
                            }
                        }
                    }

                    TransitionToState(State.Ending);
                    return;
                }

                if (_johnFound)
                {
                    if (!_john)
                    {
                        Game.LogTrivial($"[Rich Ambiance]: John is null.  Ending event.");
                        TransitionToState(State.Ending);
                        return;
                    }

                    if (!_john.IsAlive || !_john.CurrentVehicle)
                    {
                        Game.LogTrivial($"[Rich Ambiance]: John is dead or vehicle is invalid.  Ending event.");
                        TransitionToState(State.Ending);
                        return;
                    }

                    if (Game.LocalPlayer.Character.DistanceTo2D(_john.Position) < 10f)
                    {
                        _john.Tasks.Clear();
                        Game.LogTrivial($"[Rich Ambiance]: Player is near John, triggering wander.");
                        _john.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Normal);
                        TransitionToState(State.Ending);
                        return;
                    }

                    if (_john.DistanceTo2D(_prostitute) > 20f)
                    {
                        Game.LogTrivial($"[Rich Ambiance]: John is too far from prostitute.");
                        TransitionToState(State.Ending);
                        return;
                    }

                    //if (_prostitute.CurrentVehicle == _john.CurrentVehicle && AmbientEvents.ActiveEvent.EventBlips.Count > 0)
                    if (_prostitute.CurrentVehicle == _john.CurrentVehicle && AmbientEvents.ActiveEvents.First(x => x == this).EventBlips.Count > 0)
                    {
                        GameFiber.Sleep(10);
                        //foreach (Blip blip in AmbientEvents.ActiveEvent.EventBlips.Where(b => b))
                        foreach (Blip blip in AmbientEvents.ActiveEvents.First(x => x == this).EventBlips.Where(b => b))
                        {
                            while (blip && blip.Alpha > 0)
                            {
                                blip.Alpha -= 0.01f;
                                GameFiber.Yield();
                                if (State == State.Ending)
                                {
                                    return;
                                }
                            }
                            if (blip)
                            {
                                blip.Delete();
                            }
                        }
                    }
                }

                FadeBlips();
            }
        }

        private void FadeBlips()
        {
            if (Settings.EventBlips && _prostitute.Blip)
            {
                if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(_prostitute) - _oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(_prostitute) > _oldDistance && _prostitute.Blip.Alpha > 0f)
                {
                    _prostitute.Blip.Alpha -= 0.001f;
                }
                else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(_prostitute) - _oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(_prostitute) < _oldDistance && _prostitute.Blip.Alpha < 1.0f)
                {
                    _prostitute.Blip.Alpha += 0.01f;
                }
                _oldDistance = Game.LocalPlayer.Character.DistanceTo2D(_prostitute);
            }
        }

        private void StartPursuit()
        {
            //foreach (Blip blip in AmbientEvents.ActiveEvent.EventBlips)
            //{
            //    blip.Delete();
            //}

            var pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(pursuit, _prostitute);
            if (_john && _prostitute.CurrentVehicle && _prostitute.CurrentVehicle.Driver == _john)
            {
                Functions.AddPedToPursuit(pursuit, _john);
            }
            else if (_john && new Random().Next(10) < 3)
            {
                Functions.AddPedToPursuit(pursuit, _john);
            }
            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
            Game.LogTrivial($"[Rich Ambiance]: Pursuit initiated successfully");
        }
    }
}
