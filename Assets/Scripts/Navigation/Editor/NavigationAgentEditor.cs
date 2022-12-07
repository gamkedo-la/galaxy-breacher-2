using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavigationAgent))]
public class NavigationAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavigationAgent agentComponent = (NavigationAgent)target;

        if (GUILayout.Button("Calculate Size"))
        {
            Collider[] colliders = agentComponent.gameObject.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0) {
                Debug.LogError("No colliders for ship " + name);
                return;
            }

            Bounds bounds = colliders[0].bounds;
            for (int i=1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            Undo.RecordObject(agentComponent, "Calculate navigation agent size");
            agentComponent.agent.offset = bounds.center - agentComponent.gameObject.transform.position;
            agentComponent.agent.size = bounds.size;
        }
    }

    public void OnSceneGUI()
    {
        Navigation.Agent agent = ((NavigationAgent)target)?.agent;

        Handles.color = Color.green;
        Handles.DrawWireCube(agent.offset, agent.size);
    }
}
