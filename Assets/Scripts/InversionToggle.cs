using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InversionToggle : MonoBehaviour
{

    Toggle inversionToggle;
    public PlayerControl playerControl;

    void Start()
    {
        inversionToggle = GetComponent<Toggle>();
    }

    void Update()
    {
        if (inversionToggle.isOn == true)
        {
            playerControl.SetInversion(true);
        }

        else
        {
            playerControl.SetInversion(false);
        }
    }
}
