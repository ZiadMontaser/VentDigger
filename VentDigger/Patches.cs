using HarmonyLib;
using System.Linq;
using AdminLog.Utils;
using UnityEngine;
using Hazel;
using UnhollowerBaseLib;

namespace VentDigger
{
    class Patches
    {
        const byte RpcSpawnVentCallId = 40;
        const byte CmdSpawnVentCallId = 41;

        public static CooldownButton DigButton;

        public static Vent lastVent;
        static Vector2 VentSize;

        private static bool OnDigPressed()
        {
            var pos = PlayerControl.LocalPlayer.transform.position;
            var ventLeft = int.MaxValue;
            var ventCenter = int.MaxValue;
            var ventRight = int.MaxValue;

            if (lastVent != null)
            {
                ventLeft = lastVent.Id;
            }

            CmdSpawnVent(PlayerControl.LocalPlayer,pos, ventLeft, ventCenter, ventRight);

            return true;
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

        private static void CmdSpawnVent(PlayerControl spawnedBy, Vector2 position, int leftVent, int centerVent, int rightVent)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                RpcSpawnVent(spawnedBy, GetAvailableVentId(), position, leftVent, centerVent, rightVent);
            }
            else
            {
                var w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CmdSpawnVentCallId,
                    SendOption.Reliable, AmongUsClient.Instance.HostId);
                w.Write(position.x);
                w.Write(position.y);
                w.WritePacked(leftVent); //Left
                w.WritePacked(centerVent); //Center
                w.WritePacked(rightVent); //Right
                AmongUsClient.Instance.FinishRpcImmediately(w);

            }
        }

        private static void RpcSpawnVent(PlayerControl spawnedBy,int id, Vector2 position, int leftVent, int centerVent, int rightVent)
        {
            lastVent = SpawnVent(spawnedBy,id, position, leftVent, centerVent, rightVent);

            var w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) RpcSpawnVentCallId,
                SendOption.Reliable, -1);
            w.Write(spawnedBy.PlayerId);
            w.WritePacked(id);
            w.Write(position.x);
            w.Write(position.y);
            w.WritePacked(leftVent); //Left
            w.WritePacked(centerVent); //Center
            w.WritePacked(rightVent); //Right
            AmongUsClient.Instance.FinishRpcImmediately(w);
        }
        
        private static Vent SpawnVent(PlayerControl spawnedBy, int id, Vector2 position, int leftVent, int centerVent, int rightVent)
        {
            var realPos = new Vector3(position.x, position.y, position.y / 1000 + 0.001f);

            var ventPref = ShipStatus.Instance.AllVents[0];
            var vent = Object.Instantiate(ventPref, ventPref.transform.parent);
            vent.Id = id;
            vent.transform.position = realPos;
            vent.Left = leftVent == int.MaxValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == leftVent);
            vent.Center = centerVent == int.MaxValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == centerVent);
            vent.Right = rightVent == int.MaxValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == rightVent);
            
            vent.gameObject.SetActive(true);
            
            var allVents = ShipStatus.Instance.AllVents.ToList();
            allVents.Add(vent);
            ShipStatus.Instance.AllVents = allVents.ToArray();

            if (vent.Left != null)
            {
                vent.Left.Right = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == id);
            }

            if (spawnedBy.AmOwner) lastVent = vent;

            return vent;
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class HudManagerUpdatePatch
        {
            static void Postfix()
            {
                if (!PlayerControl.LocalPlayer || !DigButton.IsAvailable) return;
                
                var circleHits = Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.transform.position, VentSize.y, Constants.ShipAndAllObjectsMask);
                circleHits = circleHits.ToArray().Where((c) => (c.name.Contains("Vent") || !c.isTrigger) && c.gameObject.layer != 8 && c.gameObject.layer != 5).ToArray();
                if (circleHits.Count() != 0)
                {
                    DigButton.CanUse = false;
                }
                else
                {
                    DigButton.CanUse = true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetInfected))]
        class HandleThePLaceButton
        {
            static void Postfix([HarmonyArgument(0)] Il2CppStructArray<byte> infected)
            {
                if (infected.Contains(PlayerControl.LocalPlayer.PlayerId))
                {
                    DigButton.OnClick = OnDigPressed;
                }
                else
                {
                    DigButton.OnClick = null;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        private static class PlayerControlHandleRpcPatch
        {
            private static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader messageReader)
            {
                if (callId == CmdSpawnVentCallId)
                {
                    var reader = messageReader;
                    var x = reader.ReadSingle();
                    var y = reader.ReadSingle();
                    var leftVent = reader.ReadPackedInt32();
                    var centerVent = reader.ReadPackedInt32();
                    var rightVent = reader.ReadPackedInt32();

                    var pos = new Vector3(x, y, y / 1000 + 0.001f);

                    CmdSpawnVent(
                        spawnedBy: __instance,
                        position: pos,
                        leftVent: leftVent,
                        centerVent: centerVent,
                        rightVent: rightVent
                    );
                    return false;
                }else if (callId == RpcSpawnVentCallId)
                {
                    var reader = messageReader;
                    var spawnedBy = reader.ReadByte();
                    var id = reader.ReadPackedInt32();
                    var x = reader.ReadSingle();
                    var y = reader.ReadSingle();
                    var leftVent = reader.ReadPackedInt32();
                    var centerVent = reader.ReadPackedInt32();
                    var rightVent = reader.ReadPackedInt32();

                    var pos = new Vector3(x, y, y / 1000 + 0.001f);

                    SpawnVent(
                        spawnedBy: GameData.Instance.GetPlayerById(spawnedBy).Object,
                        id: id,
                        position: pos,
                        leftVent: leftVent,
                        centerVent: centerVent,
                        rightVent: rightVent
                    );
                    return false;
                }
                
                
                return true;
            }
        }
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        static class DigButtonPatch
        {
            [HarmonyPostfix]
            static void Postfix(HudManager __instance)
            {
                DigButton = new CooldownButton("DigButton");
                DigButton.InitCooldown = 5;
                DigButton.NormalCooldown = 15;
                DigButton.Sprite = SpriteHelper.LoadSpriteFromEmbeddedResources("VentDigger.Assets.DigImpostor.png", 700);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        static class ShipstatusOnEnablePatch
        {
            static void Prefix(ShipStatus __instance)
            {
                var vents = ShipStatus.Instance.AllVents;
                for (var i = 1; i < vents.Count; i++)
                {
                    GameObject.Destroy(vents[i].gameObject);
                }

                vents[0].Id = -1;
                vents[0].gameObject.SetActive(false);
                VentSize = Vector2.Scale(vents[0].GetComponent<BoxCollider2D>().size, vents[0].transform.localScale) * 0.75f;

                __instance.AllVents = new Vent[]{vents[0]};
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        static class PingTrackerPatches
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.gameObject.SetActive(true);
                __instance.text.text += "\n\n >>Made by Zirno#9723";
            }
        }

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        static class VersionShowerPatches
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.text += $" || <color=\"red\"> VentDigger {VentDiggerPlugin.version} </color>";
            }
        }
    }
}
