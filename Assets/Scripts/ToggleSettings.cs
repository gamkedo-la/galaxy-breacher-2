using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSettings : MonoBehaviour
{
    static Toggle inversionToggle;
    const string checkInversionToggle = "INVERSION";

    private void Start()
    {
        if(inversionToggle == null) {
            Debug.LogWarning("inversionToggle was not set?");
            return;
        }
        if ((PlayerPrefs.GetInt(checkInversionToggle) == 1))
        {
            inversionToggle.isOn = true;
        }
        else
        {
            inversionToggle.isOn = false;
        }
    }

    private void Update()
    {
        SetInversion();
    }


    private static void SetInversion()
    {
        if (inversionToggle == null) {
            Debug.LogWarning("inversionToggle was not set?");
            return;
        }
        if (inversionToggle.isOn == true)
        {
            PlayerPrefs.SetInt(checkInversionToggle, 1);
        }
        else
        {
            PlayerPrefs.SetInt(checkInversionToggle, 0);
        }

    }
}
