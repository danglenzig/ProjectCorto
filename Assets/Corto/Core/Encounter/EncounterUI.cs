using UnityEngine;
using TMPro;

public class EncounterUI : MonoBehaviour, IEncounterView
{
    [SerializeField] private TMP_Text statusText;

    public void ShowIntro()
    {
        // ...
    }
    public void ShowPlayerTurn()
    {
        // ...
    }
    public void ShowEnemyTurn()
    {
        // ...
    }
    public void ShowVictory(bool playerWon)
    {
        StartCoroutine(SimulateVictoryRoutine(playerWon));
    }
    public void SetStatusText(string statusString)
    {
        statusText.text = statusString;
    }

    private System.Collections.IEnumerator SimulateVictoryRoutine(bool playerWon)
    {
        const float duration = 2f;
        yield return new WaitForSeconds(duration);

        EncounterController ec = GetComponent<EncounterController>();
        ec.SignalEncounterCompleteUI(playerWon);
    }
}
