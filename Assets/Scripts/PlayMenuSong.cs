using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMenuSong : MonoBehaviour {
    private static uint menuSongID = 0; // static so this survives between scene changes

    void Start() {
        PlayerControl.StopEngineLoopIfPlaying(); // no matter how we got here, we don't want to hear this
        SaveGameMusicAndPlayOneSongAtATime(AkSoundEngine.PostEvent("Game_Music", gameObject));
        AkSoundEngine.SetSwitch("Gameplay_Switch", "Menu", gameObject);
    }

    static void ForceEndMenuMusicIfItsPlaying() {
        if (menuSongID != 0) {
            AkSoundEngine.StopPlayingID(menuSongID, 500, AkCurveInterpolation.AkCurveInterpolation_Constant);
            menuSongID = 0;
        }
    }

    public static void SaveGameMusicAndPlayOneSongAtATime(uint songID) {
        ForceEndMenuMusicIfItsPlaying();
        menuSongID = songID;
    }
}
