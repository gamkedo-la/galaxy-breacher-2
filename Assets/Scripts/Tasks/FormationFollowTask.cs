using System.Collections.Generic;
using UnityEngine;

public class FormationFollowTask : Task
{
    private List<Ship> ships = new List<Ship>();
    private Formation formation;

    public FormationFollowTask(Ship leader, Formation formation)
    {
        ships.Add(leader);
        this.formation = formation;
    }

    public override void Update()
    {
        formation.UpdateLeader(ships[0].transform.position, ships[0].transform.rotation);
        for (int i=1; i < ships.Count; i++)
        {
            ships[i].NavigateTo(formation.GetPosition(i), formation.GetOrientation(i), ships[i].maxSpeed);
        }
    }

    public void AddFollower(Ship follower)
    {
        ships.Add(follower);
    }

    public void RemoveFollower(Ship follower)
    {
        ships.Remove(follower);
    }
}
