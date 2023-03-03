using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeInterdictionSceneController : MonoBehaviour
{

    public static TradeInterdictionSceneController Instance;
    
    List<Freighter> freighters;

    private void Awake() {
        if(Instance != null){
            Debug.LogError("More than one TradeInterdictionController");
            Destroy(gameObject);
        }
        Instance = this;

    }

    private void Start() {
        freighters = new List<Freighter>(GameObject.FindObjectsOfType<Freighter>());
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.R)){
            MissionSuccess();
        }
    }

    public void FreighterReleasedCargo(Freighter freighter){
        freighters.Remove(freighter);

        if(freighters.Count == 0){
            MissionSuccess();
        }
    }

    private void MissionSuccess()
    {
        Debug.Log("Mission Won");
    }
}
