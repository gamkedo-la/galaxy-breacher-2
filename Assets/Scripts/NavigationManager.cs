using UnityEngine;
using Navigation;

public class NavigationManager : MonoBehaviour
{
    private static NavigationManager _instance = null;
    public static NavigationManager instance { get { return _instance; } }

    private Navigation3D navigation;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        navigation = new Navigation3D(new PhysicsWorld());
    }

    public State[] GetPath(Agent agent, State start, State end)
    {
        return navigation.GetPath(agent, start, end);
    }
}
