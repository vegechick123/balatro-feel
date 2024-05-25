using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLayout : MonoBehaviour
{
    public float spacing = 1f;
    
    public void Refresh()
    {
        List<Transform> childs = new();  
        for (int i = 0; i < transform.childCount; i++)
        {
            childs.Add(transform.GetChild(i));
        }
        float offset = -(childs.Count-1)*spacing / 2.0f;
        foreach (var child in childs)
        {
            //if(ignoreTransform != child)
            
            child.localPosition = new Vector3(offset, 0, 0);
            offset += spacing;
        }
    }
}
