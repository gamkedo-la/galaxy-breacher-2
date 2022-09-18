using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSelfRemove : MonoBehaviour
{
    // public Transform effectToUnchild; // leaving whole self to remove, including child effects
    float destroyTimer = 10.0f;

    void Start()
    {
        // to do: physics spherecast to find all objects that the rocket should damage
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        Destroy(gameObject, destroyTimer);
    }
}
