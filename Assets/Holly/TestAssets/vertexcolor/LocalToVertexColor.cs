using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalToVertexColor : MonoBehaviour {

    private void Start()
    {
        MeshFilter meshf = GetComponent<MeshFilter>();
        meshf.mesh = BakeColors(meshf.mesh);
        meshf.mesh = Deform(meshf.mesh);
    }

    Mesh BakeColors(Mesh mesh)
    {
        Mesh m = mesh;
        Color[] colors = new Color[m.vertices.Length];

        for (int i = 0; i < m.vertices.Length; i++)
        {
           Vector3 localPos = m.vertices[i];
           Color localColor = new Color(localPos.x, localPos.y, localPos.z);
           colors[i] = localColor;
           //print(localColor.r);
        }

        m.name = "Colored" + m.name;
        mesh.colors = colors;

        return m;
    }
    Mesh Deform(Mesh mesh)
    {
        Mesh m = mesh;
        Vector3[] vertices = m.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x += vertices[i].y;
        }

        m.vertices = vertices;

        return m;
    }
}
