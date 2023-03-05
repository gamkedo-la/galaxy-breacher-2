using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyJunkRemoveOnTimer : MonoBehaviour
{
    public float removeAfterSec = 5.0f;

    void Start()
    {
        if (HierarchyTrashSingleton.instance) {
            HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        }
        Destroy(gameObject, removeAfterSec);    
    }
}
