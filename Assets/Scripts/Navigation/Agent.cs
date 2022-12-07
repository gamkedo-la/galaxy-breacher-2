using System;
using UnityEngine;

namespace Navigation
{

[Serializable]
public class Agent
{
    // Offset of center of collision box
    public Vector3 offset;
    // Size of collision box
    public Vector3 size;
    public LayerMask collisionLayers;
}

}  // namespace Navigation
