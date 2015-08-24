using UnityEngine;
using System.Collections;

public class Alien : MonoBehaviour {
    Rigidbody rb;
    const float SPEED = 12f;
    const float TURN_TIME = 5f;

    float turnTime;

    ParticleSystem bullets;

    Vector3 startPos;

    void Awake () {
        rb = GetComponent<Rigidbody>();
        bullets = GetComponentInChildren<ParticleSystem>();
        turnTime = Time.time;
    }

	// Use this for initialization
	void Start () {
        ResetSpeed();
        startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        rb.velocity = rb.velocity.normalized * SPEED;
        if (Time.time > turnTime) {
            ResetSpeed();
        }

        Village v = Village.Closest(transform.position,
                                    FindObjectsOfType<Village>());
        transform.LookAt(v.transform.position);
        bullets.startLifetime = (v.transform.position - transform.position)
                .magnitude / bullets.startSpeed;
	}

    void ResetSpeed () {
        Vector3 goVec = new Vector3(Random.value - 0.5f, Random.value - 0.5f,
                                    Random.value - 0.5f);
        goVec += Vector3.up * 0.1f;
        rb.velocity = goVec.normalized * SPEED;
        turnTime = Time.time + Random.value * TURN_TIME;
    }

    public void BlowUp () {
        transform.position = startPos;
        ResetSpeed();
    }
}
