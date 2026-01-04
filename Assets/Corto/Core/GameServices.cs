using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;



public class GameServices : Singleton<GameServices>
{
    public const string ENCOUNTER_SCENE_NAME = "EncounterDemo";
    private static EncounterData currentEncounterData = null;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleOnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleOnSceneLoaded;
    }

    

    private static void HandleOnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == ENCOUNTER_SCENE_NAME)
        {
            EncounterController ec = FindFirstObjectByType<EncounterController>();
            if (ec == null) { Debug.LogError("Scene has no EncounterController"); return; }
            if (currentEncounterData == null)
            {
                Debug.LogWarning($"No current EncounterData");
                ec.SetUpEncounter(new EncounterData());
            }
            else
            {
                ec.SetUpEncounter(currentEncounterData);
            }
        }
    }

    /////////
    // API //
    /////////
    
    public static void LaunchEncounter(EncounterData inData)
    {
        currentEncounterData = inData;
        SceneManager.LoadScene(ENCOUNTER_SCENE_NAME);
    }

    public static Party GetParty(List<CombatantDataSO> combatants)
    {
        List<string> ids = new List<string>();
        foreach (CombatantDataSO combatant in combatants)
        {
            ids.Add(combatant.CombatantID);
        }
        return new Party(ids);
    }
}
