using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DialougeManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialougeText;
    [SerializeField] private TextAsset globalVarText;
    private Story curStory;
    [SerializeField] private String curStoryRef;
    [SerializeField] private bool dialougeIsPlaying;
    private static DialougeManager instance;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private PlayerInputManager playerInputManger;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RectTransform choices;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Animator portaitAnimator;
    [SerializeField] private Animator layoutAnimator;
    [Range(0f,0.2f)]
    [SerializeField] private float typingSpeed;
    private Coroutine displayLineCoroutine;
    private bool canContinueTyping,isDoneTalking;
    private DialougeVariables dialougeVariables;
    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string LAYOUT_TAG = "layout";
    private Action curCompleteAfterTalk;
    public event Action onSwitchTeamActivated,onActivateStore,onIntimidated,onCharmed,onTricked;
    private bool switchTeamActivated, activatedStore,intimidated,charmed,tricked;


    public void Awake() {
        if (instance != null) {
            Debug.Log("ERROR numplite dialoge managers");
        }
        else {
            instance = this;
        }
        dialougeIsPlaying = false;
        dialougeVariables = new DialougeVariables(globalVarText);
        isDoneTalking = false;
    }

    private void Start() {
        dialougeIsPlaying = false;
        dialoguePanel.SetActive(false);
        layoutAnimator = dialoguePanel.GetComponent<Animator>();
    }
    private void Update() {
        if (dialougeIsPlaying==false) {
            return;
        }
        // continues if buttn pressed and no choices are not available
        if (playerInputManger.IsContinue && curStory.currentChoices.Count == 0) {
            ContinueStory();
        }
    }
    public static DialougeManager Instance() {
        return instance;
    }

    public void EnterDialougeMode(TextAsset inkText, Action onCompletedAfterTalk= null) {
        curCompleteAfterTalk = onCompletedAfterTalk;
        curStory = new Story(inkText.text);
        
        dialougeIsPlaying = true;
        dialoguePanel.SetActive(true);
        dialougeVariables.StartListening(curStory);
        BindDialougeFunctions();
        // reset dialoge profile data
        displayNameText.text = "???";
        portaitAnimator.Play("default");
        layoutAnimator.Play("right");
        isDoneTalking=false;
        // shows first line of curStory
        ContinueStory();
    }
    public void BindDialougeFunctions() {
        if (curStory == null) {
            return;
        }
        curStory.BindExternalFunction("SwitchTeams", () => {
            switchTeamActivated = true;
            if (onSwitchTeamActivated != null) {

                onSwitchTeamActivated.Invoke();
            }
        });
        curStory.BindExternalFunction("ActivateStore", () => {
            activatedStore = true;
            if (onActivateStore != null) {
                onActivateStore.Invoke();
            }
        });
        curStory.BindExternalFunction("Intimidate", () => {
            intimidated = true;
            if (onIntimidated != null) {
                onIntimidated.Invoke();
            }
        });
        curStory.BindExternalFunction("Charm", () => {
            charmed = true;
            if (onCharmed != null) {
                onCharmed.Invoke();
            }
        });
        curStory.BindExternalFunction("Trick", () => {
            tricked = true;
            if (onTricked != null) {
                onTricked.Invoke();
            }
        });
    }
    public void UnbindDialougeFunctions() {
        if (switchTeamActivated) {
            curStory.UnbindExternalFunction("SwitchTeams");

        }
        if (activatedStore) { 
            curStory.UnbindExternalFunction("ActivateStore");
        }
        switchTeamActivated = false;
        activatedStore = false;

    }

    public IEnumerator ExitDialougeMode() {

        yield return new WaitForSeconds(0.2f);
        dialougeVariables.StopListening(curStory);
        UnbindDialougeFunctions();
        dialougeIsPlaying =false;
        dialoguePanel.SetActive(false);
        dialougeText.text = "";
        isDoneTalking = true;
        if(curCompleteAfterTalk!=null) {
            curCompleteAfterTalk();
        }
    }

    private void ContinueStory() {
        if (curStory.canContinue) {
            // gets first next line of dialouge
            if(displayLineCoroutine!=null) {
                StopCoroutine(displayLineCoroutine);
            }
            displayLineCoroutine = StartCoroutine(DisplayLine(curStory.Continue()));
            HandleTags(curStory.currentTags);
        }
        else {
            StartCoroutine(ExitDialougeMode());
        }
    }

    private IEnumerator DisplayLine(string line) {
        // overwrites dialouge text
        dialougeText.text = line;
        dialougeText.maxVisibleCharacters = 0;
        bool isRichText = false;
        // hies contineIcon and the choices
        continueIcon.SetActive(false);
        canContinueTyping = false;
        HideChoices();
        // displays each char of line
        foreach(char letter in line) {
            // skips typing if player preesses continue
            if (playerInputManger.IsContinue) {
                dialougeText.maxVisibleCharacters = line.Length;
                break;
            }
            // skips rich text tags
            if (letter == '<' || isRichText) {
                isRichText = true;
                if (letter == '>') {
                    isRichText = false;
                }
            }
            // displays each valid letter
            else {
                dialougeText.maxVisibleCharacters ++;
                yield return new WaitForSeconds(typingSpeed);
            } 
    
        }
        // actions for after the text is done typing
        canContinueTyping = true;
        continueIcon.SetActive(true);
        DisplayChoices();

    }

    private void HideChoices() {
        for (int i = 0; i < choices.childCount; i++) {
            choices.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void HandleTags(List<string> currentTags) {
        
        foreach (string tag in currentTags) {
            // parser the tag
            string[] splitTag = tag.Split(":");
            if(splitTag.Length != 2) {
                Debug.Log("ERROR tag of length " + splitTag.Length);
                continue;
            }

            string tagKey = splitTag[0];
            string tagValue = splitTag[1];
            // handles tag
            switch (tagKey) {
                case SPEAKER_TAG:
                   // Debug.Log("speaker = " + tagValue);
                    displayNameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:
                    //Debug.Log("portrait = " + tagValue);
                    portaitAnimator.Play(tagValue); 
                    break;
                case LAYOUT_TAG:
                    //Debug.Log("layout = " + tagValue);
                    layoutAnimator.Play(tagValue);
                    break;
                default:
                    Debug.Log("Tag found but no valid tag found with name of " + tagKey+" with val of "+tagValue);
                    break;
            }

        }
    }

    private void DisplayChoices() {
        List<Choice> curChoices = curStory.currentChoices;

        if (curChoices.Count > choices.childCount) {
            Debug.Log("error too many choices not enough boxes");
            return;
        }
        // fills the choices UI to match the choices of the current story
        for(int i = 0; i < curChoices.Count; i++) {
            choices.GetChild(i).gameObject.SetActive(true);
            choices.GetChild(i).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = curChoices[i].text;
        }
        //clears extra choices UI
        for (int i = choices.childCount-1; i >= (curChoices.Count); i --){
            choices.GetChild(i).gameObject.SetActive(false);
        }
        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice() {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices.GetChild(0).gameObject);
    }

    public void MakeChoice(int choiceIndex) {
        if(choiceIndex >= choices.childCount) {
            return;
        }
        //Debug.Log(curStory.currentChoices.Count + "   " + choiceIndex);
        curStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }

    public bool IsPlaying {
        get{ 
            return dialougeIsPlaying;
        }
        
    }

    public bool IsDoneTalking {
        get {
            return isDoneTalking;
        }
        set {
            isDoneTalking = value;
        }
    }
    public Action OnCompleteTalk { get { return curCompleteAfterTalk; } set { curCompleteAfterTalk = value; } }
 }
