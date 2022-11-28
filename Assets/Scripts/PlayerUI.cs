using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public GameObject weaponSelect;
    private bool on = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !on)
        {
            weaponSelect.SetActive(true);
            on = true;
        }
       else if (Input.GetKeyDown(KeyCode.M) && on)
        {
            weaponSelect.SetActive(false);
            on = false;
        }
    }
}
