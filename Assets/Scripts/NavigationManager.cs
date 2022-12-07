using System;
using System.Collections.Generic;
using UnityEngine;
using Navigation;


[RequireComponent(typeof(World))]
public class NavigationManager : MonoBehaviour
{
    private static NavigationManager _instance = null;
    public static NavigationManager instance { get { return _instance; } }

    private Navigation3D _navigation;
    protected Navigation3D navigation
    {
        get {
            if (_navigation is null)
                _navigation = new Navigation3D(world, navMap);
            return _navigation;
        }
    }

    private Navigation.World _world = null;
    public Navigation.World world {
        get {
            if (_world is null)
                _world = GetComponent<World>().world;
            return _world;
        }
    }
    public NavMap navMap = new NavMap();

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    public Position[] GetPath(Agent agent, Position start, Position end)
    {
        return navigation.GetPath(agent, start, end);
    }
}
