using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    protected NetworkGamePlayer player;

    public void SetPlayer(NetworkGamePlayer player)
    {
        this.player = player;
    }

    [SerializeField]
    private GameObject[] panels;

    [Header("Question Panel")]
    [SerializeField]
    private TMP_InputField inpQuestion;
    [SerializeField]
    private Button btnSendQuestion;
    [SerializeField]
    private TMP_Text textQuestion;

    [Header("Answer Panel")]
    [SerializeField]
    protected  TMP_Text promptText;
    protected string prompt;
    [SerializeField]
    protected Button leftAnswer;
    [SerializeField]
    protected Button rightAnswer;

    [Header("Results Panel")]
    [SerializeField]
    private GameObject resultsPrefab;
    [SerializeField]
    private TMP_Text titleLeft;
    [SerializeField]
    private TMP_Text titleRight;
    [SerializeField]
    private Transform listLeft;
    [SerializeField]
    private Transform listRight;
    [SerializeField]
    protected Button okBtn;

    public virtual void SetPrompt(string prompt)
    {
        this.prompt = prompt;
        promptText.text = "¿Tú eres un(a) " + prompt + "?";
    }
    public void SendQuestion()
    {
        string input = inpQuestion.text;
        inpQuestion.text = "";
        textQuestion.text = "Yo soy un(a) " + input;
        btnSendQuestion.interactable = false;
        player.SendAnswer(input);
    }

    public void UpdateQuestionInput()
    {
        btnSendQuestion.interactable = !inpQuestion.text.Equals("");
    }

    /// 0-explain  1-question  2-answer  3-results  4-resume
    public void TogglePanels(int index)
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetActive(i == index);

        switch (index)
        {
            case 1:
                inpQuestion.text = "";
                textQuestion.text = "Yo soy un(a) ...";
                btnSendQuestion.interactable = false;
                break;
            case 2:
                promptText.text = "¿Tú eres un(a) ... ?";
                leftAnswer.interactable = true;
                rightAnswer.interactable = true;
                break;
            case 3:
                titleLeft.text = "Sí soy ...";
                titleRight.text = "No soy ...";
                okBtn.interactable = true;

                foreach (Transform child in listLeft)
                    GameObject.Destroy(child.gameObject);

                foreach (Transform child in listRight)
                    GameObject.Destroy(child.gameObject);
                break;
        }
    }

    public void AddPlayerToColumn(string player, bool isPrompt)
    {
        GameObject listName;

        if (isPrompt)
            listName = Instantiate(resultsPrefab, listLeft);
        else
            listName = Instantiate(resultsPrefab, listRight);

        TMP_Text tm = listName.GetComponent<TMP_Text>();
        tm.text = player;
    }

    public void ChangeColumnsNames(string leftTitle, string rightTitle)
    {
        titleLeft.text = leftTitle;
        titleRight.text = rightTitle;
    }

    void Start()
    {
        TogglePanels(0);
    }

}
