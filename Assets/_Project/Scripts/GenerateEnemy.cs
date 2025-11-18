using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject platform;
    public GameObject enemy;
    void Start()
    {
        //Instantiate(platform, new Vector3(50, 20, 20), Quaternion.identity);
        //generate enemy
        /*for (int i = 0; i < 5; i++) {
            bool condition = false;
            int x = Random.Range(7, 15);
            int z = Random.Range(7, 15);
            
            Instantiate(enemy, new Vector3(50+x, 24, 20+z), Quaternion.identity);
        }*/
        int[,] map = new int[4,3];
        bool end = false;
        //goal
        map[0, 1] = 3;
        //start
        map[3, 1] = 4;
        int currentX = 1;
        int currentY = 3;
        while (end == false) {
            //choose a direction
            int direction = Random.Range(0, 3);
            //0 up, 1 left, 2 right
            if (direction == 0)
            {
                currentY -= 1;
                //Debug.Log(currentY);
                map[currentY, currentX] = 2;
            }
            else if (direction == 1)
            {
                //test boundary
                if (currentX - 1 >= 0)
                {
                    //Debug.Log("inside");
                    //Debug.Log("currentY is " + currentY + "currentX - 1 is " + (currentX-1));
                    if (currentY == 3 && currentX - 1 != 1)
                    {
                        //Debug.Log("inside first if ");
                        currentX = currentX - 1;
                        map[currentY, currentX] = 2;
                    }

                    if (currentY + 1 <= 3)
                    {
                        //test if there is a block under if so it is a invalid spawn
                        if (map[currentY + 1, currentX - 1] == 0)
                        {
                            currentX = currentX - 1;
                            map[currentY, currentX] = 2;
                        }
                        
                    }
                }
            }
            else if (direction == 2) {
                //test boundary
                if (currentX + 1 <= 2)
                {
                    if (currentY + 1 <= 3)
                    {
                        //test if there is a block under if so it is a invalid spawn
                        if (map[currentY + 1, currentX + 1] == 0)
                        {
                            currentX = currentX + 1;
                            map[currentY, currentX] = 2;
                        }
                    }
                    if (currentY == 3 && currentX + 1 != 1)
                    {
                        currentX = currentX + 1;
                        map[currentY, currentX] = 2;
                    }
                }
            }
            //end condition
            if (currentY == 0 || (currentY == 1 && currentX == 1)) {
                end = true;
                //Debug.Log("the end location is " +  map[currentY, currentX]);
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        //Debug.Log(map[y, x]);
                        if (map[y, x] == 2 || map[y, x] == 3 || map[y, x] == 4)
                        {
                            Instantiate(platform, new Vector3(50 + 20 * x, 5, 50 + 20 * y), Quaternion.identity);
                        }
                    }

                }
            }
            

        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
