using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ShowObjectMesh : MonoBehaviour
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;

    [ContextMenu("get data")]
    public void GetMesh()
    {
        Mesh m = GetComponent<MeshFilter>().sharedMesh;

        vertices = m.vertices;
        uvs = m.uv;
        triangles = m.triangles;
    }

    [ContextMenu("set data")]
    public void SetMesh()
    {
        Mesh m = new Mesh();

        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = m;
    }
}

