using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHullAndShield : MonoBehaviour
{
    public int shield = 8;
   
    public TextMeshProUGUI shieldText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shieldText.text = new string('|', shield);

       
    }


}
