using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerShipUI : MonoBehaviour
{
    public static PlayerShipUI instance;

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

    private void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShieldRecharge());
        UpdateInterfaceReadout();
    }

    public bool IsBossPartCountZero() {
        return (parts == 0);
    }

    public void UpdateBossPartCount() {
        GameObject[] partsInScene = GameObject.FindGameObjectsWithTag("BossPart");
        parts = partsInScene.Length;
        for(int i=0;i<parts;i++) {
            ExplosionSelfRemove esr = partsInScene[i].GetComponentInParent<ExplosionSelfRemove>();
            if(esr) {
                if(esr.AlreadyRemoved()) {
                    parts--;
                    // Debug.Log(esr.gameObject.name + " was recently removed, removing from tally");
                }
            }
        }
        partsNumber.GetComponent<TextMeshProUGUI>().text = "Boss Parts: " + parts;
    }

    void UpdateInterfaceReadout() { // moved out of update code, can only update when it changes
        shield = Mathf.Clamp(shield, 0, SHIELD_MAX);
        shieldText.text = new string('|', shield);
        hullText.text = new string('|', hull);

        UpdateBossPartCount();
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
