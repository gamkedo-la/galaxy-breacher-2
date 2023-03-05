using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchCenter : MonoBehaviour
{
    public GameObject[] listGO;

    private void LateUpdate() {
        Vector3 sumPos = Vector3.zero;
        for(int i=0;i< listGO.Length;i++) {
            if(listGO[i] == null) {
                return; // avoid errors, level is ending
            }
            sumPos += listGO[i].transform.position;
        }
        transform.position = (sumPos/listGO.Length);
    }
}
