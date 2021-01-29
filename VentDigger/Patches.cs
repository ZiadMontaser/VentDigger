using HarmonyLib;
using System.Linq;
using UnityEngine;
using Hazel;
using Vent = OPPMFCFACJB;
using ShipStatus = HLBNNHFCNAJ;
using PlayerControl = FFGALNAPKCD;
using AmongUsClient = FMLLKEACGIO;
using HudManager = PIEFJFEOGOL;

namespace VentDigger
{
    class Patches
    {
        const byte SpawnVentCallId = 62;

        public static Button DigButton;

        public static Vent lastVent;

        static void OnDigPressed()
        {
            var pos = PlayerControl.LocalPlayer.transform.position;
            int ventId = 0;
            int ventLeft = int.MaxValue;
            int ventCrnter = int.MaxValue;
            int ventRight = int.MaxValue;

            if (lastVent != null)
            {
                ventId = lastVent.Id + 1;
                ventLeft = lastVent.Id;
            }

            RpcSpawnVent(ventId , pos , ventLeft , ventCrnter , ventRight);
        }

        static void SpawnVent(int id ,Vector2 postion, int leftVent , int centerVent , int rightVent)
        {

            var ventPref = GameObject.FindObjectOfType<Vent>();
            var vent = GameObject.Instantiate<Vent>(ventPref, ventPref.transform.parent);
            vent.Id = id;
            vent.transform.position = postion;
            vent.Left = leftVent == int.MaxValue ? null : ShipStatus.Instance.CIAHFBANKDD[leftVent];
            vent.Center = centerVent == int.MaxValue ? null : ShipStatus.Instance.CIAHFBANKDD[centerVent];
            vent.Right = rightVent == int.MaxValue ? null : ShipStatus.Instance.CIAHFBANKDD[rightVent];

            Vent[] vents = new Vent[ShipStatus.Instance.CIAHFBANKDD.Count+1];
            for(int i = 0; i < ShipStatus.Instance.CIAHFBANKDD.Count; i++)
            {
                vents[i] = ShipStatus.Instance.CIAHFBANKDD[i];
            }
            vents[id] = vent;

            ShipStatus.Instance.CIAHFBANKDD = vents;

            if (lastVent != null)
            {
                lastVent.Right = ShipStatus.Instance.CIAHFBANKDD.FirstOrDefault(v => v.Id == id);
            }

            lastVent = vent;
        }

        static void RpcSpawnVent(int id, Vector2 postion, int leftVent, int centerVent, int rightVent)
        {

            SpawnVent(id, postion, leftVent, centerVent, rightVent);

           var w =  AmongUsClient.Instance.StartRpc(ShipStatus.Instance.NetId, SpawnVentCallId, SendOption.Reliable);

            w.WritePacked(id);
            w.WriteVector2(postion);
            w.WritePacked(leftVent); //Left
            w.WritePacked(centerVent); //Center
            w.WritePacked(rightVent); //Right
            w.EndMessage();

        }


        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class HandleThePLaceButton
        {
            static void Prefix(PlayerControl __instance)
            {
                if (PlayerControl.LocalPlayer != __instance) return;
                if(PlayerControl.LocalPlayer.JLGGIOLCDFC.DAPKNDBLKIA && !PlayerControl.LocalPlayer.JLGGIOLCDFC.DLPCKPBIJOE)
                {
                        DigButton.buttonManager.gameObject.SetActive(true);
                }
                else
                {
                        DigButton.buttonManager.gameObject.SetActive(false);

                }
            }
        }

        [HarmonyPatch(typeof(ShipStatus) , nameof(ShipStatus.HandleRpc))]
        static class ShipstatusHandleRpcPatch
        {
            static bool Prefix(ShipStatus __instance, byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
            {
                if (HKHMBLJFLMC == SpawnVentCallId) {
                    var reader = ALMCIJKELCP;
                    var id = reader.ReadPackedInt32();
                    var postion = reader.ReadVector2();
                    var leftVent = reader.ReadPackedInt32();
                    var centerVent = reader.ReadPackedInt32();
                    var rightVent = reader.ReadPackedInt32();
                    SpawnVent(
                        id: id,
                        postion: postion,
                        leftVent: leftVent,
                        centerVent: centerVent,
                        rightVent: rightVent
                        );
                    return false;
            }
            return true;
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

                        continue;
                    }
                    GameObject.Destroy(vent.gameObject);
                }
                __instance.CIAHFBANKDD = new Vent[0];
            }
        }
    }
}
