using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public class BasicButtonUI : ButtonUI, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {
    public override void InitalizeButton(String text) {
        this.buttonText.text = text;
        Deselct();
    }
}
