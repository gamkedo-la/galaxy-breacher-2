using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{

[Serializable]
public class NavPoint
{
    public int id { get; set; }  // Do not serialize this field
    public Vector3 position;

    public NavPoint()
    {
        id = 0;
        position = Vector3.zero;
    }

    public NavPoint(Vector3 position)
    {
        id = 0;
        this.position = position;
    }
}


[Serializable]
public class NavMap : ISerializationCallbackReceiver
{
    [Serializable]
    class EdgeData
    {
        public int point1;
        public int point2;
    }

    private List<NavPoint> points = new List<NavPoint>();
    private Dictionary<int, NavPoint> pointsById = new Dictionary<int, NavPoint>();
    private Dictionary<int, HashSet<int>> edges = new Dictionary<int, HashSet<int>>();

    private int nextId = 1;

    public int PointCount { get { return points.Count; } }

    public void Clear()
    {
        points.Clear();
        pointsById.Clear();
        edges.Clear();
        nextId = 1;
    }

    public NavPoint GetNavPoint(int id)
    {
        if (!pointsById.ContainsKey(id))
        {
            throw new ArgumentException("Point with given ID does not exist");
        }
        return pointsById[id];
    }

    public IEnumerable<NavPoint> GetNavPoints()
    {
        return points;
    }

    public int AddNavPoint(NavPoint point)
    {
        point.id = nextId++;
        points.Add(point);
        pointsById[point.id] = point;
        return point.id;
    }

    public void UpdateNavPoint(int pointId, Vector3 position)
    {
        if (!pointsById.ContainsKey(pointId))
        {
            throw new ArgumentException("Point with given ID does not exist");
        }

        pointsById[pointId].position = position;
    }

    public void RemoveNavPoint(NavPoint point)
    {
        points.Remove(point);
        RemovePointEdges(point.id);
    }

    public void RemoveNavPoint(int id)
    {
        if (!pointsById.ContainsKey(id))
        {
            throw new ArgumentException("Point with given ID does not exist");
        }
        RemoveNavPoint(pointsById[id]);
    }

    protected void RemovePointEdges(int pointId)
    {
        if (!edges.ContainsKey(pointId))
        {
            return;
        }

        foreach (int otherPointId in edges[pointId])
        {
            edges[otherPointId].Remove(pointId);
        }

        edges.Remove(pointId);
    }

    public IEnumerable<NavPoint> GetConnectedNavPoints(NavPoint point)
    {
        return GetConnectedNavPoints(point.id);
    }

    public bool AreConnected(int pointId1, int pointId2)
    {
        return edges.ContainsKey(pointId1) && edges[pointId1].Contains(pointId2);
    }

    public bool AreConnected(NavPoint p1, NavPoint p2)
    {
        return AreConnected(p1.id, p2.id);
    }

    public IEnumerable<NavPoint> GetConnectedNavPoints(int pointId)
    {
        if (!edges.ContainsKey(pointId))
        {
            yield break;
        }

        foreach (int otherPointId in edges[pointId])
        {
            yield return pointsById[otherPointId];
        }
    }

    public void Connect(int pointId1, int pointId2)
    {
        if (!pointsById.ContainsKey(pointId1) || !pointsById.ContainsKey(pointId2))
        {
            throw new ArgumentException("Point with given ID does not exist");
        }

        if (!edges.ContainsKey(pointId1))
        {
            edges[pointId1] = new HashSet<int>();
        }
        edges[pointId1].Add(pointId2);

        if (!edges.ContainsKey(pointId2))
        {
            edges[pointId2] = new HashSet<int>();
        }
        edges[pointId2].Add(pointId1);
    }

    public void Connect(NavPoint point1, NavPoint point2)
    {
        Connect(point1.id, point2.id);
    }

    public void Disconnect(int pointId1, int pointId2)
    {
        if (edges.ContainsKey(pointId1))
        {
            edges[pointId1].Remove(pointId2);
        }

        if (edges.ContainsKey(pointId2))
        {
            edges[pointId2].Remove(pointId1);
        }
    }

    public void Disconnect(NavPoint point1, NavPoint point2)
    {
        Disconnect(point1.id, point2.id);
    }

    public int GetClosestNavPoint(Vector3 position)
    {
        int minId = -1;
        float minDistance = float.PositiveInfinity;
        foreach (NavPoint point in points)
        {
            float distance = Vector3.Distance(position, point.position);
            if (distance < minDistance)
            {
                minId = point.id;
                minDistance = distance;
            }
        }

        return minId;
    }

    class MinPriorityQueue<T>
    {
        struct Item
        {
            public float priority;
            public T value;

            public Item(float priority, T value)
            {
                this.priority = priority;
                this.value = value;
            }
        }

        private List<Item> elements = new List<Item>();

        public int Count { get { return elements.Count; } }
        public bool IsEmpty { get { return elements.Count == 0; } }

        public void Add(float priority, T value)
        {
            Item item = new Item(priority, value);

            for (int i=0; i < elements.Count; i++)
            {
                if (elements[i].priority > priority)
                {
                    elements.Insert(i, item);
                    return;
                }
            }

            elements.Add(item);
        }

        public T PopTop()
        {
            if (elements.Count < 1)
            {
                throw new InvalidOperationException("No elements exist");
            }

            Item item = elements[0];
            elements.RemoveAt(0);
            return item.value;
        }
    }

    public List<int> GetPath(int startPointId, int endPointId)
    {
        if (!pointsById.ContainsKey(startPointId) || !pointsById.ContainsKey(endPointId))
        {
            throw new ArgumentException("Point with given ID does not exist");
        }

        NavPoint startPoint = pointsById[startPointId];
        NavPoint endPoint = pointsById[endPointId];

        HashSet<int> seenPoints = new HashSet<int>();
        Dictionary<int, int> parents = new Dictionary<int, int>();
        Dictionary<int, float> costs = new Dictionary<int, float>();
        parents[startPointId] = -1;
        costs[startPointId] = 0f;

        MinPriorityQueue<int> queue = new MinPriorityQueue<int>();
        queue.Add(Vector3.Distance(startPoint.position, endPoint.position), startPointId);

        while (!queue.IsEmpty)
        {
            int currentPointId = queue.PopTop();
            if (seenPoints.Contains(currentPointId))
            {
                continue;
            }

            NavPoint currentPoint = pointsById[currentPointId];
            float currentCost = costs[currentPointId];
            foreach (NavPoint neighbour in GetConnectedNavPoints(currentPointId))
            {
                float neighbourCost = currentCost + Vector3.Distance(currentPoint.position, neighbour.position);
                if (!costs.ContainsKey(neighbour.id) || costs[neighbour.id] > neighbourCost)
                {
                    parents[neighbour.id] = currentPointId;
                    costs[neighbour.id] = neighbourCost;
                    queue.Add(neighbourCost + Vector3.Distance(neighbour.position, endPoint.position), neighbour.id);
                }
            }
        }

        if (!parents.ContainsKey(endPointId))
        {
            // Path not found
            return null;
        }

        // Construct a path
        List<int> path = new List<int>();
        int x = endPointId;
        while (x != -1)
        {
            path.Add(x);
            x = parents[x];
        }
        path.Reverse();

        return path;
    }

    [SerializeField]
    [HideInInspector]
    private NavPoint[] m_points;

    [SerializeField]
    [HideInInspector]
    private EdgeData[] m_edges;

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        m_points = new NavPoint[points.Count];

        int edgeCount = 0;
        Dictionary<int, int> pointId2Index = new Dictionary<int, int>();
        for (int i=0; i < points.Count; i++)
        {
            pointId2Index[points[i].id] = i + 1;
            m_points[i] = points[i];

            if (edges.ContainsKey(points[i].id))
            {
                foreach (var otherPointId in edges[points[i].id])
                {
                    if (otherPointId < points[i].id)
                    {
                        continue;
                    }

                    edgeCount++;
                }
            }
        }

        m_edges = new EdgeData[edgeCount];
        int edgeIndex = 0;
        foreach (var point in points)
        {
            if (!edges.ContainsKey(point.id))
            {
                continue;
            }

            foreach (int otherPointId in edges[point.id])
            {
                if (otherPointId < point.id)
                {
                    continue;
                }

                EdgeData edge = new EdgeData();
                edge.point1 = pointId2Index[point.id];
                edge.point2 = pointId2Index[otherPointId];
                m_edges[edgeIndex++] = edge;
            }
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        points = new List<NavPoint>(m_points);
        pointsById = new Dictionary<int, NavPoint>();
        for (int i=0; i < points.Count; i++)
        {
            points[i].id = i + 1;
            pointsById[points[i].id] = points[i];
        }

        nextId = points.Count + 1;

        edges = new Dictionary<int, HashSet<int>>(points.Count);
        foreach (var edge in m_edges)
        {
            if (!edges.ContainsKey(edge.point1))
            {
                edges[edge.point1] = new HashSet<int>();
            }

            if (!edges.ContainsKey(edge.point2))
            {
                edges[edge.point2] = new HashSet<int>();
            }

            edges[edge.point1].Add(edge.point2);
            edges[edge.point2].Add(edge.point1);
        }

        m_points = null;
        m_edges = null;
    }
}

}  // namespace Navigation
