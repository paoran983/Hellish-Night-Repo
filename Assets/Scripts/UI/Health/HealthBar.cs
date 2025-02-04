using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    [SerializeField] private Slider slider;
    [SerializeField] private Image fill;
    [SerializeField] private Image backgorund;

    public void SetSlider(float amount) {
        slider.value = amount;
    }
    public void SetSliderMax(float amount) {
        slider.maxValue = amount;
       // SetSlider(amount);
    }

    public Slider Slider {  get { return slider; } }
    public Image Fill { get { return fill; } set { fill = value; } }
    public Image Backgorund { get { return backgorund; } set{ backgorund = value; } }

   
}
