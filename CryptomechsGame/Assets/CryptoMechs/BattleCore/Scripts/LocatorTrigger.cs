using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocatorTrigger : MonoBehaviour
{
    public string id;
    public float radius;
    public StoryNarrative storyNarrative;

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Vector3 pos = this.transform.position;
        //pos.y = 0;
        Gizmos.DrawSphere(pos, 0.3f);

        UnityEditor.Handles.BeginGUI();
        GUI.color = Color.white;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(pos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent("Trigger: " + id));
        GUI.Label(new Rect(screenPos.x - (size.x), -screenPos.y + view.position.height + 4, size.x, size.y), "Trigger: " + id);
        UnityEditor.Handles.EndGUI();
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var player = CharacterAgentsManager.Instance?.player;
        if (player == null) return;
        
        Vector3 pos = transform.position;
        pos.y = 0;
        Vector3 playerPos = CharacterAgentsManager.Instance.player.transform.position;
        playerPos.y = 0;
        if (Vector3.Distance(pos, playerPos) < radius)
        {
            player.TriggerLocation(this);
        }
    }
}
