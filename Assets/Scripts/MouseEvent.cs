using System;
using UnityEngine;

public interface IPressAble
{
    void OnPressBegin();
    void OnPressEnd();
    void OnPressing();
}
public interface IPointer
{
    void OnPointerEnter();
    void OnPointerExit();
}
public interface IClickAble
{
    void OnClick();
}
public interface IHoverable
{
    void OnHover();
}
[RequireComponent(typeof(BoxCollider))]
public class MouseEvent : MonoBehaviour
{
    public bool canClick = true;
    private static float clickTimeStamp = 0.1f;
    private static float pressTimeStamp = 0.3f;
    private static float dragThreshold = 1f;
    private float lastMouseDownTime = -100;

    private bool _mouseIsHovering = false;
    private bool _mouseIsDown = false;
    private bool _mouseIsPressing = false;
    private Vector2 _initialMouseDown = Vector2.zero;
    private IPressAble[] _pressAbleComponents;
    private IClickAble[] _clickAbleComponents;
    private IHoverable[] _hoverAbleComponents;
    private Camera _camera;
    protected void Awake()
    {
        _camera = Camera.main;
        Debug.Assert(_camera != null, "Camera.main != null");
        _clickAbleComponents = GetComponents<IClickAble>();
        _pressAbleComponents = GetComponents<IPressAble>();
        _hoverAbleComponents = GetComponents<IHoverable>();
        
    }
    
    public void CallMouseDown()
    {
        
        lastMouseDownTime = Time.time;
        _mouseIsDown = true;
        _initialMouseDown = Input.mousePosition;
    }

    public bool IsMouseBeyond()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        /*LayerMask layerMask = gameObject.layer;*/
        if (Physics.Raycast(ray,out var hit,100,1 << gameObject.layer)) {
            Transform objectHit = hit.transform;
            if (objectHit.GetComponentInParent<MouseEvent>() == this)
            {
                return true;
            }
        }

        return false;
    }
    
    private void Update()
    {
        if (_mouseIsHovering && !_mouseIsDown)
        {
            foreach (var comp in _hoverAbleComponents)
            {
                comp.OnHover();
            }
        }
        if (_mouseIsDown)
        {
            if (Time.time - lastMouseDownTime > pressTimeStamp || (new Vector2(Input.mousePosition.x,Input.mousePosition.y) - _initialMouseDown).sqrMagnitude > dragThreshold * dragThreshold)
            {
                if (_mouseIsPressing)
                {
                    foreach (var comp in _pressAbleComponents)
                    {
                        comp.OnPressing();
                    }
                }
                else
                {
                    foreach (var comp in _pressAbleComponents)
                    {
                        _mouseIsPressing = true;
                        comp.OnPressBegin();
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            CallMouseUp();
        }
        
        if (IsMouseBeyond())
        {
            _mouseIsHovering = true;
            foreach (var pointer in GetComponents<IPointer>())
            {
                pointer.OnPointerEnter();
            }
            if (Input.GetMouseButtonDown(0))
            {
                CallMouseDown();
            }
        }
        else if(_mouseIsHovering)
        {
            _mouseIsHovering = false;
            foreach (var pointer in GetComponents<IPointer>())
            {
                pointer.OnPointerExit();
            }
        }

        
    }
    
    public void CallMouseUp()
    {
        if (Time.time - lastMouseDownTime < clickTimeStamp)
        {
            foreach (var comp in _clickAbleComponents)
            {
                comp.OnClick();
            }
        }
        if (_mouseIsPressing)
        {
            foreach (var comp in _pressAbleComponents)
            {
                comp.OnPressEnd();
            }
        }
        ReleaseMouse();
    }

    void ReleaseMouse()
    {
        _mouseIsDown = false;
        _mouseIsPressing = false;
    }

    [ContextMenu("CalculateCollider")]
    void CalculateCollider()
    {
        GetComponent<BoxCollider>().size = GetComponent<SpriteRenderer>().bounds.size;
    }
    
    void OnValidate()
    {
        CalculateCollider();
    }
}
