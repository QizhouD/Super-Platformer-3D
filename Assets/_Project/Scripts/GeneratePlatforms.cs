using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class GeneratePlatforms : MonoBehaviour
{
    /* It only works if the x position of the endObject is higher than the start object
     * Same if its direction is on the z 
     * */

    public GameObject player;

    //Fitting between two checkpoints
    public GameObject startObject;
    public GameObject endObject;

    private Vector3 startObjPos;
    private Vector3 endObjPos;

    private Vector3 startPos;
    private Vector3 endPos;

    private Vector3 startObjSize;
    private Vector3 endObjSize;

    //Platforms to spawn
    public GameObject groundPlat; //Number 1
    private Vector3 groundPlatSize;  //They all have the same size
    public GameObject timedPlat; //Number 2
    public GameObject horizontalPlat; // Number 3
    public GameObject empty; //Nullholder

    //Traps and enemies
    public GameObject jellyFish;
    public GameObject enemy;
    //public GameObject waste;

    //Scripts
    private HorizontalPlatform horizontalScript;

    //grid
    private int mapLength;
    private int mapWidth;
    int[,] map;
    Vector3[,] pos;
    GameObject[,] objMap;
    GameObject[] enemies;

    private bool instantiated = false;
    private bool finished = false;

    //bake nav mesh
    public NavMeshSurface[] platforms;
    public NavMeshSurface mesh;

    private void Update()
    {
        bakeNavMesh();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        { 
            if (!instantiated)
            {
                Debug.Log("initialise");
                InitialiseGrid();
                //bakeNavMesh();
                player.GetComponent<PlayerController>().originalPos = player.GetComponent<CharacterController>().transform.position;
            }
        }
    }

    //bake nav mesh when new platform is created
    void bakeNavMesh() {
        //Debug.Log("run into bakeNavMesh");
        //Debug.Log("length is " + platforms.Length);
        //mesh.BuildNavMesh();
        //Debug.Log("finished");
        //platforms[0] = mesh;
        for (int i = 0; i < platforms.Length; i++)
        {
            platforms[i].BuildNavMesh();
            //Debug.Log("item in platform"+ platforms[i]);
        }
    }


    //Checks which direction the next checkpoint is, x or z
    void InitialiseGrid()
    {


        //Positions of the two boundary checkpoints
        startObjPos = startObject.transform.position;
        endObjPos = endObject.transform.position;

        //Size of the checkpoints
        startObjSize = startObject.GetComponent<Collider>().bounds.size;
        endObjSize = endObject.GetComponent<Collider>().bounds.size;



        //Size of the PCG platforms
        groundPlatSize = new Vector3(5, 1, 5);

        //Check if area length is on x or z
        float diffx = Mathf.Abs(startObjPos.x - endObjPos.x);
        float diffz = Mathf.Abs(startObjPos.z - endObjPos.z);


        if (diffx > diffz) //X
        {
            //Debug.Log("x");
            //Distance between boundary checkpoints and PCG egde platforms
            startPos = new Vector3(startObjPos.x + startObjSize.x / 2 + 7, startObjPos.y, startObjPos.z);
            endPos = new Vector3(endObjPos.x - endObjSize.x / 2, endObjPos.y, endObjPos.z);

            //pos[x, z] = new Vector3(startPos.x + (groundPlatSize.x + 5) * x, startPos.y - (startObjSize.y - startObjSize.y / 2), startPos.z + (groundPlatSize.z + 5) * z - groundPlatSize.z / 2);

            //Set parameters for the grid
            mapLength = Mathf.FloorToInt(Mathf.Abs(startPos.x - endPos.x) / (groundPlatSize.x + 4.9f));
            mapWidth = 2;

            //Create grid
            map = new int[mapLength, mapWidth];
            objMap = new GameObject[mapLength, mapWidth];
            pos = new Vector3[mapLength, mapWidth];

            //Randomly fill 2D array map[x,z] with 0 and 1,
            if (map != null)
            { 
                for (int x = 0; x < mapLength; x++)
                {
                    for (int z = 0; z < mapWidth; z++)
                    {
                        map[x, z] = Random.Range(0, 2);
                    }
                }
            }

            bool[] possible = new bool[mapLength]; //To check if the next row can have a 3
            for (int i = 0; i < possible.Length; i++)
            {
                possible[i] = true;
            }
            //Update map[x,z] with 2 and 3 based on certain conditions starting from the end
            for (int x = 0; x < mapLength; x++)
            {
                //Debug.Log(x + ": " + map[x, 0] + "," + map[x, 1]);
                if (map[x, 0] == 0 & map[x, 1] == 0) //Check if row is both 0
                {
                    if (x > 0) //If it's not the first row
                    {
                        if (map[x - 1, 0] != 3 & map[x - 1, 1] != 3) //If the previous row doesn't have a 3
                        {
                            if (possible[x - 1] == false)
                            {
                                map[x, Random.Range(0, 2)] = Random.Range(1, 4);
                                if (map[x, 0] != 3 & map[x, 1] != 3)
                                {
                                    possible[x] = false;
                                    //Debug.Log(x + " row is 0,0 + not first row + row-1 has no 3 and it's not possible + this row is not possible");
                                }
                                //Debug.Log(x + " row is 0,0 + not first row + row-1 has no 3 and it's not possible");
                            }
                            else if (x > 1)
                            {

                                if (map[x - 2, 0] == 3 | map[x - 2, 1] == 3)
                                {
                                    map[x, Random.Range(0, 2)] = 1;
                                    possible[x] = false;
                                    //Debug.Log(x + " row is 0,0 + not first row or 2nd + row-1 has no 3 but it's possible bc there is a 3 rows-2 before");
                                }
                                else if (map[x - 2, 0] == 0 & map[x - 2, 1] == 0)
                                {
                                    map[x, Random.Range(0, 2)] = Random.Range(1, 3);
                                    //Debug.Log(x + " row is 0,0 + not first row or 2nd + row-1 has no 3 and it's possible bc there is two 0 rows-2 before");
                                }
                                else
                                {
                                    map[x - 1, Random.Range(0, 2)] = 3;
                                    //Debug.Log(x + " row is 0,0 + not first row or 2nd + row-1 has no 3 and it's not possible so we put one");
                                }
                            }
                            else
                            {
                                map[x - 1, Random.Range(0, 2)] = 3;
                                //Debug.Log(x + " row is 0,0 + 2nd row + so we put 3 in 1st row");
                            }
                        }
                        else if (x < mapLength - 1)
                        {
                            map[x + 1, Random.Range(0, 2)] = 1;
                            //Debug.Log(x + " row is 0,0 + not first row + row-1 has a 3 so row+1 is 1");
                        }
                    }
                    else //If it's the first row
                    {
                        map[x, Random.Range(0, 2)] = Random.Range(1, 3); //1 or 2
                        //Debug.Log(x + " row is 0,0 + the first row so it's 1 or 2");
                    }
                }
                else if (map[x, 0] == 1 | map[x, 1] == 1) //There is at least one 1
                {
                    if (x > 0) //If it's not the first row
                    {
                        if (map[x - 1, 0] == 0 & map[x - 1, 1] == 0) //Both 0 in previous
                        {
                            if (x > 1)
                            {
                                if (map[x - 2, 0] != 3 & map[x - 2, 1] != 3) //No 3 in two rows before
                                {
                                    map[x - 2, Random.Range(0, 2)] = 3;
                                    //Debug.Log(x + " row has a 1 + not 1st + prev is 0,0 + not  2nd + prev+1 has no 3 so we give");
                                }
                            }
                            else //x = 1 and map[x - 1] is 0,0
                            {
                                map[x - 1, Random.Range(0, 2)] = Random.Range(1, 3);
                                //Debug.Log(x + " row has a 1 + not 1st + prev is 0,0 + 2nd so we make first 1 or 2");
                            }
                        }
                        else if (map[x - 1, 0] == 3 | map[x - 1, 1] == 3)
                        {
                            map[x, 0] = 0;
                            map[x, 1] = 0;
                            //Debug.Log(x + " row has a 1 + not 1st + prev is one+ 3");
                        }
                        else if (map[x - 1, 0] == 1 | map[x - 1, 1] == 1)
                        {
                            for (int i = 0; i < mapWidth; i++)
                            {
                                if (map[x, i] == 1)
                                {
                                    map[x, i] = Random.Range(1, 3);
                                    //Debug.Log(x + " row has a 1 + not 1st + prev has a 1 so this is 1 or 2");
                                }
                            }
                        }
                    }
                    else //It is the first row
                    {
                        if (map[x, 0] == 1)
                        {
                            map[x, 0] = Random.Range(1, 3);
                            //Debug.Log(x + " row has a 1 + 1st so it's 1 or 2");
                        }
                        if (map[x, 1] == 1)
                        {
                            map[x, 1] = Random.Range(1, 3);
                            //Debug.Log(x + " row has a 1 + 1st so it's 1 or 2");
                        }
                    }
                }
                else if (map[x, 0] == 3 | map[x, 1] == 3) //At least one 3 in map[x]
                {
                    if (x > 0)
                    {
                        if (map[x - 1, 0] != 1 & map[x - 1, 1] != 1)
                        {
                            map[x - 1, Random.Range(0, 2)] = 1;
                            //Debug.Log(x + " row has a 3 + not 1st + prev no 1 so give prev 1");
                        }
                    }
                }
            }

            //Define positions
            for (int x = 0; x < mapLength; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    pos[x, z] = new Vector3((startPos.x + (groundPlatSize.x + 5) * x) + Random.Range(-1.0f, 1.0f), (startPos.y - (startObjSize.y - startObjSize.y / 2)) + Random.Range(-1.0f, 1.0f), (startPos.z - 5 + (groundPlatSize.z * 2) * z) + Random.Range(-1.0f, 1.0f));
                }
            }


            //Instantiate Platforms
            for (int x = mapLength - 1; x >= 0; x--)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //bakeNavMesh();
                    if (map[x, z] == 1)
                    {
                        objMap[x, z] = Instantiate(groundPlat, pos[x, z], transform.rotation);
                        //Debug.Log(objMap[x, z].GetComponent<Collider>().bounds.size.x);
                    }
                    else if (map[x, z] == 2)
                    {
                        objMap[x, z] = Instantiate(timedPlat, pos[x, z], transform.rotation);

                    }
                    else if (map[x, z] == 3)
                    {
                        objMap[x, z] = Instantiate(horizontalPlat, pos[x, z], transform.rotation);
                    }
                    else
                    {
                        objMap[x, z] = Instantiate(empty, pos[x, z], transform.rotation);
                    }
                    
                }
            }

            //Set target for Horizontal Platforms
            for (int x = 0; x < mapLength; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    if (objMap[x, z].name.Equals("HorizontalPlatform(Clone)"))
                    {                   
                        objMap[x, z].GetComponent<HorizontalPlatform>().speed = Random.Range(4.0f, 9.0f);
                        objMap[x, z].GetComponent<HorizontalPlatform>().timeDelay = Random.Range(2.0f, 5.0f);
                        if (x < mapLength - 2)
                        {
                            for (int j = 0; j < mapWidth; j++)
                            {
                                if (objMap[x + 2, j].name.Equals("Platform(Clone)"))
                                {
                                    objMap[x, z].GetComponent<HorizontalPlatform>().nextPlatform = objMap[x + 2, j];
                                }
                            }
                        }
                        else
                        {
                            objMap[x, z].GetComponent<HorizontalPlatform>().nextPlatform = endObject;

                        }
                    }
                    else if (objMap[x,z].name.Equals("Platform(Clone)"))
                    {
                        objMap[x, z].transform.localScale += new Vector3(Random.Range(-0.2f, 0.5f), Random.Range(-0.2f, 0.5f), Random.Range(-0.2f, 0.5f));
                        int enemyNum = Random.Range(0, 2);
                        if (enemyNum == 1)
                        {
                            Instantiate(enemy, new Vector3(pos[x, z].x, pos[x, z].y + 1, pos[x, z].z), Quaternion.identity);
                        }
                        //int toxicNum = Random.Range(0, 2);
                        //if (toxicNum == 1)
                        //{
                        //    Instantiate(waste, new Vector3(objMap[x, z].transform.position.x + Random.Range(-objMap[x, z].GetComponent<Collider>().bounds.size.x / 2, objMap[x, z].GetComponent<Collider>().bounds.size.x / 2), pos[x, z].y + 1, objMap[x, z].transform.position.z + Random.Range(-objMap[x, z].GetComponent<Collider>().bounds.size.z / 2, objMap[x, z].GetComponent<Collider>().bounds.size.z / 2)), Quaternion.identity);
                        //}
                        if (x < mapLength - 1)
                        {
                            if (objMap[x + 1, z].name.Equals("Platform(Clone)"))
                            {                                
                                int[] jellyNum = new int[5];
                                for (int i = 0; i < 5; i++)
                                {
                                    Debug.Log(jellyFish.GetComponent<Renderer>().bounds.size.z);
                                    Debug.Log(objMap[x, z].transform.position.z + objMap[x, z].GetComponent<Renderer>().bounds.size.z / 2 - jellyFish.GetComponent<Renderer>().bounds.size.z * i);
                                    jellyNum[i] = Random.Range(0, 2);
                                    if (jellyNum[i] == 1)
                                    {
                                        Instantiate(jellyFish, new Vector3(objMap[x, z].transform.position.x + objMap[x, z].GetComponent<Collider>().bounds.size.x / 2.0f + 2.0f, objMap[x, z].transform.position.y, (objMap[x, z].transform.position.z + objMap[x, z].GetComponent<Collider>().bounds.size.z / 2.0f) - ((jellyFish.GetComponent<Renderer>().bounds.size.z + 0.5f) * i)), Quaternion.identity);
                                    }
                                }
                            }
                        }
                        if (z == 0)
                        {                            
                            if (objMap[x, z + 1].name.Equals("Platform(Clone)"))
                            {
                                int[] jellyNum = new int[5]; 
                                for (int i = 0; i < 5; i++)
                                {
                                    jellyNum[i] = Random.Range(0, 2);
                                    if (jellyNum[i] == 1)
                                    {
                                        Instantiate(jellyFish, new Vector3((objMap[x, z].transform.position.x + objMap[x, z].GetComponent<Collider>().bounds.size.x / 2.0f) - ((jellyFish.GetComponent<Renderer>().bounds.size.x + 0.5f) * i), pos[x, z].y, objMap[x, z].transform.position.z + objMap[x, z].GetComponent<Collider>().bounds.size.z / 2.0f + 2.0f), Quaternion.identity);
                                    }
                                }
                            }
                        }
                    }
                    else if (objMap[x, z].name.Equals("TimeedPlatform(Clone)"))
                    {
                        objMap[x, z].GetComponent<TimedPlatform>().timeToTogglePlatform = Random.Range(4.0f, 6.0f);
                    }
                }
            }
        }
        else //Z
        {
            //Distance between boundary checkpoints and PCG egde platforms
            startPos = new Vector3(startObjPos.x, startObjPos.y, startObjPos.z + startObjSize.z / 2 + 7);
            endPos = new Vector3(endObjPos.x, endObjPos.y, endObjPos.z - endObjSize.z / 2);

            //Number of platforms that can fit between the two checkpoint
            //int platNum = Mathf.FloorToInt(Mathf.Abs(startPos.z - endPos.z)) / groundPlatSize.z;

            //Set parameters for the grid
            mapLength = Mathf.FloorToInt(Mathf.Abs(startPos.z - endPos.z) / (groundPlatSize.z + 4.9f));
            mapWidth = 2;

            //Create grid
            map = new int[mapLength, mapWidth];
            objMap = new GameObject[mapLength, mapWidth];
            pos = new Vector3[mapLength, mapWidth];

            //Randomly fill 2D array map[x,z], at least one z is 1 in an x
            for (int x = 0; x < mapLength; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    map[x, z] = Random.Range(0, 2);
                }

            }

            bool[] possible = new bool[mapLength]; //To check if the next row can have a 3
            for (int i = 0; i < possible.Length; i++)
            {
                possible[i] = true;
            }
            //Update map[x,z] with 2 and 3 based on certain conditions starting from the end
            for (int x = 0; x < mapLength; x++)
            {
                //Debug.Log(x + ": " + map[x, 0] + "," + map[x, 1]);
                if (map[x, 0] == 0 & map[x, 1] == 0) //Check if row is both 0
                {
                    if (x > 0) //If it's not the first row
                    {
                        if (map[x - 1, 0] != 3 & map[x - 1, 1] != 3) //If the previous row doesn't have a 3
                        {
                            if (possible[x - 1] == false)
                            {
                                map[x, Random.Range(0, 2)] = Random.Range(1, 4);
                                if (map[x, 0] != 3 & map[x, 1] != 3)
                                {
                                    possible[x] = false;
                                    //Debug.Log(x + " row is 0,0 + not first row + row-1 has no 3 and it's not possible + this row is not possible");
                                }
                                //Debug.Log(x + " row is 0,0 + not first row + row-1 has no 3 and it's not possible");
                            }
                            else if (x > 1)
                            {

                                if (map[x - 2, 0] == 3 | map[x - 2, 1] == 3)
                                {
                                    map[x, Random.Range(0, 2)] = 1;
                                    possible[x] = false;
                                    //Debug.Log(x + " row is 0,0 + not first row or 2nd + row-1 has no 3 but it's possible bc there is a 3 rows-2 before");
                                }
                                else if (map[x - 2, 0] == 0 & map[x - 2, 1] == 0)
                                {
                                    map[x, Random.Range(0, 2)] = Random.Range(1, 3);
                                    //Debug.Log(x + " row is 0,0 + not first row or 2nd + row-1 has no 3 and it's possible bc there is two 0 rows-2 before");
                                }
                                else
                                {
                                    map[x - 1, Random.Range(0, 2)] = 3;
                                    //Debug.Log(x + " row is 0,0 + not first row or 2nd + row-1 has no 3 and it's not possible so we put one");
                                }
                            }
                            else
                            {
                                map[x - 1, Random.Range(0, 2)] = 3;
                                //Debug.Log(x + " row is 0,0 + 2nd row + so we put 3 in 1st row");
                            }
                        }
                        else if (x < mapLength - 1)
                        {
                            map[x + 1, Random.Range(0, 2)] = 1;
                            //Debug.Log(x + " row is 0,0 + not first row + row-1 has a 3 so row+1 is 1");
                        }
                    }
                    else //If it's the first row
                    {
                        map[x, Random.Range(0, 2)] = Random.Range(1, 3); //1 or 2
                        //Debug.Log(x + " row is 0,0 + the first row so it's 1 or 2");
                    }
                }
                else if (map[x, 0] == 1 | map[x, 1] == 1) //There is at least one 1
                {
                    if (x > 0) //If it's not the first row
                    {
                        if (map[x - 1, 0] == 0 & map[x - 1, 1] == 0) //Both 0 in previous
                        {
                            if (x > 1)
                            {
                                if (map[x - 2, 0] != 3 & map[x - 2, 1] != 3) //No 3 in two rows before
                                {
                                    map[x - 2, Random.Range(0, 2)] = 3;
                                    //Debug.Log(x + " row has a 1 + not 1st + prev is 0,0 + not  2nd + prev+1 has no 3 so we give");
                                }
                            }
                            else //x = 1 and map[x - 1] is 0,0
                            {
                                map[x - 1, Random.Range(0, 2)] = Random.Range(1, 3);
                                //Debug.Log(x + " row has a 1 + not 1st + prev is 0,0 + 2nd so we make first 1 or 2");
                            }
                        }
                        else if (map[x - 1, 0] == 3 | map[x - 1, 1] == 3)
                        {
                            map[x, 0] = 0;
                            map[x, 1] = 0;
                            //Debug.Log(x + " row has a 1 + not 1st + prev is one+ 3");
                        }
                        else if (map[x - 1, 0] == 1 | map[x - 1, 1] == 1)
                        {
                            for (int i = 0; i < mapWidth; i++)
                            {
                                if (map[x, i] == 1)
                                {
                                    map[x, i] = Random.Range(1, 3);
                                    //Debug.Log(x + " row has a 1 + not 1st + prev has a 1 so this is 1 or 2");
                                }
                            }
                        }
                    }
                    else //It is the first row
                    {
                        if (map[x, 0] == 1)
                        {
                            map[x, 0] = Random.Range(1, 3);
                            //Debug.Log(x + " row has a 1 + 1st so it's 1 or 2");
                        }
                        if (map[x, 1] == 1)
                        {
                            map[x, 1] = Random.Range(1, 3);
                            //Debug.Log(x + " row has a 1 + 1st so it's 1 or 2");
                        }
                    }
                }
                else if (map[x, 0] == 3 | map[x, 1] == 3) //At least one 3 in map[x]
                {
                    if (x > 0)
                    {
                        if (map[x - 1, 0] != 1 & map[x - 1, 1] != 1)
                        {
                            map[x - 1, Random.Range(0, 2)] = 1;
                            Debug.Log(x + " row has a 3 + not 1st + prev no 1 so give prev 1");
                        }
                    }
                }
            }

            //Define positions
            for (int x = 0; x < mapLength; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    pos[x, z] = new Vector3((startPos.x - 5 + (groundPlatSize.x + 2) * z) + Random.Range(-1.0f, 1.0f), (startPos.y - (startObjSize.y - startObjSize.y / 2)) + Random.Range(-1.0f, 1.0f), (startPos.z + (groundPlatSize.z + 5) * x) + Random.Range(-1.0f, 1.0f));
                    //pos[x, z] = new Vector3((startPos.x + (groundPlatSize.x + 5) * x) + Random.Range(-1.0f, 1.0f), (startPos.y - (startObjSize.y - startObjSize.y / 2)) + Random.Range(-1.0f, 1.0f), (startPos.z - 5 + (groundPlatSize.z * 2) * z) + Random.Range(-1.0f, 1.0f));

                }
            }


            //Instantiate Platforms
            for (int x = mapLength - 1; x >= 0; x--)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    if (map[x, z] == 1)
                    {
                        objMap[x, z] = Instantiate(groundPlat, pos[x, z], transform.rotation);
                        //Debug.Log(x + "," + z + ": " + objMap[x, z].name);
                    }
                    else if (map[x, z] == 2)
                    {
                        objMap[x, z] = Instantiate(timedPlat, pos[x, z], transform.rotation);
                        //Debug.Log(x + "," + z + ": " + objMap[x, z].name);

                    }
                    else if (map[x, z] == 3)
                    {
                        objMap[x, z] = Instantiate(horizontalPlat, pos[x, z], transform.rotation);
                        //Debug.Log(x + "," + z + ": " + objMap[x, z].name);
                    }
                    else
                    {
                        objMap[x, z] = Instantiate(empty, pos[x, z], transform.rotation);
                    }
                }
            }

            //Set target for Horizontal Platforms
            for (int x = 0; x < mapLength; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    if (objMap[x, z].name.Equals("HorizontalPlatform(Clone)"))
                    {
                        if (x < mapLength - 2)
                        {
                            for (int j = 0; j < mapWidth; j++)
                            {
                                if (objMap[x + 2, j].name.Equals("Platform(Clone)"))
                                {
                                    objMap[x, z].GetComponent<HorizontalPlatform>().nextPlatform = objMap[x + 2, j];
                                    objMap[x, z].GetComponent<HorizontalPlatform>().speed = Random.Range(4.0f, 9.0f);
                                    objMap[x, z].GetComponent<HorizontalPlatform>().timeDelay = Random.Range(1.0f, 4.0f);
                                }
                            }
                        }
                        else
                        {
                            objMap[x, z].GetComponent<HorizontalPlatform>().nextPlatform = endObject;
                            objMap[x, z].GetComponent<HorizontalPlatform>().speed = Random.Range(4.0f, 9.0f);
                            objMap[x, z].GetComponent<HorizontalPlatform>().timeDelay = Random.Range(1.0f, 4.0f);
                        }
                    }
                    else if (objMap[x, z].name.Equals("Platform(Clone)"))
                    {
                        objMap[x, z].transform.localScale += new Vector3(Random.Range(-0.2f, 0.5f), Random.Range(-0.2f, 0.5f), Random.Range(-0.2f, 0.5f));
                        int enemyNum = Random.Range(0, 2);
                        if (enemyNum == 1)
                        {
                            Instantiate(enemy, new Vector3(pos[x, z].x, pos[x, z].y + 1, pos[x, z].z), Quaternion.identity);
                        }




                        if (x < mapLength - 1)
                        {
                            if (objMap[x + 1, z].name.Equals("Platform(Clone)"))
                            {
                                int[] jellyNum = new int[5];
                                for (int i = 0; i < 5; i++)
                                {
                                    jellyNum[i] = Random.Range(0, 2);
                                    if (jellyNum[i] == 1)
                                    {
                                        Instantiate(jellyFish, new Vector3((objMap[x, z].transform.position.x + objMap[x, z].GetComponent<Collider>().bounds.size.x / 2.0f) - ((jellyFish.GetComponent<Renderer>().bounds.size.x + 0.5f) * i), pos[x, z].y, objMap[x, z].transform.position.z + objMap[x, z].GetComponent<Collider>().bounds.size.z / 2.0f + 2.0f), Quaternion.identity);
                                    }
                                }
                            }
                        }
                        if (z == 0)
                        {
                            if (objMap[x, z + 1].name.Equals("Platform(Clone)"))
                            {
                                int[] jellyNum = new int[5];
                                for (int i = 0; i < 5; i++)
                                {
                                    jellyNum[i] = Random.Range(0, 2);
                                    if (jellyNum[i] == 1)
                                    {
                                        Instantiate(jellyFish, new Vector3(objMap[x, z].transform.position.x + objMap[x, z].GetComponent<Collider>().bounds.size.x / 2.0f + 2.0f, objMap[x, z].transform.position.y, (objMap[x, z].transform.position.z + objMap[x, z].GetComponent<Collider>().bounds.size.z / 2.0f) - ((jellyFish.GetComponent<Renderer>().bounds.size.z + 0.5f) * i)), Quaternion.identity);
                                    }
                                }
                            }
                        }
                    }
                    else if (objMap[x, z].name.Equals("TimeedPlatform(Clone)"))
                    {
                        objMap[x, z].GetComponent<TimedPlatform>().timeToTogglePlatform = Random.Range(2.0f, 5.0f);
                    }
                }
            }
        }


        instantiated = true;
    }
}
