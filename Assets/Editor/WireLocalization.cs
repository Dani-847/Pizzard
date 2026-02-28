
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class WireLocalization : EditorWindow
{
    [MenuItem("Tools/Wire Localization")]
    public static void Wire()
    {
        EditorSceneManager.OpenScene("Assets/FlowScenes/MainMenu.unity");

        // Main Menu
        WireLocalizedText("MenuUI/Panel/BotonJugar/Text (TMP)", "menu_play");
        WireLocalizedText("MenuUI/Panel/BotonOpciones/Text (TMP)", "menu_options");
        WireLocalizedText("MenuUI/Panel/BotonSalir/Text (TMP)", "menu_exit");
        WireLocalizedText("MenuUI/Panel/BotonContinuar/Text (TMP)", "menu_continue");

        // Options
        WireLocalizedText("OptionsUI/Panel/LanguageTittle/Text (TMP)", "options_language");
        WireLocalizedText("OptionsUI/Panel/VolumeTittle/Text (TMP)", "options_volume");
        WireLocalizedText("OptionsUI/Panel/CombinationsTittle/Text (TMP)", "options_combinations");
        WireLocalizedText("OptionsUI/Panel/BotonAceptar/Text (TMP)", "options_accept");

        // Combinations
        WireLocalizedText("CombinationsUI/Panel/BotonAtras/Text (TMP)", "options_back");

        // Death Screen
        WireLocalizedText("DeathUI/Panel/BotonReintentar/Texto", "death_retry");
        WireLocalizedText("DeathUI/Panel/BotonVolver/Texto", "death_exit_menu");
        WireLocalizedText("DeathUI/Panel/Derrota/Texto", "death_title");

        // Bosses
        SetBossKey("Hec'kiel Arena V2/BossHealthBar", "boss_heckiel");
        SetBossKey("Pomodoro Arena/BossHealthBar", "boss_pomodoro");
        SetBossKey("Niggel Arena v2/BossHealthBar", "boss_niggel");
        SetBossKey("P'blob Arena/BossHealthBar", "boss_pblob");


        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Localization wiring complete for MainMenu.");
    }

    private static void WireLocalizedText(string path, string key)
    {
        GameObject obj = GameObject.Find(path);
        if (obj != null)
        {
            LocalizedText locText = obj.GetComponent<LocalizedText>();
            if (locText == null)
            {
                locText = obj.AddComponent<LocalizedText>();
            }

            SerializedObject so = new SerializedObject(locText);
            SerializedProperty keyProp = so.FindProperty("key");
            keyProp.stringValue = key;
            so.ApplyModifiedProperties();

            Debug.Log($"Wired: {path} -> {key}");
        }
        else
        {
            Debug.LogWarning($"Could not find GameObject at path: {path}");
        }
    }

    private static void SetBossKey(string path, string key)
    {
        GameObject obj = GameObject.Find(path);
        if (obj != null)
        {
            var healthBar = obj.GetComponent<Pizzard.UI.BossHealthBarUI>();
            if(healthBar != null)
            {
                SerializedObject so = new SerializedObject(healthBar);
                SerializedProperty keyProp = so.FindProperty("bossLocalizationKey");
                keyProp.stringValue = key;
                so.ApplyModifiedProperties();
                Debug.Log($"Set boss key: {path} -> {key}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find GameObject at path: {path}");
        }
    }
}
