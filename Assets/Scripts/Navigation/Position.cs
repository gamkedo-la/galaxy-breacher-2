using System;
using UnityEngine;

namespace Navigation
{

public class Position : IEquatable<Position>
{
    public Vector3 position;
    public Quaternion rotation;

    public Position()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
    }

    public Position(Vector3 position)
    {
        this.position = position;
        this.rotation = Quaternion.identity;
    }

    public Position(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public Position(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
    }

    bool IEquatable<Position>.Equals(Position other)
    {
        return !(other is null) && position == other.position && rotation == other.rotation;
    }

    public bool Matches(Transform transform)
    {
        return !(transform is null) && position == transform.position && rotation == transform.rotation;
    }
}

}  // namespace Navigation
