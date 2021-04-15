using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;
using System;
using RichAmbiance.Utils;

namespace RichAmbiance.AmbientEvents
{
    using RichAmbiance.Features;

    internal class DrugDeal : AmbientEvent
    {
        private List<Ped> _usablePeds;
        private EventPed _suspect, _buyer;
        private float _oldDistance;

        internal DrugDeal()
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

        internal void Prepare()
        {
            TransitionToState(State.Preparing);
            FindEventPeds();
        }

        private void FindEventPeds()
        {
            for (int i = 0; i < 100; i++)
            {
                GameFiber.Sleep(500);
                if (GuardClauses.CalloutOrPursuitActive())
                {
                    TransitionToState(State.Ending);
                    return;
                }

                if (GuardClauses.EventPedsFound(EventPeds, 2, i))
                {
                    return;
                }

                GetUsablePeds();
                SelectPedPair();
            }

            Game.LogTrivial($"[RPE Ambient Event]: Unable to find suitable event peds after 100 attempts.  Ending event.");
            TransitionToState(State.Ending);
            return;
        }

        private void GetUsablePeds() => _usablePeds = HelperMethods.GetReleventPedsForAmbientEvent().Where(p => p.IsOnFoot).ToList();

        private void SelectPedPair()
        {
            foreach (Ped ped in _usablePeds.Where(p => p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican))
            {
                var victim = _usablePeds.FirstOrDefault(p => p != ped && Math.Abs(ped.Position.Z - p.Position.Z) <= 3f && p.DistanceTo2D(ped) <= 10f);
                if (victim)
                {
                    _suspect = new EventPed(ped, Role.PrimarySuspect, true);
                    _buyer = new EventPed(victim, Role.SecondarySuspect, false);
                    return;
                }
            }
        }

        private void Process()
        {
            TransitionToState(State.Running);
            GameFiber.StartNew(() => CheckEndConditions(), "RPE End Conditions Fiber");

            AssignDealerTasks();
            AssignBuyerTasks();

            if (State == State.Ending)
            {
                return;
            }

            AssignInteractionTasks();
        }

        private void AssignDealerTasks()
        {
            _suspect.FaceEntity(_buyer, -1);
            if (_suspect.RelationshipGroup.Name == "AMBIENT_GANG_BALLAS" || _suspect.RelationshipGroup.Name == "AMBIENT_GANG_FAMILY")
            {
                _suspect.PlayAmbientSpeech("A_M_M_SOUCENT_01_BLACK_FULL_01", "GREET_ACROSS_STREET", 0, SpeechModifier.ForceShouted);
            }
            else
            {
                _suspect.PlayAmbientSpeech("A_M_M_EASTSA_02_LATINO_FULL_01", "GREET_ACROSS_STREET", 0, SpeechModifier.ForceShouted);
            }
            _suspect.Tasks.PlayAnimation("friends@frj@ig_1", "wave_c", 1, AnimationFlags.None);
        }

        private void AssignBuyerTasks()
        {
            _buyer.Tasks.GoToOffsetFromEntity(_suspect, 1.0f, 0, 2.0f).WaitForCompletion();
            if (State == State.Ending)
            {
                return;
            }

            _buyer.FaceEntity(_suspect, -1);
        }

        private void AssignInteractionTasks()
        {
            if (_buyer.RelationshipGroup == RelationshipGroup.AmbientGangMexican)
            {
                _buyer.PlayAmbientSpeech("A_M_Y_MEXTHUG_01_LATINO_FULL_01", "GENERIC_BUY", 0, SpeechModifier.Force);
            }
            else
            {
                _buyer.PlayAmbientSpeech("A_M_M_SOUCENT_01_BLACK_FULL_01", "GENERIC_BUY", 0, SpeechModifier.Force);
            }
            _buyer.Tasks.PlayAnimation("amb@world_human_bum_standing@twitchy@idle_a", "idle_a", 1, AnimationFlags.Loop);
            _suspect.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_b", "idle_d", 1, AnimationFlags.Loop);

            //Blip BuyerBlip = buyer.Ped.AttachBlip();
            //BuyerBlip.Scale = 0.75f;
            //BuyerBlip.Color = Color.White;
        }

        private void CheckEndConditions()
        {
            _oldDistance = Game.LocalPlayer.Character.DistanceTo2D(_suspect);

            while (State == State.Running)
            {
                GameFiber.Yield();
                if (_suspect == null || _buyer == null || !_suspect || !_buyer || !_suspect.IsAlive || !_buyer.IsAlive || Functions.IsPedGettingArrested(_suspect) || Functions.IsPedGettingArrested(_buyer))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Suspect or victim is null or dead, or driver is arrested.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(_suspect) > 150f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (_suspect.DistanceTo2D(_buyer) > 50f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Victim got away from suspect.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Functions.GetActivePursuit() != null && (Functions.IsPedInPursuit(_suspect) || Functions.IsPedInPursuit(_buyer)))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is in pursuit of suspect.  Ending event.");
                    TransitionToState(State.Ending);
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(_suspect.Position) < 10f || Game.LocalPlayer.Character.DistanceTo2D(_buyer.Position) < 10f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is near prostitute, triggering response.");
                    _suspect.Tasks.Clear();
                    _buyer.Tasks.Clear();

                    if (new Random().Next(10) < 2)
                    {
                        StartPursuit();
                    }
                    else
                    {
                        _suspect.Tasks.Wander();
                        _buyer.Tasks.Wander();

                        foreach (Blip blip in AmbientEvents.ActiveEvent.EventBlips.Where(b => b))
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

                FadeBlips();
            }
        }

        private void FadeBlips()
        {
            if (Settings.EventBlips && _suspect.Blip)
            {
                if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(_suspect) - _oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(_suspect) > _oldDistance && _suspect.Blip.Alpha > 0f)
                {
                    _suspect.Blip.Alpha -= 0.001f;
                }
                else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(_suspect) - _oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(_suspect) < _oldDistance && _suspect.Blip.Alpha < 1.0f)
                {
                    _suspect.Blip.Alpha += 0.01f;
                }
                _oldDistance = Game.LocalPlayer.Character.DistanceTo2D(_suspect);
            }
        }

        private void StartPursuit()
        {
            foreach (Blip blip in AmbientEvents.ActiveEvent.EventBlips)
            {
                blip.Delete();
            }

            var pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(pursuit, _suspect);
            Functions.AddPedToPursuit(pursuit, _buyer);
            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
            Game.LogTrivial($"[RPE Ambient Event]: Pursuit initiated successfully");
        }
    }
}
