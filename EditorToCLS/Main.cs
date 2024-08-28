using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;

public class Main
{
    private static Harmony harmony;

    public static bool Start(ModEntry modEntry)
    {
        modEntry.OnToggle = (entry, value) =>
        {
            if (value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony?.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        };

        modEntry.OnUpdate = OnUpdate;

        return true;
    }

    private static void OnUpdate(ModEntry modEntry, float deltaTime)
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadCustomLevelFromScnGame();
        }
    }

    private static void LoadCustomLevelFromScnGame()
    {
        scnGame gameInstance = Object.FindObjectOfType<scnGame>();
        if (gameInstance == null) return;

        string levelPath = gameInstance.levelPath;
        if (string.IsNullOrEmpty(levelPath)) return;

        scrController controllerInstance = Object.FindObjectOfType<scrController>();
        if (controllerInstance == null) return;

        controllerInstance.LoadCustomLevel(levelPath, "");
    }

    [HarmonyPatch(typeof(scrController), "LoadCustomLevel")]
    public static class LoadCustomLevel_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(scrController __instance, ref string levelPath, ref string levelId)
        {
        }
    }
}
