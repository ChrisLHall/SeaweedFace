using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {
    SpriteRenderer sr;
    public string itemName;
    public int initValue;
    public int level;

    const float SCALE_INCREMENT = 0.2f;

	// Use this for initialization
	void Start () {
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/" + itemName);
        transform.localScale = transform.localScale
                * (1f + SCALE_INCREMENT * (initValue + level));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
