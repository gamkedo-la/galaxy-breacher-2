using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{

public class Navigation3D
{
    public World world;
    public NavMap map;

    public Navigation3D(World world, NavMap map)
    {
        this.world = world;
        this.map = map;
    }

    public Position[] GetPath(Agent agent, Position start, Position end)
    {
        if (Navigation.Utils.IsCollisionFree(world, agent, start, end))
        {
            return new Position[] { start, end };
        }

        int startPointId = map.GetClosestNavPoint(start.position);
        int endPointId = map.GetClosestNavPoint(end.position);
        if (startPointId == -1 || endPointId == -1)
            return null;

        List<int> pathPointIds = map.GetPath(startPointId, endPointId);
        if (pathPointIds is null)
            return null;

        List<Position> path = new List<Position>();
        path.Add(start);

        Position prevPosition = start;
        for (int i=0; i < pathPointIds.Count; i++)
        {
            Vector3 nextPosition = (i < pathPointIds.Count - 1)
                ? map.GetNavPoint(pathPointIds[i+1]).position : end.position;

            // Eliminate unnecessary steps
            // if (Navigation.Utils.IsCollisionFree(world, agent, prevPosition, new Position(nextPosition)))
            // {
            //     continue;
            // }

            NavPoint point = map.GetNavPoint(pathPointIds[i]);

            Vector3 prevDirection = (point.position - prevPosition.position).normalized;
            Vector3 nextDirection = (nextPosition - point.position).normalized;
            Vector3 direction = (prevDirection + nextDirection) * 0.5f;

            Vector3 up = Quaternion.AngleAxis(90f, Vector3.Cross(prevDirection, nextDirection)) * direction;

            Position position = new Position(point.position, Quaternion.LookRotation(direction, up));
            path.Add(position);

            prevPosition = position;
        }

        path.Add(end);

        return path.ToArray();
    }
}

}  // namespace Navigation
