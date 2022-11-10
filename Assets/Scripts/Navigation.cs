using UnityEngine;

namespace Navigation
{

public class State {
    public Vector3 position;
    public Quaternion orientation;

    public State(Vector3 position)
    {
        this.position = position;
        this.orientation = Quaternion.identity;
    }

    public State(Vector3 position, Quaternion orientation)
    {
        this.position = position;
        this.orientation = orientation;
    }

    public static State FromTransform(Transform transform)
    {
        return new State(transform.position, transform.rotation);
    }
}

public abstract class World
{
    public abstract bool BoxCollides(Vector3 center, Vector3 size, Quaternion orientation, int layerMask);
}

public class PhysicsWorld : World
{
    public override bool BoxCollides(Vector3 center, Vector3 size, Quaternion orientation, int layerMask)
    {
        return Physics.OverlapBox(center, size, orientation, layerMask).Length > 0;
    }
}

public class Agent
{
    // Offset of center of collision box
    public Vector3 offset;
    // Size of collision box
    public Vector3 size;
    public LayerMask collisionLayers;
}

public class Navigation3D
{
    World world;

    public Navigation3D(World world)
    {
        this.world = world;
    }

    private bool IsSegmentCollisionFree(Agent agent, State start, State end)
    {
        float step = Vector3.Distance(start.position, end.position) * 2 / agent.size.z;
        float progress = step;
        while (progress < 1.0)
        {
            Vector3 position = Vector3.Lerp(start.position, end.position, progress);
            Quaternion orientation = Quaternion.Slerp(start.orientation, end.orientation, progress);

            if (world.BoxCollides(position + agent.offset, agent.size, orientation, agent.collisionLayers))
            {
                return false;
            }

            progress += step;
        }
        return true;
    }

    public State[] GetPath(Agent agent, State start, State end)
    {
        if (IsSegmentCollisionFree(agent, start, end))
        {
            return new State[] { start, end };
        }

        // ??? More complex navigation

        return null;  // unreachable
    }
}

}  // namespace Navigation
