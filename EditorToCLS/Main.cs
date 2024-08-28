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
    private static bool isWaitingForKey = false;
    private static bool isWaitingForKeyF4 = false;
    private static string settingsFilePath;

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
        if (isWaitingForKey && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    keyCode = key; 
                    isWaitingForKey = false;
                    SaveSettings();
                    break;
                }
            }
        }

        if (isWaitingForKeyF4 && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    keyCodeWithNoFailMod = key;
                    isWaitingForKeyF4 = false;
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
        scnGame gameInstance = Object.FindObjectOfType<scnGame>();
        if (gameInstance == null) return;

        string levelPath = gameInstance.levelPath;
        if (string.IsNullOrEmpty(levelPath)) return;

        scrController controllerInstance = Object.FindObjectOfType<scrController>();
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
        GUILayout.Label("KeyCode: " + keyCode.ToString(), GUILayout.Width(300), GUILayout.ExpandWidth(false));
        if (GUILayout.Button(isWaitingForKey ? "Prass any key.." : "Set Key", GUILayout.Width(150), GUILayout.Height(20)))
        {
            isWaitingForKey = true; 
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("KeyCodeWithNoFailMod: " + keyCodeWithNoFailMod.ToString(), GUILayout.Width(300), GUILayout.ExpandWidth(false));
        if (GUILayout.Button(isWaitingForKeyF4 ? "Prass any key.." : "Set Key", GUILayout.Width(150), GUILayout.Height(20)))
        {
            isWaitingForKeyF4 = true; 
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
