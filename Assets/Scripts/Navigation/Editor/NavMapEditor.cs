using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditorInternal;
using Navigation;

using CompareFunction = UnityEngine.Rendering.CompareFunction;

[CustomEditor(typeof(NavigationManager))]
public class NavMapEditor : Editor
{
    const float NAVPOINT_MARKER_RADIUS = 10f;
    Color CONNECTION_NORMAL_COLOR = new Color(0, 0.2f, 0.2f, 0.5f); // Dark transparent cyan
    Color CONNECTION_HIGHLIGHT_COLOR = Color.cyan;

    NavMap navMap { get { return (target as NavigationManager).navMap; } }
    Navigation.World world { get { return (target as NavigationManager).gameObject.GetComponent<World>().world; } }
    NavigationAgent navigationAgent;
    Navigation.Agent agent { get { return navigationAgent?.agent; } }

    bool editing = false;
    int selected = -1;

    enum NavPointDisplay
    {
        Normal,
        Highlighted,
        Selected
    }

    enum ConnectionDisplay
    {
        Normal,
        Highlighted,
        Blocked
    }

    enum EditMode
    {
        Normal,
        Moving,
        Connecting,
        Disconnecting,
        GenerateMap
    }

    EditMode editMode = EditMode.Normal;

    Bounds generateMapBounds;
    LayerMask generateMapLayerMask = 0;
    float generateMapExpandAmount = 50f;
    int generateMapNumberOfPoints = 100;

    Mesh navpointMesh = null;
    Material navpointMaterial = null;
    Material navpointHighlightedMaterial = null;

    void OnEnable()
    {
        InitNavpointGraphics();

        editing = false;
        editMode = EditMode.Normal;
        selected = -1;
    }

    void InitNavpointGraphics()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        navpointMesh = obj.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(obj);

        navpointMaterial = new Material(Shader.Find("Unlit/Color"));
        navpointMaterial.color = new Color(0.5f, 0f, 0f, 1f);

