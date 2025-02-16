using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataBinding), true)]
public class DataBindingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        var dataBinding = (DataBinding)target;

        // AutoRefreshDataID가 활성화되어 있고 DataID가 오브젝트 이름과 다르면 갱신
        if (dataBinding.AutoRefreshDataID && !dataBinding.DataID.Equals(dataBinding.gameObject.name))
        {
            dataBinding.DataID = dataBinding.gameObject.name;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
