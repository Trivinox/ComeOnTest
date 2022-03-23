using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectMenu : MonoBehaviour
{
    private CategoriesList cl;
    private CategoriesList selected;
    public static string prefsKey = "selectedPrefs";
    [SerializeField]
    private TextAsset categoriesJson;

    [SerializeField]
    private Button playBtn;
    [SerializeField]
    private Image playImage;
    [SerializeField]
    private Image prefImage;
    private int minHobbies = 4;
    private int maxHobbies = 15;

    [Header("Container")]
    [SerializeField] private ScrollRect ItemsContainer;

    [Header("Items Lists")]
    [SerializeField] private GameObject mainItemsList;
    [SerializeField] private GameObject categoryItemsList;
    [SerializeField] private GameObject sectionItemsList;
    [SerializeField] private GameObject selectedItemsList;

    [Header("Tabs")]
    [SerializeField] private GameObject categoryTab;
    [SerializeField] private GameObject sectionTab;

    [Header("Texts")]
    [SerializeField] private Text selectedCount;
    [SerializeField] private GameObject addPreferencesPrompt;
    [SerializeField] private GameObject minimumWarning;

    [Header("Prefabs")]
    [SerializeField] private GameObject selectedListPrefab;

    [Header("Colors (NOT ASSIGNED)")]
    [SerializeField] private Color normalColor;       //PROVISIONALES!!!!!!!!!
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color textColor;

    private void Awake()
    {
        cl = JsonUtility.FromJson<CategoriesList>(categoriesJson.text);

        LoadPersonalList();
        UpdateSelectedCount();


        foreach (Category cat in cl.Categories)
        {
            GameObject actual = Instantiate(selectedListPrefab, mainItemsList.transform, false);
            actual.GetComponent<Image>().color = normalColor;
            actual.GetComponentInChildren<Text>().text = cat.Name;
            actual.GetComponentInChildren<Text>().color = textColor;
            actual.GetComponent<Button>().onClick.AddListener(delegate { OnSelectTab(cat.Name, true); SetList(1); });
        }
    }

    void OnSelectTab(string name, bool isCategory)
    {
        SetList(isCategory ? 1 : 2);

        if (isCategory)
        {
            if (!categoryTab.GetComponentInChildren<Text>().text.Equals(name))
            {
                SetTabName(name, isCategory);
                DeleteAllChildren(categoryItemsList);

                foreach (Section sec in cl.FindCategory(name).Sections)
                {
                    GameObject actual = Instantiate(selectedListPrefab, categoryItemsList.transform, false);
                    actual.GetComponent<Image>().color = normalColor;
                    actual.GetComponentInChildren<Text>().text = sec.Name;
                    actual.GetComponentInChildren<Text>().color = textColor;
                    actual.GetComponent<Button>().onClick.AddListener(delegate { OnSelectTab(sec.Name, false); SetList(2); });
                }

            }
        }
        else
        {
            if (!sectionTab.GetComponentInChildren<Text>().text.Equals(name))
            {
                SetTabName(name, isCategory);
                DeleteAllChildren(sectionItemsList);

                foreach (Hobby hob in cl.FindSection(name).Hobbies)
                {
                    GameObject actual = Instantiate(selectedListPrefab, sectionItemsList.transform, false);
                        actual.GetComponentInChildren<Text>().text = hob.name;
                        actual.GetComponentInChildren<Text>().color = textColor;
                    if (selected.FindHobby(hob.name) == null)
                    {
                        actual.GetComponent<Button>().onClick.AddListener(delegate { AddSelectedHobby(cl.FindCategoryForSection(name).Name, name, hob.name); });
                        actual.GetComponent<Image>().color = normalColor;
                    }
                    else
                    {
                        actual.GetComponent<Button>().onClick.AddListener(delegate { RemoveSelectedHobby(cl.FindCategoryForSection(name).Name, name, hob.name); });
                        actual.GetComponent<Image>().color = selectedColor;
                    }
                }
            }
        }
    }

    void SetTabName(string name, bool isCategory)
    {
        if (isCategory)
            categoryTab.GetComponentInChildren<Text>().text = name;
        else
            sectionTab.GetComponentInChildren<Text>().text = name;
    }

    void DeleteAllChildren(GameObject go)
    {
        foreach (Transform child in go.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetList(int tabIndex)
    {
        mainItemsList.SetActive(false);
        categoryItemsList.SetActive(false);
        sectionItemsList.SetActive(false);

        categoryTab.SetActive(false);
        sectionTab.SetActive(false);

        switch (tabIndex)
        {
            case 0:
                mainItemsList.SetActive(true);
                ItemsContainer.content = mainItemsList.GetComponent<RectTransform>();
                break;
            case 1:
                categoryItemsList.SetActive(true);
                categoryTab.SetActive(true);
                ItemsContainer.content = categoryItemsList.GetComponent<RectTransform>();
                break;
            case 2:
                sectionItemsList.SetActive(true);
                categoryTab.SetActive(true);
                sectionTab.SetActive(true);
                ItemsContainer.content = sectionItemsList.GetComponent<RectTransform>();
                break;
            default:
                Debug.Log("Se trata de mostrar una lista seleccionable que no existe");
                break;
        }

        Image containerImage = transform.GetChild(1).GetChild(1).GetComponent<Image>();
        Color colorToAssign = transform.GetChild(1).GetChild(0).GetChild(tabIndex).GetComponent<Image>().color;
        containerImage.color = colorToAssign;
    }

    void AddSelectedHobby(string category, string section, string hobby)
    {
        if (selected.CountHobbies() < maxHobbies)
        {
            selected.AddHobby(category, section, hobby);
            AddSelectedGameObject(category, section, hobby);
            UpdateListedGameObject(category, section, hobby, true);
            UpdateSelectedCount();
        }
    }

    void RemoveSelectedHobby(string category, string section, string hobby)
    {
        selected.RemoveHobby(hobby);
        UpdateListedGameObject(category, section, hobby, false);
        UpdateSelectedCount();
        RemoveSelectedGameObject(hobby);
    }

    void AddSelectedGameObject(string category, string section, string hobby)
    {
        GameObject actual = Instantiate(selectedListPrefab, selectedItemsList.transform, false);
        actual.GetComponentInChildren<Text>().text = hobby;
        actual.GetComponent<Image>().color = normalColor;
        actual.GetComponent<Button>().onClick.AddListener(delegate { RemoveSelectedHobby(category, section, hobby);});
    }

    void RemoveSelectedGameObject(string hobby)
    {
        GameObject temp;
        for (int i = 0; i < selectedItemsList.transform.childCount; i++)
        {
            temp = selectedItemsList.transform.GetChild(i).gameObject;
            if (temp.transform.GetChild(0).GetComponent<Text>().text.Equals(hobby))
            {
                Destroy(temp);
                return;
            }
        }
    }

    void UpdateListedGameObject(string category, string section, string hobby, bool listenerToAdd)
    {
        GameObject temp;
        for (int i = 0; i < sectionItemsList.transform.childCount; i++)
        {
            temp = sectionItemsList.transform.GetChild(i).gameObject;
            if (temp.transform.GetChild(0).GetComponent<Text>().text.Equals(hobby))
            {
                temp.GetComponent<Image>().color = listenerToAdd ?  selectedColor : normalColor;
                temp.GetComponent<Button>().onClick.RemoveAllListeners();
                if (listenerToAdd)
                    temp.GetComponent<Button>().onClick.AddListener(delegate { RemoveSelectedHobby(category, section, hobby); });
                else
                    temp.GetComponent<Button>().onClick.AddListener(delegate { AddSelectedHobby(category, section, hobby); });
                return;
            }
        }
    }

    void UpdateSelectedCount()
    {
        int number = selected.CountHobbies();
        selectedCount.text = number.ToString();

        if (number >= minHobbies)
        {
            selectedCount.color = new Color(0.15f, 0.15f, 0.15f);
            minimumWarning.SetActive(false);
        }
        else
        {
            selectedCount.color = Palette.Red3;
            minimumWarning.SetActive(true);
        }
    }

    public void SavePersonalList()
    {
        PlayerPrefs.SetString(prefsKey, JsonUtility.ToJson(selected, false));
    }

    public void LoadPersonalList()
    {
        // Load Personal Selected List
        string prefsString = PlayerPrefs.GetString(prefsKey, "");

        if (prefsString.Equals(""))
            selected = JsonUtility.FromJson<CategoriesList>("{}");
        else
            selected = JsonUtility.FromJson<CategoriesList>(PlayerPrefs.GetString(prefsKey, "{}"));

        foreach (Category cat in selected.Categories)
        {
            foreach (Section sec in cat.Sections)
            {
                foreach (Hobby hob in sec.Hobbies)
                {
                    AddSelectedGameObject(cat.Name, sec.Name, hob.name);
                }
            }
        }
    }

    public void AllowPlayBtn()
    {
        bool haveMinimum = selected.CountHobbies() >= minHobbies;
        playBtn.interactable = haveMinimum;
        playImage.color = haveMinimum ? Palette.Red3 : Palette.Red1;
        prefImage.color = haveMinimum ? Palette.Red1 : Palette.Red3;
        addPreferencesPrompt.SetActive(!haveMinimum);
    }

    public void ExitGame() => Application.Quit(0);
}