using UnityEngine;
using System.Collections;

public class TapTarget : MonoBehaviour {
    SpriteRenderer sr;
    Vector3 origScale;

    bool active;
    public bool Active {
        get {
            return active;
        }
        private set {
            active = value;
            if (active) {
                sr.color = Color.white;
            } else {
                sr.color = Color.clear;
            }
        }
    }

    void Awake () {
        sr = GetComponent<SpriteRenderer>();
        origScale = transform.localScale;
    }

	// Use this for initialization
	void Start () {
        sr.color = Color.clear;
	}
	
	// Update is called once per frame
	void Update () {
        transform.localScale = (1f + 0.3f * Mathf.Sin(Time.time * 2f))
                * origScale;
	}

    public void TargetFromScreen (Vector2 screenPos) {
        Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(screenPos.x, screenPos.y, 0f));
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        if (hit.collider.gameObject.name == "Island(Clone)"
                && hit.normal == Vector3.up) {
            transform.position = hit.point;
            Active = true;
        }
    }

    public void Deselect () {
        Active = false;
    }
}
