using UnityEngine;

namespace Navigation
{

public static class Utils
{
    public static bool IsCollisionFree(World world, Agent agent, Position start, Position end)
    {
        // Do a quick line of sight check first
        if (world.LineCollides(start.position, end.position, agent.collisionLayers))
        {
            return false;
        }

        float step = Vector3.Distance(start.position, end.position) * 2 / agent.size.z;
        float progress = step;
        while (progress < 1.0)
        {
            Vector3 position = Vector3.Lerp(start.position, end.position, progress);
            Quaternion rotation = Quaternion.Slerp(start.rotation, end.rotation, progress);

            if (world.BoxCollides(position + agent.offset, agent.size, rotation, agent.collisionLayers))
            {
                return false;
            }

            progress += step;
        }

        return true;
    }

    // Faster Line Segment Intersection
    public static bool LinesIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float Ax = p2.x - p1.x;
        float Bx = p3.x - p4.x;

        float xmin, xmax;
        if (Ax < 0)
        {
            xmin = p2.x; xmax = p1.x;
        }
        else
        {
            xmin = p1.x; xmax = p2.x;
        }
        if (Bx > 0)
        {
            if (xmax < p4.x || p3.x < xmin)
                return false;
        }
        else
        {
            if (xmax < p3.x || p4.x < xmin)
                return false;
        }

        float Ay = p2.y - p1.y;
        float By = p3.y - p4.y;
        float ymin, ymax;
        if (Ay < 0)
        {
            ymin = p2.y; ymax = p1.y;
        }
        else
        {
            ymin = p1.y; ymax = p2.y;
        }
        if (By > 0)
        {
            if (ymax < p4.y || p3.y < ymin)
                return false;
        }
        else
        {
            if (ymax < p3.y || p4.y < ymin)
                return false;
        }

        float Cx = p1.x - p3.x;
        float Cy = p1.y - p3.y;

        float f = Ay * Bx - Ax * By;
        if (Mathf.Approximately(f, 0f))
            return false;

        float d = By * Cx - Bx * Cy;
        if (f > 0f)
        {
            if (d < 0f || d > f)
                return false;
        }
        else
        {
            if (d > 0f || d < f)
                return false;
        }

        float e = Ax * Cy - Ay * Cx;
        if (f > 0)
        {
            if (e < 0f || e > f)
                return false;
        }
        else
        {
            if (e > 0f || e < f)
                return false;
        }

        float num_x = d * Ax;
        float num_y = d * Ay;
        float offset_x = Mathf.Sign(num_x * f) * f / 2f;
        float offset_y = Mathf.Sign(num_y * f) * f / 2f;
        intersection = new Vector2(
            p1.x + (num_x + offset_x) / f,
            p1.y + (num_y + offset_y) / f
        );
        return true;
    }

    public static bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        return PointsOnDifferentSides(p1, p2, p3, p4) && PointsOnDifferentSides(p3, p4, p1, p2);
    }

    public static bool PointsOnDifferentSides(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 direction = p2 - p1;
        Vector2 normal = new Vector2(-direction.y, direction.x);

        return (Vector2.Dot(normal, p3 - p1) * Vector2.Dot(normal, p4 - p1) < 0f);
    }

}

}  // namespace Navigation
