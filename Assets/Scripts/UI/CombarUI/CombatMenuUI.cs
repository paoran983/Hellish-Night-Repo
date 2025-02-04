using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatMenuUI : MonoBehaviour
{
    [SerializeField] private List<CombatUIButton> buttons;
    [SerializeField] private List<CombatUIButton> initalButtons;
    public event Action<CombatUIButton>
        onButtonClick,
        onActionsMenuButtonClick,
        onStartDrag,
        onSelectButton,
        onDeselectButton,
        onPressButton,
        onReleaseButton,
        onItemMenuButtonClick,
        onMoveButtonClick,
        onWaitButtonClick,
        onPushButtonClick,
        onCaptureButtonClick,
        onAttackButtonClick,
        onMagicMenuButtonClick,
        onRunButtonClick,
        onTeamUpButtonClick,
        onSubmitTeamUpClick;
    public void InitalizeMenu()
    {
        foreach (CombatUIButton curInitButton in initalButtons)
        {
            if (curInitButton != null)
            {

                buttons.Add(curInitButton);
            }
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            CombatUIButton curButton = child.GetComponent<CombatUIButton>();
            if (curButton != null)
            {
                buttons.Add(curButton);

            }
        }
        foreach (CombatUIButton curButton in buttons)
        {
            curButton.InitalizeButton();
            if (curButton.type == CombatUIButton.State.Magic)
            {
                curButton.onButtonClicked += HandleMagicMenuButtonClick;

            }
            else if (curButton.type == CombatUIButton.State.Item)
            {
                curButton.onButtonClicked += HandleItemMenuButtonClick;

            }
            else if (curButton.type == CombatUIButton.State.Move)
            {
                curButton.onButtonClicked += HandleMoveButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.Wait)
            {
                curButton.onButtonClicked += HandleWaitButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.Push)
            {
                curButton.onButtonClicked += HandlePushButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.Run)
            {
                curButton.onButtonClicked += HandleRunButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.Capture)
            {
                curButton.onButtonClicked += HandleCaptureButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.Attack)
            {
                curButton.onButtonClicked += HandleAttackButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.Actions)
            {
                curButton.onButtonClicked += HandleActionsButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.TeamUp)
            {
                curButton.onButtonClicked += HandleTeamUpButtonClick;
            }
            else if (curButton.type == CombatUIButton.State.SubmitTeamUp)
            {
                curButton.onButtonClicked += HandleSubmitTeamUpButtonClick;
            }
            else
            {
                curButton.onButtonClicked += HandleButtonClick;
            }
            curButton.onButtonReleased += HandleButtonRelased;
            curButton.onPointerEnter += HandlePonterEnterButton;
            curButton.onPointerExit += HandlePointerExitBUtton;
        }

    }
    public void Activate(bool isActive)
    {


        if (isActive)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            foreach (CombatUIButton button in buttons)
            {
                button.Release();
                button.Deselct();
            }
            this.gameObject.SetActive(false);
        }


    }
    private void HandleWaitButtonClick(CombatUIButton button)
    {
        if (onWaitButtonClick != null)
        {
            onWaitButtonClick.Invoke(button);
        }
    }
    private void HandleTeamUpButtonClick(CombatUIButton button)
    {
        if (onTeamUpButtonClick != null)
        {
            onTeamUpButtonClick.Invoke(button);
        }
    }
    private void HandleSubmitTeamUpButtonClick(CombatUIButton button)
    {
        if (onSubmitTeamUpClick != null)
        {
            onSubmitTeamUpClick.Invoke(button);
        }
    }



    private void HandleMagicMenuButtonClick(CombatUIButton button)
    {
        if (onMagicMenuButtonClick != null)
        {
            onMagicMenuButtonClick.Invoke(button);
        }
    }

    private void HandleAttackButtonClick(CombatUIButton button)
    {
        if (onAttackButtonClick != null)
        {
            onAttackButtonClick.Invoke(button);
        }
    }

    private void HandleCaptureButtonClick(CombatUIButton button)
    {
        if (onCaptureButtonClick != null)
        {
            onCaptureButtonClick.Invoke(button);
        }
    }

    private void HandleActionsButtonClick(CombatUIButton button)
    {
        if (onActionsMenuButtonClick != null)
        {
            onActionsMenuButtonClick.Invoke(button);
        }
    }
    private void HandlePushButtonClick(CombatUIButton button)
    {
        if (onPushButtonClick != null)
        {
            onPushButtonClick.Invoke(button);
        }
    }
    private void HandleRunButtonClick(CombatUIButton button)
    {
        if (onRunButtonClick != null)
        {
            onRunButtonClick.Invoke(button);
        }
    }

    private void HandleMoveButtonClick(CombatUIButton button)
    {
        if (onMoveButtonClick != null)
        {
            onMoveButtonClick.Invoke(button);
        }
    }

    private void HandleItemMenuButtonClick(CombatUIButton button)
    {
        if (onItemMenuButtonClick != null)
        {
            onItemMenuButtonClick.Invoke(button);
        }
    }

    private void HandlePointerExitBUtton(CombatUIButton button)
    {

        if (onDeselectButton != null)
        {
            onDeselectButton.Invoke(button);
        }
    }

    private void HandlePonterEnterButton(CombatUIButton button)
    {

        if (onSelectButton != null)
        {
            onSelectButton.Invoke(button);
        }
    }

    private void HandleButtonRelased(CombatUIButton button)
    {

        if (onReleaseButton != null)
        {
            onReleaseButton.Invoke(button);
        }
    }

    private void HandleActionMenuButtonClick(CombatUIButton button)
    {
        if (onActionsMenuButtonClick != null)
        {
            onActionsMenuButtonClick.Invoke(button);
        }
    }

    private void HandleButtonClick(CombatUIButton button)
    {
        if (onButtonClick != null)
        {
            onButtonClick.Invoke(button);
        }
    }

}
