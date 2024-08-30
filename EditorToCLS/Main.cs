using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using static UnityModManagerNet.UnityModManager;

public class Main
{
    private static Harmony harmony;
    private static KeyCode keyCode = KeyCode.F3;
    private static KeyCode keyCodeWithNoFailMod = KeyCode.F4;
    private static KeybindOption inputAwaitingOption = KeybindOption.None;
    private static string settingsFilePath;

    private enum KeybindOption
    {
        None,
        OpenLevel,
        OpenLevelWithNoFail
    }

    public static bool Start(ModEntry modEntry)
    {
        string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        settingsFilePath = Path.Combine(dllPath, "settings.xml");

        LoadSettings();

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
        modEntry.OnGUI = OnGUI;

        return true;
    }

    private static void OnUpdate(ModEntry modEntry, float deltaTime)
    {
        if (inputAwaitingOption != KeybindOption.None && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    if (inputAwaitingOption == KeybindOption.OpenLevel)
                    {
                        keyCode = key;
                    }
                    else if (inputAwaitingOption == KeybindOption.OpenLevelWithNoFail)
                    {
                        keyCodeWithNoFailMod = key;
                    }
                    inputAwaitingOption = KeybindOption.None;
                    SaveSettings();
                    break;
                }
            }
        }

        if (Input.GetKeyDown(keyCode))
        {
            LoadCustomLevelFromScnGame();
            SetUseNoFail(false);
        }

        if (Input.GetKeyDown(keyCodeWithNoFailMod))
        {
            LoadCustomLevelFromScnGame();
            SetUseNoFail(true);
        }
    }

    private static void LoadCustomLevelFromScnGame()
    {
        scnGame gameInstance = scnGame.instance;
        if (gameInstance == null) return;

        string levelPath = gameInstance.levelPath;
        if (string.IsNullOrEmpty(levelPath)) return;

        scrController controllerInstance = scrController.instance;
        if (controllerInstance == null) return;

        controllerInstance.LoadCustomLevel(levelPath, "");
    }

    private static void SetUseNoFail(bool value)
    {
        GCS.useNoFail = value;
    }

    private static void OnGUI(ModEntry modEntry)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Keybind for opening level in CLS: {keyCode}", GUILayout.Width(500), GUILayout.ExpandWidth(false));
        if (GUILayout.Button(inputAwaitingOption == KeybindOption.OpenLevel ? "Press any key..." : "Set Key", GUILayout.Width(150), GUILayout.Height(20)))
        {
            inputAwaitingOption = KeybindOption.OpenLevel;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Keybind for opening level in CLS with No-Fail mod: {keyCodeWithNoFailMod}", GUILayout.Width(500), GUILayout.ExpandWidth(false));
        if (GUILayout.Button(inputAwaitingOption == KeybindOption.OpenLevelWithNoFail ? "Press any key..." : "Set Key", GUILayout.Width(150), GUILayout.Height(20)))
        {
            inputAwaitingOption = KeybindOption.OpenLevelWithNoFail;
        }
        GUILayout.EndHorizontal();
    }

    private static void SaveSettings()
    {
        XDocument settingsDoc = new XDocument(
            new XElement("Settings",
                new XElement("KeyCode", keyCode.ToString()),
                new XElement("KeyCodeWithNoFailMod", keyCodeWithNoFailMod.ToString())
            )
        );

        settingsDoc.Save(settingsFilePath);
    }

    private static void LoadSettings()
    {
        if (!File.Exists(settingsFilePath))
        {
            return;
        }

        XDocument settingsDoc = XDocument.Load(settingsFilePath);
        XElement root = settingsDoc.Root;

        XElement keyCodeElement = root?.Element("KeyCode");
        if (keyCodeElement != null)
        {
            if (System.Enum.TryParse(keyCodeElement.Value, out KeyCode loadedKeyCode))
            {
                keyCode = loadedKeyCode;
            }
        }

        XElement keyCodeF4Element = root?.Element("KeyCodeWithNoFailMod");
        if (keyCodeF4Element != null)
        {
            if (System.Enum.TryParse(keyCodeF4Element.Value, out KeyCode loadedKeyCodeF4))
            {
                keyCodeWithNoFailMod = loadedKeyCodeF4;
            }
        }
    }
}
