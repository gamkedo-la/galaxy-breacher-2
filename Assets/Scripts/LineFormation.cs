using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineFormation : Formation
{
    private float spread;
    private Vector3 leaderPosition;
    private Quaternion leaderOrientation;

    public LineFormation(float spread)
    {
        this.spread = spread;
    }

    public override void UpdateLeader(Vector3 position, Quaternion orientation)
    {
        leaderPosition = position;
        leaderOrientation = orientation;
    }

    public override Vector3 GetPosition(int index)
    {
        if (index < 0)
        {
            throw new ArgumentException("Invalid formation unit index");
        }
        else if (index == 0)
        {
            return leaderPosition;
        }

        return leaderPosition +  leaderOrientation * Vector3.right * ((index % 2) * 2 -1) * spread * ((index + 1) / 2);
    }

    public override Quaternion GetOrientation(int index)
    {
        return leaderOrientation;
    }
}
