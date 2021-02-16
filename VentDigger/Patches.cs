using HarmonyLib;
using System.Linq;
using UnityEngine;
using Hazel;
using Reactor.Extensions;
using Reactor;

namespace VentDigger
{
    class Patches
    {
        const byte SpawnVentCallId = 62;

        public static Button DigButton;

        public static Vent lastVent;
        static Vector2 VentSize;

        private static void OnDigPressed()
        {
            var pos = PlayerControl.LocalPlayer.transform.position;
            var ventId = GetAvailableVentId();
            var ventLeft = int.MaxValue;
            var ventCenter = int.MaxValue;
            var ventRight = int.MaxValue;

            if (lastVent != null)
            {
                ventLeft = lastVent.Id;
            }

            RpcSpawnVent(ventId, pos, pos.z + .001f, ventLeft, ventCenter, ventRight);
        }

        static int GetAvailableVentId()
        {
            int id = 0;

            while (true)
            {
                if (!ShipStatus.Instance.AllVents.Any(v => v.Id == id))
                {
                    return id;
                }
                id++;
            }
        }

        private static void SpawnVent(PlayerControl sender, int id, Vector2 position,float zAxis, int leftVent, int centerVent, int rightVent)
        {
            var realPos = new Vector3(position.x, position.y, zAxis);

            var ventPref = Object.FindObjectOfType<Vent>();
            var vent = Object.Instantiate(ventPref, ventPref.transform.parent);
            vent.Id = id;
            vent.transform.position = realPos;
            vent.Left = leftVent == int.MaxValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == leftVent);
            vent.Center = centerVent == int.MaxValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == centerVent);
            vent.Right = rightVent == int.MaxValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == rightVent);

            var allVents = ShipStatus.Instance.AllVents.ToList();
            allVents.Add(vent);
            ShipStatus.Instance.AllVents = allVents.ToArray();

            if (vent.Left != null)
            {
                vent.Left.Right = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == id);
            }

            if(sender.AmOwner)
                lastVent = vent;
        }

        private static void RpcSpawnVent(int id, Vector2 position, float zAxis, int leftVent, int centerVent, int rightVent)
        {

            SpawnVent(PlayerControl.LocalPlayer, id, position, zAxis, leftVent, centerVent, rightVent);

            var w = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)SpawnVentCallId, SendOption.Reliable);

            w.WritePacked(id);
            w.Write(position);
            w.Write(zAxis);
            w.WritePacked(leftVent); //Left
            w.WritePacked(centerVent); //Center
            w.WritePacked(rightVent); //Right
            w.EndMessage();

        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class HudManagerUpdatePatch
        {
            static void Postfix()
            {
                if (!PlayerControl.LocalPlayer) return;

                var hits = Physics2D.OverlapBoxAll(PlayerControl.LocalPlayer.transform.position, VentSize, 0);
                hits = hits.ToArray().Where((c) => (c.name.Contains("Vent") || !c.isTrigger) && c.gameObject.layer != 8 && c.gameObject.layer != 5).ToArray();
                if (hits.Count() != 0)
                {
                    DigButton.buttonManager.renderer.color = Palette.DisabledColor;
                    DigButton.CanPlace = false;
                }
                else
                {
                    DigButton.buttonManager.renderer.color = Palette.EnabledColor;
                    DigButton.CanPlace = true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class HandleThePLaceButton
        {
            static void Prefix(PlayerControl __instance)
            {
                if (!__instance.AmOwner) return;
                if (PlayerControl.LocalPlayer.Data.IsImpostor && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    DigButton.buttonManager.gameObject.SetActive(true);
                }
                else
                {
                    DigButton.buttonManager.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        private static class PlayerControlHandleRpcPatch
        {
            private static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader messageReader)
            {
                if (callId != (byte)SpawnVentCallId) return true;
                var reader = messageReader;
                var id = reader.ReadPackedInt32();
                var position = reader.ReadVector2();
                var zAxis = reader.ReadSingle();
                var leftVent = reader.ReadPackedInt32();
                var centerVent = reader.ReadPackedInt32();
                var rightVent = reader.ReadPackedInt32();
                SpawnVent(
                    sender: __instance,
                    id: id,
                    position: position,
                    zAxis: zAxis,
                    leftVent: leftVent,
                    centerVent: centerVent,
                    rightVent: rightVent
                );
                return false;
            }
        }

        [HarmonyPatch(typeof(HudManager),nameof(HudManager.Start))]
        static class DigButtonPatch
        {
            static void Postfix(HudManager __instance)
            {
                DigButton = new Button(__instance, "VentDigger.Assets.DigImpostor.png");
                DigButton.AddListener(OnDigPressed);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        static class ShipstatusOnEnablePatch
        {
            static void Prefix(ShipStatus __instance)
            {
                var vents = GameObject.FindObjectsOfType<Vent>();
                bool first = true;
                foreach (var vent in vents)
                {
                    if (first)
                    {
                        first = false;
                        vent.transform.position = new Vector2(50, 50);
                        VentSize = Vector2.Scale(vent.GetComponent<BoxCollider2D>().size, vent.transform.localScale) * 0.75f;
                        continue;
                    }
                    GameObject.Destroy(vent.gameObject);
                }
                __instance.AllVents = new Vent[0];
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        static class PingTrackerPatches
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.gameObject.SetActive(true);
                __instance.text.Text += "\n\n >>Made by [FF0000FF]Zirno#9723";
            }
        }
    }
}
