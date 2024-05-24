using UnityEngine;
[RequireComponent(typeof(MouseEvent))]
public class Draggable : MonoBehaviour,IPressAble
{
    public bool canDrag = true;
    private Camera _camera;


    public bool BeDragging { get; private set; }

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnMouseDrag()
    {
        if (!canDrag) return;
    }
    
    public void OnPressBegin()
    {
        if (canDrag)
        {
            Debug.Log("Begin");
        }
    }

    public void OnPressEnd()
    {
        if (canDrag)
        {
            Debug.Log("End");
        }
    }

    public void OnPressing()
    {
        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            transform.position.z - _camera.transform.position.z);
        Vector3 pos = _camera.ScreenToWorldPoint(screenPos);
        Debug.Log($"{Input.mousePosition}{pos}");
        transform.position = pos;//new Vector3(pos.x, pos.y ,-1f);
    }
}
