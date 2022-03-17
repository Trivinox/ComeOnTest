using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game01UI : GameUI
{
    public void AnswerYes()
    {
        leftAnswer.interactable = false;
        rightAnswer.interactable = true;
        player.SendAnswer("Y: " + prompt);
    }
    public void AnswerNo()
    {
        leftAnswer.interactable = true;
        rightAnswer.interactable = false;
        player.SendAnswer("N: " + prompt);
    }
    public void SendReady()
    {
        okBtn.interactable = false;
        player.SendReady();
    }
}
