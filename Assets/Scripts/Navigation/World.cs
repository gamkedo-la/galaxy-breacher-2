using UnityEngine;

namespace Navigation
{

public abstract class World
{
    public abstract bool LineCollides(Vector3 p1, Vector3 p2, int layerMask);
    public abstract bool BoxCollides(Vector3 center, Vector3 size, Quaternion orientation, int layerMask);
}

}  // namespace Navigation
