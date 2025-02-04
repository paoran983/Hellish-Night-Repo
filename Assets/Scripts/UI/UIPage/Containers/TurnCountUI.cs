using UnityEngine;
using UnityEngine.UI;

public class TurnCountUI : MonoBehaviour {
    [SerializeField] private Image spriteRender;
    private Color empty = Color.white;
    [SerializeField] Color complete;

    public void Activate(bool isActive) {
        if (isActive) {
            spriteRender.color = complete;
        }
        else {
            spriteRender.color = empty;
        }
    }
}
