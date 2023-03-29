using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ControllerUnlink();
public delegate void ControllerLink();

public class PlayerController : MonoBehaviour
{
    #region PublicKeyboardFields

    #region Movement
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode rightKey;
    public KeyCode leftKey;
    #endregion

    #region Actions
    public KeyCode interact;
    public KeyCode activeAbility;
    public KeyCode scurry;
    public KeyCode selfHeal;
    public KeyCode sprint;
    public KeyCode sneak;
    public KeyCode scoreboard;
    #endregion

    #endregion

    #region Mouse Input
    public string mouseX = "Mouse X";
    public string mouseY = "Mouse Y";
    #endregion

    #region Controller Input
    public string horizontal = "Horizontal";
    public string vertical = "Vertical";
    #endregion

    public ControlledBody linkedBody;
    [HideInInspector]
    public bool isLinked;

    public event ControllerLink OnControllerLink;
    public event ControllerUnlink OnControllerUnlink;

    private void Start()
    {
        isLinked = linkedBody != null ? true : false;

    }

    public void Link(ControlledBody newBody)
    {

        ControlledBody previousBody = linkedBody;

        linkedBody = newBody;
        if (newBody.linkedController != this)
            newBody.Link(this);

        if (previousBody != null)
            previousBody.Unlink();

        isLinked = true;
        OnControllerLink?.Invoke();

    }

    public void Unlink()
    {

        ControlledBody tempRef = linkedBody;
        linkedBody = null;

        if(tempRef.linkedController == this)
            tempRef.Unlink();

        isLinked = false;
        OnControllerUnlink?.Invoke();

    }

}