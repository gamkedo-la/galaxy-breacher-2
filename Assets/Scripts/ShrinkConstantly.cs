using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkConstantly : MonoBehaviour
{
    float scaleAfterEachFixedTick = 0.98f;

    void FixedUpdate()
    {
        transform.localScale *= scaleAfterEachFixedTick;
    }
}
