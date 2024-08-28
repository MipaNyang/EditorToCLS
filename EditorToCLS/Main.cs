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
    private static KeyCode keyCode = KeyCode.F3; // 기본 키 설정
    private static KeyCode keyCodeWithNoFailMod = KeyCode.F4; // 기본 F4 키 설정
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
        // 키 입력 대기 상태일 때 키를 감지
        if (isWaitingForKey && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    keyCode = key; // 키 변경
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
                    keyCodeWithNoFailMod = key; // F4 키 변경
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
        // 현재 키 설정 UI
        GUILayout.BeginHorizontal();
        GUILayout.Label("KeyCode: " + keyCode.ToString(), GUILayout.Width(300), GUILayout.ExpandWidth(false));
        if (GUILayout.Button(isWaitingForKey ? "..." : "Set Key", GUILayout.Width(80), GUILayout.Height(20)))
        {
            isWaitingForKey = true; // 키 변경 대기 상태
        }
        GUILayout.EndHorizontal();

        // F4 키 설정 UI
        GUILayout.BeginHorizontal();
        GUILayout.Label("KeyCodeWithNoFailMod: " + keyCodeWithNoFailMod.ToString(), GUILayout.Width(300), GUILayout.ExpandWidth(false));
        if (GUILayout.Button(isWaitingForKeyF4 ? "..." : "Set Key", GUILayout.Width(80), GUILayout.Height(20)))
        {
            isWaitingForKeyF4 = true; // F4 키 변경 대기 상태
        }
        GUILayout.EndHorizontal();
    }

    private static void SaveSettings()
    {
        // 설정 파일에 현재 키 설정 저장
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
        // 설정 파일에서 키 설정 불러오기
        if (!File.Exists(settingsFilePath))
        {
            return; // 설정 파일이 없으면 기본 값을 사용
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
