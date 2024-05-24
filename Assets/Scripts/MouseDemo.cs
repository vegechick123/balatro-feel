using UnityEngine;

public class MouseDemo : MonoBehaviour, IPressAble, IClickAble, IHoverable
{
    public void OnPressBegin()
    {
        Debug.Log("PressBegin");
    }

    public void OnPressEnd()
    {
        Debug.Log("PressEnd");
    }

    public void OnPressing()
    {
        Debug.Log($"{gameObject} Pressing");
    }

    public void OnClick()
    {
        Debug.Log("Click");
    }

    public void OnHover()
    {
        Debug.Log("Hover");
    }
}
