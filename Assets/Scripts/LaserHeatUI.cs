using UnityEngine;
using UnityEngine.UI;


public class LaserHeatUI : MonoBehaviour
{
    public Slider heatSlider;
    public Image heatFill;

    public void SetHeatPercentage(float percentage)
    {
        heatSlider.value = percentage;
    }
}