        navpointHighlightedMaterial = new Material(Shader.Find("Unlit/Color"));
        navpointHighlightedMaterial.color = new Color(1f, 0f, 0f, 1f);
    }

    Matrix4x4 PointMatrix(Vector3 point)
    {
        return Matrix4x4.TRS(point, Quaternion.identity, Vector3.one * NAVPOINT_MARKER_RADIUS);
    }

    Bounds EstimateMapBounds(LayerMask layerMask)
    {

        bool boundsInitialized = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        for (int sceneIndex=0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Collider collider in root.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled || (((1 << collider.gameObject.layer) & layerMask) == 0))
                    {
                        continue;
                    }

                    if (!boundsInitialized)
                    {
                        bounds = collider.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }
        }

        return bounds;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Navigation Manager");

        navigationAgent = EditorGUILayout.ObjectField("Default agent", navigationAgent, typeof(NavigationAgent), /*allowSceneObjects=*/true) as NavigationAgent;

        if (!editing)
        {
            GUI.enabled = !(agent is null);
            if (GUILayout.Button("Edit map"))
            {
                editing = true;
                ToolManager.SetActiveTool((EditorTool)null);
                editMode = EditMode.Normal;
                selected = -1;
                Repaint();
                SceneView.RepaintAll();
            }
            if (agent is null)
            {
                EditorGUILayout.HelpBox("You need to pick default agent to edit map", MessageType.Warning);
            }
            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Stop editing map"))
            {
                editing = false;
                editMode = EditMode.Normal;
                GUIUtility.hotControl = 0;
                selected = -1;
                Repaint();
                SceneView.RepaintAll();
            }

            switch (editMode)
            {
                case EditMode.Normal:
                    {
                        GUI.enabled = true;
                        if (GUILayout.Button("Add navpoint")) {
                            Camera camera = SceneView.lastActiveSceneView.camera;
                            Vector3 position = camera.transform.position + camera.transform.forward * 100f;
                            Undo.RecordObject(target, "Add NavPoint");
                            selected = navMap.AddNavPoint(new NavPoint(position));
                            EditorUtility.SetDirty(target);
                            Repaint();
                            SceneView.RepaintAll();
                        }

                        GUI.enabled = selected != -1;
                        if (GUILayout.Button("Remove navpoint")) {
                            Undo.RecordObject(target, "Remove NavPoint");
                            navMap.RemoveNavPoint(selected);
                            EditorUtility.SetDirty(target);
                            selected = -1;
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        if (GUILayout.Button("Move navpoint")) {
                            editMode = EditMode.Moving;
                            SceneView.lastActiveSceneView.Focus();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        if (GUILayout.Button("Connect")) {
                            editMode = EditMode.Connecting;
                            SceneView.lastActiveSceneView.Focus();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        if (GUILayout.Button("Connect All")) {
                            Undo.RecordObject(target, "Connect NavPoint to all others");
                            ConnectAll(selected);
                            EditorUtility.SetDirty(target);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        if (GUILayout.Button("Disconnect")) {
                            editMode = EditMode.Disconnecting;
                            SceneView.lastActiveSceneView.Focus();
                            Repaint();
                            SceneView.RepaintAll();
                        }

                        GUI.enabled = !(agent is null);
                        if (GUILayout.Button("Generate Map")) {
                            editMode = EditMode.GenerateMap;
                            generateMapBounds = EstimateMapBounds(generateMapLayerMask);
                            generateMapBounds.Expand(generateMapExpandAmount);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        if (agent is null)
                        {
                            EditorGUILayout.HelpBox("You need to pick default agent to generate map", MessageType.Warning);
                        }
                        GUI.enabled = true;
                        break;
                    }
                case EditMode.Moving:
                    {
                        EditorGUILayout.LabelField("Moving");

                        GUI.enabled = selected != -1;
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = EditorGUILayout.Vector3Field("Selected navpoint", selected != -1 ? navMap.GetNavPoint(selected).position : Vector3.zero);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Move NavPoint");
                            navMap.UpdateNavPoint(selected, newPosition);
                            EditorUtility.SetDirty(target);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        break;
                    }
                case EditMode.Connecting:
                    {
                        EditorGUILayout.LabelField("Connecting");
                        break;
                    }
                case EditMode.Disconnecting:
                    {
                        EditorGUILayout.LabelField("Disconnecting");
                        break;
                    }
                case EditMode.GenerateMap:
                    {
                        EditorGUILayout.LabelField("Generate Map");

                        EditorGUI.BeginChangeCheck();
                        var mask = EditorGUILayout.MaskField(InternalEditorUtility.LayerMaskToConcatenatedLayersMask(generateMapLayerMask), InternalEditorUtility.layers);
                        generateMapLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(mask);
                        generateMapExpandAmount = EditorGUILayout.FloatField("Expand amount", generateMapExpandAmount);

                        if (EditorGUI.EndChangeCheck())
                        {
                            generateMapBounds = EstimateMapBounds(generateMapLayerMask);
                            generateMapBounds.Expand(generateMapExpandAmount);
                            Repaint();
                            SceneView.RepaintAll();
                        }

                        EditorGUI.BeginChangeCheck();
                        generateMapBounds = EditorGUILayout.BoundsField("Bounds", generateMapBounds);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Repaint();
                            SceneView.RepaintAll();
                        }

                        generateMapNumberOfPoints = EditorGUILayout.IntField("Number of map points", generateMapNumberOfPoints);

                        if (GUILayout.Button("Generate"))
                        {
                            GenerateMap(generateMapBounds, generateMapNumberOfPoints, generateMapLayerMask);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        break;
                    }
            }

        }
    }

    int GetNavPointUnder(Vector2 point)
    {
        Camera camera = SceneView.lastActiveSceneView.camera;
        float f = camera.pixelRect.height / 2 * NAVPOINT_MARKER_RADIUS / Mathf.Tan(camera.fieldOfView / 2 * Mathf.Deg2Rad);
        foreach (NavPoint navPoint in navMap.GetNavPoints())
        {
            Vector2 p = camera.WorldToScreenPoint(navPoint.position);
            float screenSpaceRadius = f / Vector3.Distance(camera.transform.position, navPoint.position);

            if (Vector2.Distance(p, point) < screenSpaceRadius)
            {
                return navPoint.id;
            }
        }

        return -1;
    }

    int GetNavPointClosestTo(Vector2 screenPoint, float maxDistance = 50f)
    {
        Camera camera = SceneView.lastActiveSceneView.camera;

        int closestId = -1;
        float closestDistance = float.PositiveInfinity;
        foreach (NavPoint navPoint in navMap.GetNavPoints())
        {
            float distance = Vector2.Distance(camera.WorldToScreenPoint(navPoint.position), screenPoint);
            if (distance < closestDistance)
            {
                closestId = navPoint.id;
                closestDistance = distance;
            }
        }

        if (closestId != -1 && closestDistance > maxDistance)
        {
            closestId = -1;
        }

        return closestId;
    }

    int GetConnectedNavPointClosestTo(NavPoint point, Vector2 screenPoint, float maxDistance = 50f)
    {
        Camera camera = SceneView.lastActiveSceneView.camera;

        int closestId = -1;
        float closestDistance = float.PositiveInfinity;
        foreach (NavPoint navPoint in navMap.GetConnectedNavPoints(point))
        {
            float distance = Vector2.Distance(camera.WorldToScreenPoint(navPoint.position), screenPoint);
            if (distance < closestDistance)
            {
                closestId = navPoint.id;
                closestDistance = distance;
            }
        }

        if (closestId != -1 && closestDistance > maxDistance)
        {
            closestId = -1;
        }

        return closestId;
    }

    void OnSceneGUI()
    {
        Camera camera = SceneView.lastActiveSceneView.camera;

        if (Event.current.type == EventType.Repaint)
        {
            foreach (NavPoint point in navMap.GetNavPoints())
            {
                NavPointDisplay navPointDisplay = NavPointDisplay.Normal;
                ConnectionDisplay connectionDisplay = ConnectionDisplay.Normal;
                if (selected == point.id)
                {
                    navPointDisplay = NavPointDisplay.Selected;
                    connectionDisplay = ConnectionDisplay.Highlighted;
                }

                DrawNavPoint(point, navPointDisplay);

                foreach (NavPoint connectedPoint in navMap.GetConnectedNavPoints(point))
                {
                    if (connectionDisplay != ConnectionDisplay.Highlighted && connectedPoint.id < point.id)
                    {
                        continue;
                    }

                    DrawConnection(point.position, connectedPoint.position, connectionDisplay);
                }
            }
        }

        if (editing)
        {
            switch (editMode)
            {
                case EditMode.Normal:
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            Vector2 mousePosition = new Vector2(Event.current.mousePosition.x, camera.pixelRect.height - Event.current.mousePosition.y);
                            selected = GetNavPointUnder(mousePosition);
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                        {
                            if (GUIUtility.hotControl == GUIUtility.GetControlID(FocusType.Passive))
                            {
                                GUIUtility.hotControl = 0;
                                Event.current.Use();
                            }
                        }
                        else if (Event.current.type == EventType.KeyDown && (Tools.viewTool == ViewTool.None || Tools.viewTool == ViewTool.Pan))
                        {
                            if (Event.current.keyCode == KeyCode.C && selected != -1)
                            {
                                editMode = EditMode.Connecting;
                                Event.current.Use();
                                Repaint();
                                SceneView.RepaintAll();
                            }
                            else if (Event.current.keyCode == KeyCode.D)
                            {
                                editMode = EditMode.Disconnecting;
                                Event.current.Use();
                                Repaint();
                                SceneView.RepaintAll();
                            }
                            else if (Event.current.keyCode == KeyCode.M && selected != -1)
                            {
                                editMode = EditMode.Moving;
                                Event.current.Use();
                                Repaint();
                                SceneView.RepaintAll();
                            }
                            else if (Event.current.keyCode == KeyCode.X && selected != -1)
                            {
                                Undo.RecordObject(target, "Remove NavPoint");
                                navMap.RemoveNavPoint(selected);
                                EditorUtility.SetDirty(target);
                                Event.current.Use();
                                Repaint();
                                SceneView.RepaintAll();
                            }
                        }
                        break;
                    }
                case EditMode.Moving:
                    {
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);

                        if (selected != -1)
                        {
                            NavPoint selectedPoint = navMap.GetNavPoint(selected);
                            foreach (NavPoint connectedPoint in navMap.GetConnectedNavPoints(selectedPoint))
                            {
                                bool collisionFree = Navigation.Utils.IsCollisionFree(world, agent, new Position(selectedPoint.position), new Position(connectedPoint.position));
                                if (collisionFree)
                                {
                                    continue;
                                }

                                DrawConnection(selectedPoint.position, connectedPoint.position, ConnectionDisplay.Blocked);
                            }

                            EditorGUI.BeginChangeCheck();
                            Vector3 newPosition = Handles.PositionHandle(selectedPoint.position, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(target, "Move NavPoint");
                                navMap.UpdateNavPoint(selected, newPosition);

                                List<int> pointIdsToDisconnect = new List<int>();
                                foreach (NavPoint connectedPoint in navMap.GetConnectedNavPoints(selectedPoint))
                                {
                                    bool collisionFree = Navigation.Utils.IsCollisionFree(world, agent, new Position(selectedPoint.position), new Position(connectedPoint.position));
                                    if (collisionFree)
                                    {
                                        continue;
                                    }

                                    pointIdsToDisconnect.Add(connectedPoint.id);
                                }

                                foreach (int connectedId in pointIdsToDisconnect)
                                {
                                    navMap.Disconnect(selected, connectedId);
                                }

                                EditorUtility.SetDirty(target);
                                Repaint();
                                SceneView.RepaintAll();
                            }
                        }

                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            GUIUtility.hotControl = controlId;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && GUIUtility.hotControl == controlId)
                        {
                            Vector2 mousePosition = new Vector2(Event.current.mousePosition.x, camera.pixelRect.height - Event.current.mousePosition.y);
                            int newSelected = GetNavPointUnder(mousePosition);
                            if (newSelected != selected)
                            {
                                selected = newSelected;
                                editMode = EditMode.Normal;
                            }

                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                        {
                            editMode = EditMode.Normal;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        break;
                    }
                case EditMode.Connecting:
                    {
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);

                        NavPoint selectedPoint = navMap.GetNavPoint(selected);

                        Vector2 mousePosition = new Vector2(Event.current.mousePosition.x, camera.pixelRect.height - Event.current.mousePosition.y);
                        int closestId = GetNavPointClosestTo(mousePosition, 25f);
                        bool closestCollisionFree = true;

                        Vector3 p = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));

                        if (closestId != -1)
                        {
                            NavPoint closestPoint = navMap.GetNavPoint(closestId);
                            DrawNavPoint(closestPoint, NavPointDisplay.Highlighted);

                            closestCollisionFree = Navigation.Utils.IsCollisionFree(world, agent, new Position(selectedPoint.position), new Position(closestPoint.position));
                            p = closestPoint.position;
                        }

                        DrawConnection(selectedPoint.position, p, closestCollisionFree ? ConnectionDisplay.Highlighted : ConnectionDisplay.Blocked);

                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                        {
                            editMode = EditMode.Normal;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseDown)
                        {
                            GUIUtility.hotControl = controlId;
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
                        {
                            if (closestId != -1 && closestId != selected && closestCollisionFree)
                            {
                                Undo.RecordObject(target, "Connect NavPoints");
                                navMap.Connect(selected, closestId);
                                EditorUtility.SetDirty(target);
                                if (!Event.current.shift)
                                {
                                    editMode = EditMode.Normal;
                                }
                            }
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseMove)
                        {
                            SceneView.lastActiveSceneView.Repaint();
                        }
                        break;
                    }
                case EditMode.Disconnecting:
                    {
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);

                        NavPoint selectedPoint = navMap.GetNavPoint(selected);

                        Vector2 mousePosition = new Vector2(Event.current.mousePosition.x, camera.pixelRect.height - Event.current.mousePosition.y);
                        NavPoint closestPoint = null;

                        int closestId = GetConnectedNavPointClosestTo(selectedPoint, mousePosition, 25f);
                        if (closestId != -1)
                        {
                            closestPoint = navMap.GetNavPoint(closestId);
                            DrawNavPoint(closestPoint, NavPointDisplay.Highlighted);
                            DrawConnection(selectedPoint.position, closestPoint.position, ConnectionDisplay.Blocked);
                        }

                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                        {
                            editMode = EditMode.Normal;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            GUIUtility.hotControl = controlId;
                            Event.current.Use();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
                        {
                            if (!(closestPoint is null))
                            {
                                Undo.RecordObject(target, "Disconnect NavPoints");
                                navMap.Disconnect(selectedPoint, closestPoint);
                                EditorUtility.SetDirty(target);
                                editMode = EditMode.Normal;
                            }

                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseMove)
                        {
                            SceneView.RepaintAll();
                        }
                        else if (Event.current.type == EventType.MouseDrag)
                        {
                            SceneView.RepaintAll();
                        }
                        break;
                    }
                case EditMode.GenerateMap:
                    {
                        Handles.color = Color.green;
                        var oldZTest = Handles.zTest;
                        Handles.zTest = CompareFunction.Less;
                        Handles.DrawWireCube(generateMapBounds.center, generateMapBounds.size);
                        Handles.zTest = oldZTest;
                        break;
                    }
            }
        }
    }

    void DrawNavPoint(NavPoint point, NavPointDisplay display)
    {
        switch (display)
        {
            case NavPointDisplay.Normal:
                navpointMaterial.SetPass(0);
                break;
            case NavPointDisplay.Highlighted:
                navpointHighlightedMaterial.SetPass(0);
                break;
            case NavPointDisplay.Selected:
                navpointHighlightedMaterial.SetPass(0);
                break;
        }

        Graphics.DrawMeshNow(navpointMesh, Matrix4x4.Translate(point.position) * Matrix4x4.Scale(Vector3.one * NAVPOINT_MARKER_RADIUS));

        if (display == NavPointDisplay.Selected)
        {
            Camera camera = SceneView.lastActiveSceneView.camera;

            Handles.color = Color.yellow;
            Handles.DrawWireDisc(point.position, (camera.transform.position - point.position), NAVPOINT_MARKER_RADIUS + 1f);
        }
    }

    void DrawConnection(Vector3 p1, Vector3 p2, ConnectionDisplay display)
    {
        switch (display)
        {
            case ConnectionDisplay.Normal:
                Handles.color = CONNECTION_NORMAL_COLOR;
                break;
            case ConnectionDisplay.Highlighted:
                Handles.color = CONNECTION_HIGHLIGHT_COLOR;
                break;
            case ConnectionDisplay.Blocked:
                Handles.color = Color.red;
                break;
        }

        var oldZTest = Handles.zTest;
        Handles.zTest = CompareFunction.Less;
        Handles.DrawLine(p1, p2);
        Handles.zTest = oldZTest;
    }

    void DrawScreenLine(Vector2 p1, Vector2 p2)
    {
        Camera camera = SceneView.lastActiveSceneView.camera;
        Handles.DrawLine(
            camera.ScreenToWorldPoint(new Vector3(p1.x, p1.y, 10f)),
            camera.ScreenToWorldPoint(new Vector3(p2.x, p2.y, 10f))
        );
    }

    IEnumerable<Vector3> GetBoundsCorners(Bounds bounds)
    {
        yield return bounds.min + Vector3.zero;
        yield return bounds.min + new Vector3(bounds.size.x, 0, 0);
        yield return bounds.min + new Vector3(0, bounds.size.y, 0);
        yield return bounds.min + new Vector3(0, 0, bounds.size.z);
        yield return bounds.min + new Vector3(bounds.size.x, bounds.size.y, 0);
        yield return bounds.min + new Vector3(bounds.size.x, 0, bounds.size.z);
        yield return bounds.min + new Vector3(0, bounds.size.y, bounds.size.z);
        yield return bounds.min + bounds.size;
    }

    void GenerateMap(Bounds mapBounds, int pointCount, LayerMask layerMask)
    {
        Undo.RecordObject(target, "Generate NavMap");
        navMap.Clear();
        //while (navMap.PointCount < pointCount)
        //{
        //    Vector3 position = MyRandom.Vector3(mapBounds);
        //    int newId = navMap.AddNavPoint(new NavPoint(position));

        //    foreach (NavPoint existingPoint in navMap.GetNavPoints())
        //    {
        //    }
        //}

        for (int sceneIndex=0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Collider collider in root.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled || (((1 << collider.gameObject.layer) & layerMask) == 0))
                    {
                        Debug.Log("Collider did not pass criteria, skipping");
                        continue;
                    }

                    Bounds bounds = collider.bounds;
                    bounds.Expand(generateMapExpandAmount);

                    foreach (Vector3 point in GetBoundsCorners(bounds))
                    {
                        Debug.Log("World = " + world);
                        if (world.BoxCollides(point + agent.offset, agent.size, Quaternion.identity, agent.collisionLayers))
                            continue;

                        navMap.AddNavPoint(new NavPoint(point));
                    }
                }
            }
        }

        foreach (NavPoint p1 in navMap.GetNavPoints())
        {
            ConnectAll(p1.id);
        }

        EditorUtility.SetDirty(target);
    }

    void ConnectAll(int pointId)
    {
        NavPoint p1 = navMap.GetNavPoint(pointId);

        foreach (NavPoint p2 in navMap.GetNavPoints())
        {
            if (p1.id == p2.id)
                continue;

            if (!Navigation.Utils.IsCollisionFree(world, agent, new Position(p1.position), new Position(p2.position)))
                continue;

            navMap.Connect(p1, p2);
        }
    }
}
