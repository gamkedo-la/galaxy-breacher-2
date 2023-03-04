using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSettings : MonoBehaviour
{
    public Toggle inversionToggle;
    const string checkInversionToggle = "INVERSION";

    private void Awake()
    {
        if (inversionToggle == null)
        {
            return;
        }

        if (PlayerPrefs.GetInt(checkInversionToggle) == 1)
        {
            inversionToggle.isOn = true;
        }
        else
        {
            inversionToggle.isOn = false;
        }
    }

    public void SetInversion()
    {
        if (inversionToggle == null)
        {
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

        PlayerPrefs.Save();
    }
}
