using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwisePlaySound : MonoBehaviour
{
    public string eventName;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(eventName+" is about to try to play");
        AkSoundEngine.PostEvent(eventName ,gameObject);
    }

}
