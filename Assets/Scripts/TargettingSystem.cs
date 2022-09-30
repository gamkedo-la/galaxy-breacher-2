using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TargettingSystem : MonoBehaviour
{

    [SerializeField] TargettingHud targettingHud;
    [SerializeField] float range;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SphereCollider>().radius = range;
        targettingHud.TargettingSystemRange = range;
    }

    private void OnValidate() {
        GetComponent<SphereCollider>().radius = range;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out Target target)){
            targettingHud.AddTarget(target);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.TryGetComponent(out Target target)){
            targettingHud.RemoveTarget(target);
        }
    }
}
