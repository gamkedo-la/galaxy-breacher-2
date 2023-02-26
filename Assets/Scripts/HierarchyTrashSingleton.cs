using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyTrashSingleton : MonoBehaviour
{
    public static HierarchyTrashSingleton instance;

    void Awake()
    {
        instance = this;
    }

    public void GroupTempJunk(Transform containThis)
    {
        if(transform==null) {
            Debug.Log("object already destroyed before it could go in group temp junk");
            return;
        }
        containThis.SetParent(transform);
    }
}
