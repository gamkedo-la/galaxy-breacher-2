using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BossPartsCount : MonoBehaviour
{
    private int bossPartsNumber;
    int parts;
    
    public TextMeshProUGUI partsNumber;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        parts = GameObject.FindGameObjectsWithTag("BossPart").Length;
        partsNumber.GetComponent<TextMeshProUGUI>().text = "Boss Parts: " + parts;

    }
}
