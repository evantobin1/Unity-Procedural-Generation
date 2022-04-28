using UnityEngine;

public class Player : MonoBehaviour
{
    public float playerSpeed = 15f;
    public Transform transform_;
    public Rigidbody rb;
    Vector2 rotation = new Vector2(0, 0);
    public bool Generate;
    GameObject[] allTerrains;
    public int xOffset, zOffset, chunkSize, octaves, biome, chunkAmount;
    private int xTileMoved, zTileMoved = 0;
    public float maxVal = -100f;
    public float minVal = 100f;
    public float amplitude, frequency, amplitudeScale, frequencyScale;
    public Gradient gradient;
    public AnimationCurve meshHeightCurve;
    public GameObject[] generatedChunks;


    void Start()
    {
        startChunks();
        allTerrains = GameObject.FindGameObjectsWithTag("Terrain");
        randomizePerlin();
    }

    void FixedUpdate()
    {
        checkGenerate();
        checkChunkSpawn();

    }



    public void checkGenerate()
    {
        if (Generate)
        {
            foreach (GameObject terrain in allTerrains)
            {
                terrain.GetComponent<Chunk>().y = terrain.GetComponent<Chunk>().calculateNoise();
            }
            setMinMaxVals();
            foreach (GameObject terrain in allTerrains)
            {
                terrain.GetComponent<Chunk>().create();
            }
            foreach (GameObject terrain in allTerrains)
            {
                terrain.GetComponent<Chunk>().ColorShape();
            }
            foreach (GameObject terrain in allTerrains)
            {
                terrain.GetComponent<Chunk>().UpdateMesh();
            }
            foreach (GameObject terrain in allTerrains)
            {
                terrain.GetComponent<Chunk>().createCollision();
            }
            Generate = false;
        }
    }

    public void generateChunk(GameObject chunk)
    {
        chunk.GetComponent<Chunk>().y = chunk.GetComponent<Chunk>().calculateNoise();
        chunk.GetComponent<Chunk>().create();
        chunk.GetComponent<Chunk>().ColorShape();
        chunk.GetComponent<Chunk>().UpdateMesh();
        chunk.GetComponent<Chunk>().createCollision();
    }

    public void checkChunkSpawn()
    {
        if(getPlayerTileNumber() == 4)
        {
            return;
        }
        if (getPlayerTileNumber() == 3)
        {
            xTileMoved--;
            GameObject[] temp = new GameObject[chunkAmount * chunkAmount];
            for (int i = 0; i < generatedChunks.Length; i++)
            {
                if ((i + chunkAmount) % chunkAmount == 0)
                {
                    generatedChunks[i].GetComponent<Chunk>().transform.position -= new Vector3(chunkAmount * chunkSize, 0, 0);
                    temp[i + chunkAmount - 1] = generatedChunks[i];
                    generateChunk(temp[i + chunkAmount - 1]);
                }
                else
                {
                    temp[i - 1] = generatedChunks[i];
                }
            }
            generatedChunks = temp;
        }
        if (getPlayerTileNumber() == 5)
        {
            xTileMoved++;
            GameObject[] temp = new GameObject[chunkAmount * chunkAmount];
            for (int i = 0; i < generatedChunks.Length; i++)
            {
                if ((i + chunkAmount) % chunkAmount == chunkAmount - 1)
                {
                    generatedChunks[i].GetComponent<Chunk>().transform.position += new Vector3(chunkAmount * chunkSize, 0, 0);
                    temp[i - (chunkAmount - 1)] = generatedChunks[i];
                    generateChunk(temp[i - (chunkAmount - 1)]);
                }
                else
                {
                    temp[i + 1] = generatedChunks[i];
                }
            }
            generatedChunks = temp;
        }
        if (getPlayerTileNumber() == 7)
        {
            zTileMoved--;
            GameObject[] temp = new GameObject[chunkAmount * chunkAmount];
            for (int i = 0; i < generatedChunks.Length; i++)
            {
                if (i < chunkAmount)
                {
                    generatedChunks[i].GetComponent<Chunk>().transform.position -= new Vector3(0, 0, chunkAmount * chunkSize);
                    temp[i + (chunkAmount * (chunkAmount - 1))] = generatedChunks[i];
                    generateChunk(temp[i + (chunkAmount * (chunkAmount - 1))]);
                }
                else
                {
                    temp[i - chunkAmount] = generatedChunks[i];
                }
            }
            generatedChunks = temp;
        }
        if (getPlayerTileNumber() == 1)
        {
            zTileMoved++;
            GameObject[] temp = new GameObject[chunkAmount * chunkAmount];
            for (int i = 0; i < generatedChunks.Length; i++)
            {
                if (i > chunkAmount * (chunkAmount - 1) - 1)
                {
                    generatedChunks[i].GetComponent<Chunk>().transform.position += new Vector3(0, 0, chunkAmount * chunkSize);
                    temp[i - (chunkAmount * (chunkAmount - 1))] = generatedChunks[i];
                    generateChunk(temp[i - (chunkAmount * (chunkAmount - 1))]);
                }
                else
                {
                    temp[i + chunkAmount] = generatedChunks[i];
                }
            }
            generatedChunks = temp;
        }
    }

    public void setMinMaxVals()
    {
        foreach (GameObject terrain in allTerrains)
        {
            float[] range = terrain.GetComponent<Chunk>().getRange();

            if (range[0] < minVal)
            {
                minVal = range[0];
            }
            if (range[1] > maxVal)
            {
                maxVal = range[1];
            }
        }
    }

    private void randomizePerlin()
    {
        xOffset = Random.Range(-100000, 100000);
        zOffset = Random.Range(-100000, 100000);
    }

    public void startChunks()
    {
        generatedChunks = new GameObject[chunkAmount * chunkAmount];
        for (int z = 0; z < chunkAmount; z++)
        {
            for (int x = 0; x < chunkAmount; x++)
            {
                GameObject mesh = new GameObject("Tile: " + ((z * chunkAmount) + x).ToString(), typeof(Chunk), typeof(MeshFilter), typeof(MeshRenderer));
                mesh.GetComponent<Chunk>().setTileNumber((z * chunkAmount) + x);
                mesh.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Terrain/Mesh", typeof(Material)) as Material; ;
                mesh.GetComponent<Chunk>().player = GameObject.FindGameObjectWithTag("Player");
                mesh.GetComponent<Chunk>().transform.position += new Vector3((chunkAmount / 2 - x) * chunkSize, 0, (chunkAmount / 2 - z) * chunkSize);
                mesh.tag = "Terrain";
                mesh.GetComponent<Chunk>().createCollision();
                generatedChunks[(z * chunkAmount) + x] = mesh;

            }
        }
    }

    public int getPlayerTileNumber()
    {
        if(transform_.position.x < xTileMoved * chunkSize)
        {
            return 3;
        }
        if (transform_.position.x > (xTileMoved + 1) * chunkSize)
        {
            return 5;
        }
        if (transform_.position.z < zTileMoved * chunkSize)
        {
            return 7;
        }
        if (transform_.position.z > (zTileMoved + 1) * chunkSize)
        {
            return 1;
        }
        return 4;
    }


}
