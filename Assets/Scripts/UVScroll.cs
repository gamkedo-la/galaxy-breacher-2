using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScroll : MonoBehaviour {
    float scrollSpeed = 0.05f;
    Renderer rend;

    void Start() {
        rend = GetComponent<Renderer>();
    }

    void Update() {
        float offset = Time.time * scrollSpeed;
        rend.material.SetTextureOffset("_BaseMap", new Vector2(offset*0.33f, offset));
    }
}
