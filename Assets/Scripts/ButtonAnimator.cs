using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimator : MonoBehaviour
{
    public float changeTime = .3f;
    public float deltaTiling = .125f;
    public int materialIndex = 0;
    private Renderer renderer;
    private void Start()
    {
        renderer = GetComponent<Renderer>();
        StartCoroutine(ChangeButtonTiling());
    }

    IEnumerator ChangeButtonTiling()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeTime);
            float newX = renderer.materials[materialIndex].mainTextureOffset.x + deltaTiling;
            renderer.materials[materialIndex].mainTextureOffset = new Vector2(newX, 0);
        }
    }
}
