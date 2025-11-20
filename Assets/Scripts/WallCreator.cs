using UnityEngine;

[ExecuteAlways] 
public class WallCreator : MonoBehaviour
{
    Mesh mesh;
    MeshFilter mf;
    MeshCollider mc;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();  

        mesh = MeshCreator.CreateCubeMesh();
        mf.mesh = mesh;
        mc.sharedMesh = mesh;
    }
}
