using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSelfRemove : MonoBehaviour
{
    float destroyTimer = 2.0f;

    void Start()
    {
        //  HierarchyTrashSingleton.instance.GroupTempJunk(transform);
    }

    public void Remove()
    {
        Destroy(gameObject, destroyTimer);
        Debug.Log("Destroy");

    }


}
