using UnityEngine;

public class NavigationTestController : MonoBehaviour
{
    public Ship[] ships;
    public Transform target;

    void Start()
    {
        if (ships.Length == 0)
        {
            Debug.Log("No ships configured");
            return;
        }

        Ship leader = ships[0];

        Task task = new TopLevelTask(new ChaseTask(leader, target));
        task.Start();
    }
}
