using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script para reconstruir la escena del boss con la estructura correcta.
/// Uso: En Unity Editor, ir a Tools > Pizzard > Rebuild Boss Scene
/// </summary>
public class SceneRebuilder : EditorWindow
{
    [MenuItem("Tools/Pizzard/Rebuild Boss Scene")]
    public static void ShowWindow()
    {
        GetWindow<SceneRebuilder>("Scene Rebuilder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Reconstruir Escena de Boss", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Este script organizará la escena actual en la estructura correcta.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Organizar Escena Actual"))
        {
            OrganizeCurrentScene();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Verificar Referencias"))
        {
            VerifyReferences();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Configurar Estado Inicial"))
        {
            SetupInitialState();
        }
    }

    private void OrganizeCurrentScene()
    {
        Debug.Log("🔧 Organizando escena...");
        
        // Crear contenedores principales si no existen
        GameObject managers = FindOrCreate("--- MANAGERS ---", null);
        GameObject boss = FindOrCreate("--- BOSS ---", null);
        GameObject player = FindOrCreate("--- PLAYER ---", null);
        GameObject ui = FindOrCreate("--- UI ---", null);
        GameObject environment = FindOrCreate("--- ENVIRONMENT ---", null);
        
        // Mover GameFlowManager al contenedor MANAGERS
        MoveToParent("GameFlowManager", managers);
        MoveToParent("SoundManager", managers);
        MoveToParent("UIManager", managers);
        MoveToParent("LocalizationManager", managers);
        MoveToParent("ElementsRegister", managers);
        
        // Mover objetos del boss al contenedor BOSS
        MoveToParent("Pblob", boss);
        MoveToParent("PblobArena", boss);
        MoveToParent("PblobRhythmManager", boss);
        MoveToParent("MoustachePoints", boss);
        
        // Mover player al contenedor PLAYER
        MoveToParent("MainCharacter", player);
        
        // Mover UI al contenedor UI
        MoveToParent("Interfaz", ui); // El Canvas principal puede llamarse Interfaz
        MoveToParent("Canvas", ui);
        MoveToParent("EventSystem", ui);
        
        // Mover environment
        MoveToParent("Environment", environment);
        MoveToParent("AuxEsquinas", environment);
        MoveToParent("AuxEsquinas (1)", environment);
        MoveToParent("AuxEsquinas (2)", environment);
        MoveToParent("AuxEsquinas2", environment);
        
        Debug.Log("✅ Escena organizada correctamente");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void VerifyReferences()
    {
        Debug.Log("🔍 Verificando referencias...");
        
        // Verificar GameFlowManager
        var gfm = FindObjectOfType<GameFlowManager>();
        if (gfm != null)
        {
            Debug.Log("✅ GameFlowManager encontrado");
            
            // Verificar UIManager
            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log("  ✅ UIManager encontrado");
                
                if (uiManager.menuUI == null)
                    Debug.LogWarning("  ⚠️ UIManager.menuUI no asignado");
                if (uiManager.dialogUI == null)
                    Debug.LogWarning("  ⚠️ UIManager.dialogUI no asignado");
                if (uiManager.tiendaUI == null)
                    Debug.LogWarning("  ⚠️ UIManager.tiendaUI no asignado");
                if (uiManager.deathUI == null)
                    Debug.LogWarning("  ⚠️ UIManager.deathUI no asignado");
                if (uiManager.optionsUI == null)
                    Debug.LogWarning("  ⚠️ UIManager.optionsUI no asignado");
            }
            else
            {
                Debug.LogError("  ❌ UIManager no encontrado");
            }
            
            // Verificar PblobController
            var pblob = FindObjectOfType<PblobController>();
            if (pblob != null)
            {
                Debug.Log("  ✅ PblobController encontrado");
            }
            else
            {
                Debug.LogWarning("  ⚠️ PblobController no encontrado");
            }
            
            // Verificar PlayerController
            var playerCtrl = FindObjectOfType<PlayerController>();
            if (playerCtrl != null)
            {
                Debug.Log("  ✅ PlayerController encontrado");
            }
            else
            {
                Debug.LogWarning("  ⚠️ PlayerController no encontrado");
            }
        }
        else
        {
            Debug.LogError("❌ GameFlowManager no encontrado - La escena no funcionará correctamente");
        }
        
        // Verificar SoundManager
        var sound = FindObjectOfType<SoundManager>();
        if (sound != null)
        {
            Debug.Log("✅ SoundManager encontrado");
        }
        else
        {
            Debug.LogWarning("⚠️ SoundManager no encontrado");
        }
        
        Debug.Log("🔍 Verificación completada");
    }

    private void SetupInitialState()
    {
        Debug.Log("🔧 Configurando estado inicial...");
        
        // Primero, ocultar todas las UIs de gameplay
        var pblobUI = FindObjectOfType<PblobUI>();
        if (pblobUI != null)
        {
            pblobUI.gameObject.SetActive(false);
            Debug.Log("  ✅ PblobUI desactivado");
        }
        
        var potionUI = FindObjectOfType<PotionUI>();
        if (potionUI != null)
        {
            potionUI.gameObject.SetActive(false);
            Debug.Log("  ✅ PotionUI desactivado");
        }
        
        // Configurar UIs de flujo
        var menuUI = FindObjectOfType<MenuUI>();
        if (menuUI != null)
        {
            menuUI.gameObject.SetActive(true);
            Debug.Log("  ✅ MenuUI activado");
        }
        
        var dialogUI = FindObjectOfType<DialogUI>();
        if (dialogUI != null)
        {
            if (dialogUI.dialogPanel != null)
                dialogUI.dialogPanel.SetActive(false);
            else
                dialogUI.gameObject.SetActive(false);
            Debug.Log("  ✅ DialogUI desactivado");
        }
        
        var shopUI = FindObjectOfType<ShopUI>();
        if (shopUI != null)
        {
            shopUI.gameObject.SetActive(false);
            Debug.Log("  ✅ ShopUI desactivado");
        }
        
        var deathUI = FindObjectOfType<DeathUI>();
        if (deathUI != null)
        {
            if (deathUI.pantallaMuerte != null)
                deathUI.pantallaMuerte.SetActive(false);
            Debug.Log("  ✅ DeathUI desactivado");
        }
        
        var optionsUI = FindObjectOfType<OptionsUI>();
        if (optionsUI != null)
        {
            optionsUI.gameObject.SetActive(false);
            Debug.Log("  ✅ OptionsUI desactivado");
        }
        
        var combinationsUI = FindObjectOfType<CombinationsUI>();
        if (combinationsUI != null)
        {
            if (combinationsUI.panelCombinations != null)
                combinationsUI.panelCombinations.SetActive(false);
            Debug.Log("  ✅ CombinationsUI desactivado");
        }
        
        // Configurar GameFlowManager para empezar en MainMenu
        var gfm = FindObjectOfType<GameFlowManager>();
        if (gfm != null)
        {
            // El campo faseInicial es privado, así que lo configuramos via SerializedObject
            var so = new SerializedObject(gfm);
            var prop = so.FindProperty("faseInicial");
            if (prop != null)
            {
                prop.enumValueIndex = (int)GamePhase.MainMenu;
                so.ApplyModifiedProperties();
                Debug.Log("  ✅ GameFlowManager.faseInicial = MainMenu");
            }
            
            // Verificar que UIManager está asignado
            var uiManagerProp = so.FindProperty("uiManager");
            if (uiManagerProp != null && uiManagerProp.objectReferenceValue == null)
            {
                var uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    uiManagerProp.objectReferenceValue = uiManager;
                    so.ApplyModifiedProperties();
                    Debug.Log("  ✅ GameFlowManager.uiManager asignado");
                }
            }
        }
        
        Debug.Log("✅ Estado inicial configurado");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private GameObject FindOrCreate(string name, Transform parent)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
            if (parent != null)
                obj.transform.SetParent(parent);
            Debug.Log($"  Creado: {name}");
        }
        return obj;
    }

    private void MoveToParent(string objName, GameObject parent)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null && obj.transform.parent != parent.transform)
        {
            obj.transform.SetParent(parent.transform);
            Debug.Log($"  Movido: {objName} → {parent.name}");
        }
    }
}
