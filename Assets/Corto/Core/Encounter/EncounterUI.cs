using UnityEngine;
using TMPro;
public interface IEncounterView
{
    void ShowIntro();
    void ShowPlayerTurn();
    void ShowEnemyTurn();
    void ShowVictory(bool playerWon);
    void SetStatusText(string statusString);
}

public class EncounterUI : MonoBehaviour, IEncounterView
{
    [SerializeField] private TMP_Text statusText;

    public void ShowIntro()
    {
        StartCoroutine(SimulateIntroRoutine());
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

    private System.Collections.IEnumerator SimulateIntroRoutine()
    {
        const float duration = 2f;
        yield return new WaitForSeconds(duration);
        EncounterController ec = GetComponent<EncounterController>();
        ec.SignalIntroCompleteUI();
    }

}
