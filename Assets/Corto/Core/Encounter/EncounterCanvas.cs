using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncounterCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    private Canvas myCanvas;

    private void Awake()
    {
        myCanvas = GetComponent<Canvas>();
    }
    public void ShowIntro()
    {

    }
    public void ShowPlayerTurn()
    {

    }
    public void ShowEnemyTurn()
    {

    }
    public void ShowVictory(bool playerWon)
    {

    }
    public void SetStatusText(string statusStr)
    {
        statusText.text = statusStr;
    }
}
