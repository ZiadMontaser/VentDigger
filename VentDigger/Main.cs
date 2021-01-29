using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace VentDigger
{
    [BepInPlugin("com.zirno.ventdigger", "Vent Digger", "1.0.0")]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
