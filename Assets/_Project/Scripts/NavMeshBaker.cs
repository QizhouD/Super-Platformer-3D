using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface[] platforms;
    public GeneratePlatforms generatePlatforms;
    void Start()
    {
        for (int i = 0; i < platforms.Length; i++) {
            platforms[i].BuildNavMesh();
        }

    }

    // Update is called once per frame
    
}
