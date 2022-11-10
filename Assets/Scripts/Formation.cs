using UnityEngine;


public abstract class Formation
{
    public abstract void UpdateLeader(Vector3 position, Quaternion orientation);
    // Returns position of index'th unit in a formation with 0 being leader
    public abstract Vector3 GetPosition(int index);
    // Returns orientation of index'th unit in a formation with 0 being leader
    public abstract Quaternion GetOrientation(int index);
}
