using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithButDontSpin : MonoBehaviour
{
    public Transform matchThisPosition;

    private void Start() {
        if (matchThisPosition == null) { // avoids error on level select/menu screen version
            Destroy(this);
        }
    }

    void LateUpdate()
    {
        transform.position = matchThisPosition.position;
    }
}
