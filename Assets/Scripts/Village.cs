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
    const float GROWTH_INCREMENT = 0.01f;

    public Sprite tent;
    public Sprite hut;
    public Sprite castle;

    SpriteRenderer sprite;
    Vector3 origScale;

    GameObject villagersPrefab;
    List<GameObject> createdVillagers;

	// Use this for initialization
	void Awake () {
        TotalPoints = 0;
        origScale = transform.localScale;
        sprite = GetComponentInChildren<SpriteRenderer>();
        villagersPrefab = Resources.Load<GameObject>(
            "Prefabs/VillagerParticles");
        createdVillagers = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
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

    static Village Closest (Vector3 pos, Village[] villages) {
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
}
