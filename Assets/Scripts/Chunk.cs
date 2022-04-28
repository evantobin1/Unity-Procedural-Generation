using UnityEngine;

public class Chunk : MonoBehaviour
{

    Mesh mesh; //Local Mesh
    Vector3[] vertices; //Vertex coordinates
    int[] triangles; //Order of vertices
    Color[] colors; //The color at each vertex

    public int tileNumber;
    public float planeHeight, minTerrainHeight, maxTerrainHeight = 0;
    public Gradient gradient;
    public GameObject player;
    public float[,] y;



    //Instantiates the chunk's mesh
    public void create()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
    }

    //Creates a collision model
    public void createCollision()
    {
        MeshCollider previous = gameObject.GetComponent(typeof(MeshCollider)) as MeshCollider;
        if (previous != null)
        {
            Destroy(previous);
        }
        MeshCollider sc = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
    }

    //Destroys the collision model
    public void destroyCollision()
    {
        MeshCollider previous = gameObject.GetComponent(typeof(MeshCollider)) as MeshCollider;
        if (previous != null)
        {
            DestroyImmediate(previous);
        }
    }

    void CreateShape()
    {
        vertices = new Vector3[(player.GetComponent<Player>().chunkSize + 1) * (player.GetComponent<Player>().chunkSize + 1)];
        triangles = new int[6 * player.GetComponent<Player>().chunkSize * player.GetComponent<Player>().chunkSize];

        for (int z = 0, i = 0; z < player.GetComponent<Player>().chunkSize + 1; z++)
        {
            for (int x = 0; x < player.GetComponent<Player>().chunkSize + 1; x++)
            {
                float meshHeightCurveLocation = player.GetComponent<Player>().meshHeightCurve.Evaluate(Mathf.InverseLerp(player.GetComponent<Player>().minVal, player.GetComponent<Player>().maxVal, y[x, z]));

                vertices[i] = new Vector3(x, player.GetComponent<Player>().amplitudeScale * meshHeightCurveLocation, z);
                i++;
            }
        }


        int tris = 0;
        int vert = 0;
        for (int z = 0; z < player.GetComponent<Player>().chunkSize; z++)
        {
            for (int x = 0; x < player.GetComponent<Player>().chunkSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + player.GetComponent<Player>().chunkSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + player.GetComponent<Player>().chunkSize + 1;
                triangles[tris + 5] = vert + player.GetComponent<Player>().chunkSize + 2;

                tris += 6;
                vert++;
            }
            vert++;
        }

    }


    public void ColorShape()
    {
        colors = new Color[vertices.Length];
        gradient = player.GetComponent<Player>().gradient;
        for (int z = 0, i = 0; z < player.GetComponent<Player>().chunkSize + 1; z++)
        {
            for (int x = 0; x < player.GetComponent<Player>().chunkSize + 1; x++)
            {
                float height = Mathf.InverseLerp(player.GetComponent<Player>().minVal, player.GetComponent<Player>().maxVal, y[x, z]);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }


    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = CalculateNormals();
        mesh.colors = colors;
    }

    public float[,] calculateNoise()
    {

        float[,] perlin = new float[player.GetComponent<Player>().chunkSize + 1, player.GetComponent<Player>().chunkSize + 1];

        for (int oct = 0; oct < player.GetComponent<Player>().octaves; oct++)
        {
            for (int z = 0; z < player.GetComponent<Player>().chunkSize + 1; z++)
            {
                for (int x = 0; x < player.GetComponent<Player>().chunkSize + 1; x++)
                {
                    float noise = (-1 + 2 * Mathf.PerlinNoise((x + transform.position.x + player.GetComponent<Player>().xOffset * (oct + 1)) * Mathf.Pow(player.GetComponent<Player>().frequency, oct) / player.GetComponent<Player>().frequencyScale, (z + transform.position.z + player.GetComponent<Player>().zOffset * (oct + 1)) * Mathf.Pow((player.GetComponent<Player>().frequency), oct) / player.GetComponent<Player>().frequencyScale)) * Mathf.Pow(player.GetComponent<Player>().amplitude, oct + 1);


                    //Sets all vertices to an initial height of 0
                    if(oct == 0)
                    {
                        perlin[x, z] = 0;
                    }
                    perlin[x, z] += noise;

                    //Sets the max and min terrain height values for the chunk
                    if (perlin[x, z] > maxTerrainHeight)
                    {
                        maxTerrainHeight = perlin[x, z];
                    }
                    if (perlin[x, z] < minTerrainHeight)
                    {
                        minTerrainHeight = perlin[x, z];
                    }
                }
            }
        }
        return perlin;
    }


    public float[] getRange()
    {
        float[] range = { minTerrainHeight, maxTerrainHeight };
        return range;
    }

    public void setTileNumber(int tileNumber)
    {
        this.tileNumber = tileNumber;
    }

    public Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;

        }
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}
