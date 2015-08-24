using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlienMgr : MonoBehaviour {
    public float minutesLeftToSpawn;
    public int maxSpawns;

    GameObject alienPrefab;
    List<GameObject> aliens;

    void Awake () {
        alienPrefab = Resources.Load<GameObject>("Prefabs/Alien");
        aliens = new List<GameObject>();
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        int numAliens = AliensNeededNow();
        if (aliens.Count < numAliens) {
            GameObject newAlien = Instantiate<GameObject>(alienPrefab);
            newAlien.transform.position = new Vector3(0f, 15f, 0f);
            aliens.Add(newAlien);
        }
    }

    public void ClearAliens () {
        for (int i = 0; i < aliens.Count; i++) {
            Destroy(aliens[i]);
        }
        aliens.Clear();
    }

    int AliensNeededNow () {
        System.DateTime expires = new System.DateTime(
                FindObjectOfType<LevelGen>().World.GetLong("expires"));
        float minutesLeft = (float) (expires - System.DateTime.UtcNow)
                .TotalMinutes;
        return Mathf.Clamp(Mathf.CeilToInt(
                (1f - minutesLeft / minutesLeftToSpawn) * maxSpawns), 0,
                maxSpawns);
    }
    
    public Alien FindClosest (Vector3 pos) {
        if (aliens.Count == 0) {
            return null;
        }
        Alien closest = aliens[0].GetComponent<Alien>();
        float closestSqrDist = (closest.transform.position - pos).sqrMagnitude;
        foreach (GameObject obj in aliens) {
            float sqrDist = (obj.transform.position - pos).sqrMagnitude;
            if (sqrDist < closestSqrDist) {
                closest = obj.GetComponent<Alien>();
                closestSqrDist = sqrDist;
            }
        }
        return closest;
    }
}
