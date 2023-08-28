using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NimbatSettingsWindow : EditorWindow
{
    static string settingsFileName = "NimbatSettings.asset";
    static string packagePath = "Packages/com.lunesnowtail.vrc-nimbat-tools/Runtime/Resources/";

    static NimbatSettingsSO _settings;
    static public NimbatSettingsSO settings
    {
        get
        {
            if (!_settings)
            {                                
                _settings = (NimbatSettingsSO) Resources.Load(settingsFileName);

                if (!_settings)
                {
                    Debug.Log(string.Concat("Settings have been reset to default ", settingsFileName, " failed to load"));

                    _settings = NimbatSettingsSO.CreateInstance<NimbatSettingsSO>();
                    AssetDatabase.CreateAsset(_settings, string.Concat(packagePath, settingsFileName));
                    
                }
            }

            return _settings;
        }

        private set
        {
            _settings = value;
        }
    }

    // Add menu named "My Window" to the Window menu
    [MenuItem("Nimbat Tools/Settings")]
    static void OpenSettings()
    {
        // Get existing open window or if none, make a new one:
        NimbatSettingsWindow window = (NimbatSettingsWindow)EditorWindow.GetWindow(typeof(NimbatSettingsWindow));
        window.Show();

        Debug.Log(settings.ToString());
        SetDefaultRules();
    }

    static void SetDefaultRules()
    {
        MirrorNameRule rule1 = new MirrorNameRule(MirrorNameRule.CaseModes.CaseSensitive, MirrorNameRule.RuleTypes.AtEnd, "_L", "_R");
        MirrorNameRule rule2 = new MirrorNameRule(MirrorNameRule.CaseModes.CaseSensitive, MirrorNameRule.RuleTypes.AtEnd, ".L", ".R");
        MirrorNameRule rule3 = new MirrorNameRule(MirrorNameRule.CaseModes.NonCaseSensitive, MirrorNameRule.RuleTypes.Contains, "Left", "Right");

        settings.mirrorRules = new List<MirrorNameRule>();

        settings.mirrorRules.Add(rule1);
        settings.mirrorRules.Add(rule2);
        settings.mirrorRules.Add(rule3);
    }

    private void OnGUI()
    {
        for(int i = 0; i< settings.mirrorRules.Count ; i++)
        {
            GUILayout.Label(settings.mirrorRules[i].caseMode.ToString());
            GUILayout.Label(settings.mirrorRules[i].ruleType.ToString());
            GUILayout.Label(settings.mirrorRules[i].rightName.ToString());
            GUILayout.Label(settings.mirrorRules[i].leftName.ToString());
        }
    }

}
