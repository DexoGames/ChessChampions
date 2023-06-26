using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] objects;
    GameObject[] spawnedObjects;

    private void OnEnable()
    {
        spawnedObjects = new GameObject[objects.Length];

        for(int i = 0; i < objects.Length; i++)
        {
            spawnedObjects[i] = Instantiate(objects[i]);
        }
    }

    public void DestroyBoard()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }
    }
}
