using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHullAndShield : MonoBehaviour
{
    public int shield = 8;
    public int hull = 8;

    public TextMeshProUGUI hullText;
   
    public TextMeshProUGUI shieldText;

    bool Recharge = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
        if (hull <-0)
        {
            hull = 0;
        }

        
        
    }

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
