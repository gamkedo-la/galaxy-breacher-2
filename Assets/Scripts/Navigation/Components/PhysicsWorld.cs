using UnityEngine;

[ExecuteAlways]
public class PhysicsWorld : World
{
    private class AWorld : Navigation.World
    {
        public override bool LineCollides(Vector3 p1, Vector3 p2, int layerMask)
        {
            return Physics.Raycast(p1, p2 - p1, Vector3.Distance(p1, p2), layerMask);
        }

        public override bool BoxCollides(Vector3 center, Vector3 size, Quaternion orientation, int layerMask)
        {
            return Physics.OverlapBox(center, size, orientation, layerMask).Length > 0;
        }
    }

    private AWorld _world;
    public override Navigation.World world {
        get {
            if (_world is null)
                _world = new AWorld();
            return _world;
        }
    }
}
