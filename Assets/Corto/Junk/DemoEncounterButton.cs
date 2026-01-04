using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class DemoEncounterButton : MonoBehaviour
{
    private Button encounterButton;

    [SerializeField] private List<CombatantDataSO> playerCombatants;
    [SerializeField] private List<CombatantDataSO> enemyCombatants;
    [SerializeField] private GameObject environmentPrefab;

    private void Awake()
    {
        encounterButton = GetComponent<Button>();
        encounterButton.onClick.AddListener(HandleButtonPressed);
    }

    private void OnDestroy()
    {
        encounterButton.onClick.RemoveAllListeners();
    }

    private void HandleButtonPressed()
    {
        Party playerParty = GameServices.GetParty(playerCombatants);
        Party enemyParty = GameServices.GetParty(enemyCombatants);
        GameServices.LaunchEncounter(new EncounterData(playerParty, enemyParty, environmentPrefab));
    }
}
