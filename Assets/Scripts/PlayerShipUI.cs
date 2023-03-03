using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerShipUI : MonoBehaviour
{
    //Player Health and Shield
    public TextMeshProUGUI hullText;
    public TextMeshProUGUI shieldText;

    private static int SHIELD_MAX = 4;
    private static int HEALTH_MAX = 4;
    private int shield = SHIELD_MAX;
    private int hull = HEALTH_MAX;

    //Player Weapon Select
    public GameObject weaponSelect;

    //Boss Part Count
    private int bossPartsNumber;
    int parts;
    public TextMeshProUGUI partsNumber;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShieldRecharge());
        UpdateInterfaceReadout();
    }

    void UpdateInterfaceReadout() { // moved out of update code, can only update when it changes
        shield = Mathf.Clamp(shield, 0, SHIELD_MAX);
        shieldText.text = new string('|', shield);
        hullText.text = new string('|', hull);

                //Boss Part Count
        parts = GameObject.FindGameObjectsWithTag("BossPart").Length;
        partsNumber.GetComponent<TextMeshProUGUI>().text = "Boss Parts: " + parts;
    }
    public void TakeDamage() {
        if(shield > 0) {
            shield--;
        } else {
            AkSoundEngine.PostEvent("Player_Cabin_Warning", gameObject);
            hull--;
            if (hull < 0) {
                hull = 0;
                SceneManager.LoadScene("PlayerLost");
            }
        }
        UpdateInterfaceReadout();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            weaponSelect.SetActive(weaponSelect.activeSelf == false);
        }
    }
    //Player Health and Shield Recharge
    IEnumerator ShieldRecharge()
    {
        while(true) {
            yield return new WaitForSeconds(1f);
            if (shield < SHIELD_MAX)
            {
                shield++;
            }
        }
    }
    public void HealHull() {
        hull = HEALTH_MAX;
    }
}
