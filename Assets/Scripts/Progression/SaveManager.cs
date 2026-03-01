using System;
using System.Collections.Generic;
using UnityEngine;
using Pizzard.Core;

namespace Pizzard.Progression
{
    [Serializable]
    public class SaveData
    {
        public int tokens = 0;
        public int wandTier = 0;
        public int potionCount = 0;
        public float manaMax = GameBalance.Mana.MaxMana;
        public int bossIndex = 1;
        public string languageSetting = "en";
        // In-memory only: carries the wand element palette across scene loads (Shop → Boss).
        // Not written to disk. Populated by GameFlowManager before loading a combat scene.
        [NonSerialized] public List<ElementType> pendingWandElements = new List<ElementType>();
    }

    /// <summary>
    /// Singleton manager responsible for reading and writing the unified JSON save file.
    /// Used across the MainMenu and In-Game Shop/Boss phases.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        public SaveData CurrentSave { get; private set; }

        private string savePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            savePath = Application.persistentDataPath + "/pizzard_save.json";
            
            LoadGame();
        }

        public void ResetSave()
        {
            Debug.Log("[SaveManager] Resetting save data for a new run.");
            CurrentSave = new SaveData();
            SaveGame();
            ApplySaveDataToGame();
        }

        public void SaveGame()
        {
            try
            {
                // Push live memory states into CurrentSave before serializing
                UpdateSaveDataFromGame();

                string json = JsonUtility.ToJson(CurrentSave, true);
                System.IO.File.WriteAllText(savePath, json);
                Debug.Log($"[SaveManager] Game saved successfully to: {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game! Error: {e.Message}");
            }
        }

        public void LoadGame()
        {
            if (System.IO.File.Exists(savePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(savePath);
                    CurrentSave = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log("[SaveManager] Game loaded successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Corrupted save file, creating new... Error: {e.Message}");
                    CurrentSave = new SaveData();
                }
            }
            else
            {
                Debug.Log("[SaveManager] No save file found, creating a new run.");
                CurrentSave = new SaveData();
                SaveGame();
            }
            
            // Push loaded data back into the game managers
            ApplySaveDataToGame();
        }

        private void UpdateSaveDataFromGame()
        {
            // Gather state from GameFlowManager and ProgressionManager
            if (GameFlowManager.Instance != null)
            {
                CurrentSave.bossIndex = GameFlowManager.Instance.currentBossIndex;
            }
            
            if (ProgressionManager.Instance != null)
            {
                CurrentSave.tokens = ProgressionManager.Instance.BossCurrency;
            }
        }

        private void ApplySaveDataToGame()
        {
            // Pushes loaded SaveData out into the wild
            // GameFlowManager.currentBossIndex is read-only usually, so we let the IniciarJuego / Continuar handle it
            if (ProgressionManager.Instance != null)
            {
                // Overwrite the in-memory currency with loaded JSON
                ProgressionManager.Instance.SetCurrencyFromSave(CurrentSave.tokens);
            }

            var playerEquip = FindObjectOfType<PlayerEquip>(true);
            if (playerEquip != null)
            {
                playerEquip.LoadTierFromSave(CurrentSave.wandTier);
            }
            
            var mana = FindObjectOfType<ManaSystem>(true);
            if (mana != null)
            {
                mana.LoadMaxManaFromSave(CurrentSave.manaMax);
            }
        }
    }
}
