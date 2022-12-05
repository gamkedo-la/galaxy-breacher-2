using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSelfRemove : MonoBehaviour
{
    float destroyTimer = 10.0f;
    public Transform explosionEffectPosition;
    public GameObject explosionVFX;

    void Start()
    {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        Destroy(gameObject, destroyTimer);
    }
}
