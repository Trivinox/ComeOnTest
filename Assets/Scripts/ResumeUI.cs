using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResumeUI : MonoBehaviour
{
    MainNetworkManager manager;
    bool isHost = false;

    [SerializeField]
    private GameObject namePrefab;
    [SerializeField]
    private GameObject contentPrefab;
    [SerializeField]
    private Transform[] ownMatchesSections;
    [SerializeField]
    private TMP_Text firstCoupleA;
    [SerializeField]
    private TMP_Text firstCoupleB;
    [SerializeField]
    private TMP_Text secondCouple;
    [SerializeField]
    private TMP_Text thirdCouple;
    [SerializeField]
    private TMP_Text endBtnText;

    public void ChangeGeneralCouple(string playerA, string playerB, int podiumPosition)
    {
        switch (podiumPosition)
        {
            case 1:
                firstCoupleA.text = playerA;
                firstCoupleB.text = playerB;
                break;
            case 2:
                secondCouple.text = playerA + " & " + playerB;
                break;
            case 3:
                thirdCouple.text = playerA + " & " + playerB;
                break;
            default:
                Debug.Log("Invalid Podium Position");
                break;
        }
    }

    public void AddOwnMatch(string playerName, int podiumPosition, CategoriesList common)
    {
        foreach (Transform child in ownMatchesSections[podiumPosition])
            GameObject.Destroy(child.gameObject);

        GameObject nameGO = Instantiate(namePrefab, ownMatchesSections[podiumPosition]);
        nameGO.transform.GetComponent<TMP_Text>().text = (podiumPosition + 1) + ": " + playerName;

        string[] hobbies = common.GetHobbies();
        string[] sections = common.GetBlankSections();
        string[] categories = common.GetBlankCategories();

        int max = 7;

        for (int i = 0; i < max; i++)
        {
            if (i < hobbies.Length)
                AddCoincidence(podiumPosition, hobbies[i], 2);
            else if (i < hobbies.Length + sections.Length)
                AddCoincidence(podiumPosition, sections[i - hobbies.Length], 1);
            else if (i < hobbies.Length + sections.Length + categories.Length)
                AddCoincidence(podiumPosition, sections[i - hobbies.Length - sections.Length], 0);
            else
                break;
        }
    }

    void AddCoincidence(int position, string text, int type)
    {
        GameObject contentGO = Instantiate(contentPrefab, ownMatchesSections[position]);
        contentGO.transform.GetChild(0).GetComponent<TMP_Text>().text = text;
        switch (type)
        {
            case 0:
                contentGO.GetComponent<Image>().color = Palette.Blue3;
                break;
            case 1:
                contentGO.GetComponent<Image>().color = Palette.Blue2;
                break;
            case 2:
                contentGO.GetComponent<Image>().color = Palette.Blue1;
                break;
            default:
                break;
        }
    }

    public void SetManager(MainNetworkManager manager) => this.manager = manager;

    public void SetBtnHost()
    {
        endBtnText.text = "Terminar para todos";
        isHost = true;
    }

    public void EndBtn()
    {
        if (isHost)
            manager.StopHost();
        else
            manager.StopClient();
    }
}
