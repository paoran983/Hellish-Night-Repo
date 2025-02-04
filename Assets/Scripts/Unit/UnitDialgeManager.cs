using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDialgeManager : MonoBehaviour
{
    [SerializeField] private TextAsset curText;
    [SerializeField] private List<TextAsset> textList;
    [SerializeField] private TextAsset deafultText;
    [SerializeField] private TextAsset combatText;

    public void Start() {

        textList.Add(deafultText);
    }
    public TextAsset CurText { get { return curText; } set { curText = value; } }
    public List<TextAsset> TextList { get {  return textList; } set { textList = value; } }
    public TextAsset DeafultText { get { return deafultText; } set {  deafultText = value; } }
    public TextAsset ComatText { get { return combatText; } set {  combatText = value; } }
    
}
