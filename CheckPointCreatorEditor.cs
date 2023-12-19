using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR // 에디터 전용 코드 시작
using UnityEditor;

[CustomEditor(typeof(CheckPointCreator))]
public class CheckPointCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CheckPointCreator myScript = (CheckPointCreator)target;
        if (GUILayout.Button("Create Checkpoints"))
        {
            myScript.CreateCheckpoints();
        }
    }
}
#endif // 에디터 전용 코드 끝
