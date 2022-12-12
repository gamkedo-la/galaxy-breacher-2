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
        if (Mathf.Approximately(ships[0].GetComponent<Rigidbody>().velocity.magnitude, 0f))
            return;

        List<Ship> shipsToAssign = new List<Ship>();
        for (int i=1; i < ships.Count; i++)
            shipsToAssign.Add(ships[i]);

        for (int i=1; i < ships.Count; i++)
        {
            Vector3 position = formation.GetPosition(i);

            float closestDistance = float.PositiveInfinity;
            Ship closestShip = null;
            foreach (Ship ship in shipsToAssign)
            {
                float distance = Vector3.Distance(ship.transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestShip = ship;
                }
            }

            shipsToAssign.Remove(closestShip);
            closestShip.NavigateTo(position, formation.GetOrientation(i), closestShip.maxSpeed);
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
