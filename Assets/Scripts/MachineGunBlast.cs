using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunBlast : MonoBehaviour
{
    [SerializeField]float maxLifetimeInSec = 2f;
    [SerializeField] float blastSpeed = 2.0f;


    private void Start()
    {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        Destroy(this.gameObject, maxLifetimeInSec);
    }

}
