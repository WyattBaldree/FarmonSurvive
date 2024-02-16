using UnityEngine;
using System.Linq;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;


public class AddInvertedMeshCollider : MonoBehaviour
{
    public bool removeExistingColliders = true;
    public bool generateMeshAtProjectRoot = false;

    public void CreateInvertedMeshCollider()
    {
        if (removeExistingColliders)
            RemoveExistingColliders();

        InvertMesh();

        gameObject.AddComponent<MeshCollider>();
    }

    private void RemoveExistingColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            DestroyImmediate(colliders[i]);
    }

    private void InvertMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();

        if (generateMeshAtProjectRoot)
        {
            AssetDatabase.CreateAsset(mesh, "Assets/invertedMesh.asset");
            AssetDatabase.SaveAssets();
        }
    }
}

[CustomEditor(typeof(AddInvertedMeshCollider))]
public class AddInvertedMeshColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AddInvertedMeshCollider script = (AddInvertedMeshCollider)target;
        if (GUILayout.Button("Create Inverted Mesh Collider"))
            script.CreateInvertedMeshCollider();
    }
}
#endif