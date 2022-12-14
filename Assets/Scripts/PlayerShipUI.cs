using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerShipUI : MonoBehaviour
{
    //Player Health and Shield
    public int shield = 8;
    public int hull = 8;
    public TextMeshProUGUI hullText;
    public TextMeshProUGUI shieldText;
    bool Recharge = false;

    //Player Weapon Select
    public GameObject weaponSelect;
    private bool on = false;

    //Boss Part Count
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
        //Player Health and Shield Recharge
        shieldText.text = new string('|', shield);
        hullText.text = new string('|', hull);
        shield = Mathf.Clamp(shield, 0, 8);
        if (Input.GetKeyDown(KeyCode.T))
        {
            shield -= 1;
        }
        if (Input.GetKeyDown(KeyCode.T) && shield <= 0)
        {
            hull -= 1;
        }
        if (shield <= 0)
        {
            Recharge = true;

            shield = 0;


        }
        if (shield <= 0)
        {
            StartCoroutine(ShieldRecharge());
        }
        if (hull < -0)
        {
            hull = 0;
        }

        //Player Weapon Select
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


        //Boss Part Count
        parts = GameObject.FindGameObjectsWithTag("BossPart").Length;
        partsNumber.GetComponent<TextMeshProUGUI>().text = "Boss Parts: " + parts;
    }
    //Player Health and Shield Recharge
    IEnumerator ShieldRecharge()
    {
        if (shield >= 0)
        {
            yield return new WaitForSeconds(1f);
            shield += 1;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "HullPickUp" && hull < 1)
        {
            hull = 8;
        }
    }
}
