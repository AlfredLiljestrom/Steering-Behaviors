using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public static class MeshCreator
{
    public static Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> indices = new();

        vertices.Add(Vector3.zero); // 0
        vertices.Add(Vector3.forward); // 1
        vertices.Add(Vector3.right); // 2
        vertices.Add(Vector3.forward + Vector3.right); // 3
        vertices.Add(Vector3.up); // 4
        vertices.Add(Vector3.forward + Vector3.up); // 5
        vertices.Add(Vector3.right + Vector3.up); // 6
        vertices.Add(Vector3.forward + Vector3.right + Vector3.up); // 7

        // Bottom 
        indices.Add(0);
        indices.Add(2);
        indices.Add(3);

        indices.Add(0);
        indices.Add(3);
        indices.Add(1);

        // Front 
        indices.Add(0);
        indices.Add(4);
        indices.Add(2);

        indices.Add(2);
        indices.Add(4);
        indices.Add(6);

        // Left Side 
        indices.Add(0);
        indices.Add(1);
        indices.Add(5);

        indices.Add(0);
        indices.Add(5);
        indices.Add(4);

        // Right Side 
        indices.Add(2);
        indices.Add(7);
        indices.Add(3);

        indices.Add(2);
        indices.Add(6);
        indices.Add(7);

        // Back
        indices.Add(3);
        indices.Add(5);
        indices.Add(1);

        indices.Add(3);
        indices.Add(7);
        indices.Add(5);

        // Top 
        indices.Add(4);
        indices.Add(5);
        indices.Add(7);

        indices.Add(4);
        indices.Add(7);
        indices.Add(6);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }



    public static Mesh CreateCylinderMesh(float radius, int detail, Vector3 from, Vector3 to)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> indices = new();

        Vector3 dir = (to - from) / (float) (detail - 1); 
        float dist = dir.magnitude;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, dir);

        List<int> edgeIndices = new();

        for (int y = 0; y < detail; y++)
        {
            for (int x = 0; x < detail; x++)
            {
                float angle = (x / (float)detail) * 2 * Mathf.PI;
                Vector3 pos = from + rotation * new Vector3(Mathf.Cos(angle) * radius, dir.magnitude * y, Mathf.Sin(angle) * radius);
                vertices.Add(pos);

                if (y == 0 || y == detail - 1)
                {
                    edgeIndices.Add(vertices.Count - 1);
                }

                if (y == detail - 1)
                    continue;

                if (x == detail - 1)
                {
                    indices.Add(x + y * detail);
                    indices.Add(detail + y * detail);
                    indices.Add(y * detail);

                    indices.Add(x + y * detail);
                    indices.Add(x + detail + y * detail);
                    indices.Add(detail + y * detail);
                    continue;
                }

                // Index
                indices.Add(x + y * detail);
                indices.Add(x + 1 + detail + y * detail);
                indices.Add(x + 1 + y * detail);


                indices.Add(x + y * detail);
                indices.Add(x + detail + y * detail);
                indices.Add(x + 1 + detail + y * detail);
            }
        }

        int bottomCenterIndex = vertices.Count;
        vertices.Add(from);
        int topcenterindex = vertices.Count;
        vertices.Add(to);

        for (int i = 0; i < detail; i++)
        {
            indices.Add(bottomCenterIndex);
            indices.Add(edgeIndices[i]);
            indices.Add(edgeIndices[(i + 1) % detail]);
            indices.Add(topcenterindex);
            indices.Add(edgeIndices[(i + 1) % detail + detail]);
            indices.Add(edgeIndices[i + detail]);

        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreateCylinderMesh(float radius, float heigth, int detail)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> indices = new();

        List<int> edgeIndices = new();

        for (int y = 0; y < detail; y++)
        {
            for (int x = 0; x < detail; x++)
            {
                float angle = (x / (float)detail) * 2 * Mathf.PI;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, (y - detail / 2) * (heigth / detail), Mathf.Sin(angle) * radius);
                vertices.Add(pos);

                if (y == 0 || y == detail - 1)
                {
                    edgeIndices.Add(vertices.Count - 1);
                }

                if (y == detail - 1)
                    continue;

                if (x == detail - 1)
                {
                    indices.Add(x + y * detail);
                    indices.Add(detail + y * detail);
                    indices.Add(y * detail);

                    indices.Add(x + y * detail);
                    indices.Add(x + detail + y * detail);
                    indices.Add(detail + y * detail);
                    continue;
                }

                // Index
                indices.Add(x + y * detail);
                indices.Add(x + 1 + detail + y * detail);
                indices.Add(x + 1 + y * detail);


                indices.Add(x + y * detail);
                indices.Add(x + detail + y * detail);
                indices.Add(x + 1 + detail + y * detail);
            }
        }

        int bottomCenterIndex = vertices.Count; 
        vertices.Add(new Vector3(0, (-detail / 2) * (heigth / detail), 0));
        int TopCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0, (+detail / 2) * (heigth / detail), 0));

        for (int i = 0; i < detail; i++)
        {
            indices.Add(bottomCenterIndex);
            indices.Add(edgeIndices[i]);
            indices.Add(edgeIndices[(i + 1) % detail]);
            indices.Add(TopCenterIndex);
            indices.Add(edgeIndices[(i + 1) % detail + detail]);
            indices.Add(edgeIndices[i + detail]);
 
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreateCylinderMesh(int detail)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> indices = new();


        List<int> edgeIndices = new();

        for (int y = 0; y < detail; y++)
        {
            for (int x = 0; x < detail; x++)
            {
                float angle = (x / (float)detail) * 2 * Mathf.PI;
                Vector3 pos = new Vector3(Mathf.Cos(angle), y / (float)(detail - 1) - 0.5f, Mathf.Sin(angle));
                vertices.Add(pos);

                if (y == 0 || y == detail - 1)
                {
                    edgeIndices.Add(vertices.Count - 1);
                }

                if (y == detail - 1)
                    continue;

                if (x == detail - 1)
                {
                    indices.Add(x + y * detail);
                    indices.Add(detail + y * detail);
                    indices.Add(y * detail);

                    indices.Add(x + y * detail);
                    indices.Add(x + detail + y * detail);
                    indices.Add(detail + y * detail);
                    continue;
                }

                // Index
                indices.Add(x + y * detail);
                indices.Add(x + 1 + detail + y * detail);
                indices.Add(x + 1 + y * detail);


                indices.Add(x + y * detail);
                indices.Add(x + detail + y * detail);
                indices.Add(x + 1 + detail + y * detail);
            }
        }

        int bottomCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0, -0.5f, 0));
        int topcenterindex = vertices.Count;
        vertices.Add(new Vector3(0, 0.5f, 0));

        for (int i = 0; i < detail; i++)
        {
            indices.Add(bottomCenterIndex);
            indices.Add(edgeIndices[i]);
            indices.Add(edgeIndices[(i + 1) % detail]);
            indices.Add(topcenterindex);
            indices.Add(edgeIndices[(i + 1) % detail + detail]);
            indices.Add(edgeIndices[i + detail]);

        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreatePlaneMesh(float size, int detail, Vector3 position, float perlinMultiplier = 0, float perlinHeight = 0)
    {
        Mesh mesh = new Mesh(); 

        // Create vertices and indices
        List<Vector3> vertices = new();
        List<int> indices = new();

        List<Vector2> uvs = new();
        uvs.Clear();
        for (int z = 0; z < detail; z++)
        {
            for (int x = 0; x < detail; x++)
            {
                var px = (float)x / (detail - 1);
                var pz = (float)z / (detail - 1); 

                // Vertex
                var point = new Vector3(px * size, 0f, pz * size);
                vertices.Add(position + point);
                uvs.Add(new Vector2(px, pz));

                if (x == detail - 1 || z == detail - 1)
                    continue;
                // Index
                indices.Add(x + z * detail);
                indices.Add(x + 1 + detail + z * detail);
                indices.Add(x + 1 + z * detail);


                indices.Add(x + z * detail);
                indices.Add(x + detail + z * detail);
                indices.Add(x + 1 + detail + z * detail);

            }
        }


        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh; 
    }

    public static Mesh CreateSphereMeshIco(float size, int detail, Vector3 position, float perlinMultiplier = 0, float perlinHeight = 0)
    {
        Mesh mesh = new Mesh();
        List<int> indices = new();
        List<Vector3> vertices = new();
        indices.Clear();
        vertices.Clear();

        setupVertices(ref vertices, size, position);
        float radius = Vector3.Distance(vertices[0], Vector3.zero);

        setupIndices(ref indices);
        subDivide(ref vertices, ref indices, detail);
        removeDuplicates(ref vertices, ref indices);
        expand(ref vertices, radius, Vector3.zero);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh; 
    }


    static void removeDuplicates(ref List<Vector3> vertices, ref List<int> indices)
    {
        List<Vector3> uniqueVertices = new List<Vector3>();
        Dictionary<int, int> newIndices = new();

        for (int i = 0; i < vertices.Count; i++)
        {
            int index = 0;
            if (!contains(uniqueVertices, vertices[i], ref index))
            {
                uniqueVertices.Add(vertices[i]);
                newIndices.Add(i, uniqueVertices.Count - 1);
            }
            else
            {
                newIndices.Add(i, index);
            }


        }

        for (int i = 0; i < indices.Count; i++)
        {
            indices[i] = newIndices[indices[i]];
        }
        vertices = uniqueVertices;
    }

    static bool contains(List<Vector3> list, Vector3 vec, ref int index)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (Vector3.Distance(list[i], vec) < 0.1f)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }


    static void expand(ref List<Vector3> vertices, float radius, Vector3 middle)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 dir = vertices[i] - middle;
            float length = Vector3.Distance(dir, middle);
            vertices[i] *= radius / length;
        }
    }

    static void subDivide(ref List<Vector3> vertices, ref List<int> indices, int detail)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newIndices = new List<int>();

        if (detail == 0)
            return;

        var divisions = detail + 1;



        int numberOfAddedPoint = (detail * (detail + 5)) / 2 + 3;

        int count = indices.Count;
        for (int s = 0; s < count; s += 3)
        {
            Vector3 p1 = vertices[indices[s]];
            Vector3 p2 = vertices[indices[s + 1]];
            Vector3 p3 = vertices[indices[s + 2]];

            Vector3 p1ToP2 = p2 - p1;
            Vector3 p3ToP2 = p2 - p3;
            Vector3 across = p3 - p1;


            Vector3 left = p1;
            Vector3 right = p3;
            int firstAvailableIndex = newVertices.Count;

            for (int j = 0; j <= divisions; j++)
            {
                for (int i = 0; i <= divisions - j; i++)
                {
                    float scaler = 0;
                    if (divisions - j != 0)
                        scaler = (float)(i) / (divisions - j);
                    Vector3 p12 = left + across * scaler;

                    if (i == 0 || i == divisions - j)
                    {
                        //problematicVertices.Add(newVertices.Count);
                    }
                    newVertices.Add(p12);
                }

                float scaler2 = (float)(j + 1) / (divisions);
                left = p1 + p1ToP2 * scaler2;
                right = p3 + p3ToP2 * scaler2;
                across = right - left;
            }


            subDivisionIndices(ref newIndices, firstAvailableIndex, divisions);
        }

        vertices = newVertices;
        indices = newIndices;
    }

    static void subDivisionIndices(ref List<int> indices, int firstAvailableIndex, int divisions)
    {
        int ind = firstAvailableIndex;
        int up = 0;

        for (int j = 0; j < divisions; j++)
        {
            for (int i = 0; i < divisions - j; i++)
            {

                indices.Add(ind + i + up);
                indices.Add(ind + i + (divisions - j + 1) + up);
                indices.Add(ind + i + 1 + up);

                if (i == divisions - j - 1)
                    continue;
                indices.Add(ind + i + 1 + up);
                indices.Add(ind + i + (divisions - j + 1) + up);
                indices.Add(ind + i + (divisions - j + 2) + up);
            }
            up += divisions + 1 - j;
        }
    }

    static void setupIndices(ref List<int> indices)
    {
        indices.Clear();

        indices.Add(0);
        indices.Add(10);
        indices.Add(1);

        indices.Add(0);
        indices.Add(1);
        indices.Add(8);

        indices.Add(8);
        indices.Add(1);
        indices.Add(4);

        indices.Add(8);
        indices.Add(6);
        indices.Add(0);

        indices.Add(0);
        indices.Add(7);
        indices.Add(10);

        indices.Add(1);
        indices.Add(10);
        indices.Add(5);

        indices.Add(1);
        indices.Add(5);
        indices.Add(4);

        indices.Add(0);
        indices.Add(6);
        indices.Add(7);

        indices.Add(10);
        indices.Add(7);
        indices.Add(11);

        indices.Add(10);
        indices.Add(11);
        indices.Add(5);

        indices.Add(5);
        indices.Add(11);
        indices.Add(3);

        indices.Add(4);
        indices.Add(5);
        indices.Add(3);

        indices.Add(7);
        indices.Add(6);
        indices.Add(2);

        indices.Add(3);
        indices.Add(11);
        indices.Add(2);

        indices.Add(2);
        indices.Add(11);
        indices.Add(7);

        indices.Add(6);
        indices.Add(8);
        indices.Add(9);

        indices.Add(8);
        indices.Add(4);
        indices.Add(9);

        indices.Add(9);
        indices.Add(4);
        indices.Add(3);

        indices.Add(9);
        indices.Add(3);
        indices.Add(2);

        indices.Add(9);
        indices.Add(2);
        indices.Add(6);
    }

    static void setupVertices(ref List<Vector3> vertices, float size, Vector3 position)
    {
        vertices ??= new();
        vertices.Clear();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;

        float rectangleLength = size * goldenRatio;
        float rectangleWidth = size;

        float halfLength = rectangleLength / 2.0f;
        float halfWidth = rectangleWidth / 2.0f;

        // First rectangle. 
        Vector3 bottomLeft = position + new Vector3(-halfLength, 0, halfWidth);
        Vector3 bottomRight = position + new Vector3(-halfLength, 0, -halfWidth);
        Vector3 TopLeft = position + new Vector3(halfLength, 0, halfWidth);
        Vector3 TopRight = position + new Vector3(halfLength, 0, -halfWidth);

        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);
        vertices.Add(TopLeft);
        vertices.Add(TopRight);

        // Second rectangle. 
        bottomLeft = position + new Vector3(0, -halfWidth, -halfLength);
        bottomRight = position + new Vector3(0, halfWidth, -halfLength);
        TopLeft = position + new Vector3(0, -halfWidth, halfLength);
        TopRight = position + new Vector3(0, halfWidth, halfLength);

        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);
        vertices.Add(TopLeft);
        vertices.Add(TopRight);

        // Third rectangle. 
        bottomLeft = position + new Vector3(-halfWidth, -halfLength, 0);
        bottomRight = position + new Vector3(halfWidth, -halfLength, 0);
        TopLeft = position + new Vector3(-halfWidth, halfLength, 0);
        TopRight = position + new Vector3(halfWidth, halfLength, 0);

        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);
        vertices.Add(TopLeft);
        vertices.Add(TopRight);
    }
}
