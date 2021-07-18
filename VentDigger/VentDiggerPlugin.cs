using System;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;

namespace VentDigger
{
    [BepInPlugin("com.zirno.ventdigger", "Vent Digger", version)]
    [BepInProcess("Among Us.exe")]
    public class VentDiggerPlugin : BasePlugin
    {
        public const string version  = "2.1.0";
        public override void Load()
        {
            Harmony.CreateAndPatchAll(typeof(VentDiggerPlugin).Assembly);
        }
    }
}
