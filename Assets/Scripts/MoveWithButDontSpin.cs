using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithButDontSpin : MonoBehaviour
{
    public Transform matchThisPosition;

    void LateUpdate()
    {
        transform.position = matchThisPosition.position;
    }
}
