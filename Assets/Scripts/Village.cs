using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village : MonoBehaviour {
    public int TotalPoints { get; private set; }
    public bool CanDefend {
        get {
            return TotalPoints >= POINTS_FOR_CASTLE;
        }
    }
    const int POINTS_FOR_HUT = 10;
    const int POINTS_FOR_CASTLE = 30;
    const float GROWTH_INCREMENT = 0.03f;

    public Sprite tent;
    public Sprite hut;
    public Sprite castle;

    SpriteRenderer sprite;
    Vector3 origScale;

    GameObject villagersPrefab;
    List<GameObject> createdVillagers;

    GameObject laser;
    float stopLaserTime;
    float nextShotTime;
    bool startedShot;
    const float SHOT_INTERVAL = 2f;
    const float SHOT_LENGTH = 0.3f;

	// Use this for initialization
	void Awake () {
        TotalPoints = 0;
        origScale = transform.localScale;
        sprite = GetComponentInChildren<SpriteRenderer>();
        villagersPrefab = Resources.Load<GameObject>(
            "Prefabs/VillagerParticles");
        createdVillagers = new List<GameObject>();

        laser = transform.FindChild("Laser").gameObject;
        stopLaserTime = Time.time + SHOT_LENGTH;
        nextShotTime = Time.time + SHOT_LENGTH + SHOT_INTERVAL;
        startedShot = false;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Time.time < stopLaserTime) {
            if (!startedShot) {
                startedShot = true;
                Alien closest = FindObjectOfType<AlienMgr>()
                        .FindClosest(transform.position);
                if (TotalPoints >= POINTS_FOR_CASTLE && closest != null) {
                    ShowLaser(closest.transform.position);
                    GetComponent<AudioSource>().Play();
                    closest.BlowUp();
                }
            }
        } else if (Time.time < nextShotTime) {
            HideLaser();
        } else {
            stopLaserTime = Time.time + SHOT_LENGTH;
            nextShotTime = Time.time + SHOT_LENGTH
                    + Random.value * SHOT_INTERVAL;
            startedShot = false;
        }
	}

    void UpdateSprite () {
        if (TotalPoints >= POINTS_FOR_CASTLE) {
            sprite.sprite = castle;
        } else if (TotalPoints >= POINTS_FOR_HUT) {
            sprite.sprite = hut;
        } else {
            sprite.sprite = tent;
        }

        transform.localScale = origScale * (1f + TotalPoints * GROWTH_INCREMENT);
    }

    public static void RecalculateAll () {
        Village[] villages = FindObjectsOfType<Village>();
        if (villages.Length == 0) {
            Debug.LogError("There are no villages!");
            return;
        }
        foreach (Village v in villages) {
            v.TotalPoints = 0;
        }

        Item[] allItems = FindObjectsOfType<Item>();
        foreach (Item i in allItems) {
            Village closest = Village.Closest(i.transform.position, villages);
            closest.TotalPoints += i.TotalPoints;
            closest.CreateVillagers(i.transform.position);
        }
        foreach (Village v in villages) {
            v.UpdateSprite();
        }
    }

    public static Village Closest (Vector3 pos, Village[] villages) {
        Village closest = villages[0];
        float closestSqrDist = (closest.transform.position - pos).sqrMagnitude;
        foreach (Village v in villages) {
            float sqrDist = (v.transform.position - pos).sqrMagnitude;
            if (sqrDist < closestSqrDist) {
                closest = v;
                closestSqrDist = sqrDist;
            }
        }
        return closest;
    }

    GameObject CreateVillagers (Vector3 at) {
        GameObject go = Instantiate<GameObject>(villagersPrefab);
        go.transform.position = at;
        go.transform.LookAt(transform.position);
        ParticleSystem ps = go.GetComponentInChildren<ParticleSystem>();
        ps.startLifetime
                = (go.transform.position - transform.position).magnitude
                / ps.startSpeed;
        createdVillagers.Add(go);
        return go;
    }

    void DestroyAllVillagers () {
        for (int i = 0; i < createdVillagers.Count; i++) {
            Destroy(createdVillagers[i]);
        }
        createdVillagers.Clear();
    }

    void OnDestroy () {
        DestroyAllVillagers();
    }

    void ShowLaser (Vector3 dest) {
        laser.transform.localScale
                = new Vector3(1f, 1f, (dest - transform.position).magnitude);
        laser.transform.LookAt(dest);
    }

    void HideLaser () {
        laser.transform.localScale = Vector3.one * 0.01f;
    }
}
