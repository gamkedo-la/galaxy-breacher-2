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
        containThis.SetParent(transform);
    }
}
