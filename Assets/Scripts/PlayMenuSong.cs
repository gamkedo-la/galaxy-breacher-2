using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMenuSong : MonoBehaviour
{
    void Start()
    {
        AkSoundEngine.SetSwitch("Gameplay_Switch","Menu", gameObject);
        AkSoundEngine.PostEvent("Game_Music", gameObject);
        
    }
}
