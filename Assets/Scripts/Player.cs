using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    Main main;

    Vector3 camOffset;
    float camAngle;
    const float SPIN_SPEED = 30f;
    Vector3 currentCamOffset;
    const float OFFSET_ALPHA = 0.05f;

    Vector3 physicsCtrlVec;
    const float CTRL_VEC_STRENGTH = 3f;
    const float VERTICAL_OFFSET = 0.5f;
    const float VERTICAL_MULT = 0.7f;
    const float SIDEWAYS_MULT = 0.1f;
    const float MAX_TAP_TIME = 0.4f;
    const float MAX_LONGPRESS_TIME = 2f;
    const float MAX_VELOCITY = 0.3f;

    const float FLOAT_FORCE = 0.2f;
    const float MAP_BOTTOM = 2f;

    void Awake () {
        main = FindObjectOfType<Main>();
        camAngle = 0f;
        physicsCtrlVec = Vector2.zero;
    }

	// Use this for initialization
	void Start () {
        Debug.Log(Login.User.Username);
        camOffset = transform.FindChild("CamOffset").position
                - transform.position;
        currentCamOffset = camOffset;

        FindStartPos();
	}
	
	// Update is called once per frame
	void Update () {
        ProcessControls();

        Vector3 angledCamOffset = Quaternion.AngleAxis(camAngle, Vector3.up)
                * camOffset;
        currentCamOffset = Vector3.Lerp(currentCamOffset, angledCamOffset,
                                        OFFSET_ALPHA);
        Camera.main.transform.position = transform.position + currentCamOffset;
        Camera.main.transform.LookAt(transform.position);
	}

    void FixedUpdate () {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (physicsCtrlVec != Vector3.zero) {
            rb.AddForce(physicsCtrlVec * CTRL_VEC_STRENGTH, ForceMode.Impulse);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, MAX_VELOCITY);
            physicsCtrlVec = Vector3.zero;
        }

        if (transform.position.y < MAP_BOTTOM) {
            rb.AddForce(Vector3.up * FLOAT_FORCE, ForceMode.Impulse);
        }
    }

    void FindStartPos() {
        LevelGen lg = FindObjectOfType<LevelGen>();
        transform.position = lg.FindSuitableSpawn() + 3f * Vector3.up;
    }

    void ProcessControls () {
        if (main.InputMgr.JustTapped) {
            Vector2 fromMiddle
                    = main.InputMgr.NormalizedTapPos - Vector2.one * 0.5f;
            float upComponent = VERTICAL_MULT
                    * Mathf.Max(0f, VERTICAL_OFFSET
                    + 0.5f - fromMiddle.magnitude);
            physicsCtrlVec = Quaternion.AngleAxis(camAngle, Vector3.up)
                    * new Vector3(fromMiddle.x * SIDEWAYS_MULT, upComponent,
                    fromMiddle.y);
            camAngle += fromMiddle.x * SPIN_SPEED;
        }
    }
}
