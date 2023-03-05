using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public List<GameObject> asteroids = new List<GameObject>();
    public GameObject celestialObjectsParent;
    public int minAsteroids = 0;
    public int maxAsteroids = 20;
    public int maxX;
    public int maxY;
    public int maxZ;    
    public int minX;
    public int minY;
    public int minZ;

    // Start is called before the first frame update
    void Start()
    {

        int numberOfAsteriods = Random.Range(minAsteroids, maxAsteroids);

        Debug.Log("Spawning Asteroids");
        Debug.Log("Number of Asteroids to Spawn: " + numberOfAsteriods);

        for (int i = 0; i < numberOfAsteriods; i++)
        {
            int newX = Random.Range(minX, maxX);
            int newY = Random.Range(minY, maxY);
            int newZ = Random.Range(minZ, maxZ);

            GameObject newAsteroid = Instantiate(asteroids[Random.Range(0, asteroids.Count)]);

            newAsteroid.transform.position = new Vector3(newX, newY, newZ);

            newAsteroid.transform.SetParent(celestialObjectsParent.transform);
            
        }
    }
}
