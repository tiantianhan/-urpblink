using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Blinker))]
public class BlinkerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Blinker blinker = (Blinker)target;
        if(GUILayout.Button("Blink now"))
        {
            blinker.Blink();
        }
    }
}