using UnityEngine;
using System.Collections;

public class InputMgr : MonoBehaviour {
    public bool JustTapped {get; private set;}
    public Vector2 TapPos {get; private set;}
    public Vector2 NormalizedTapPos {get; private set;}
    public float TapLength {get; private set;}
    const float MAX_TAP_DIST = 25f;

    Vector2 tapStart;
    float tapStartTime;

	// Use this for initialization
	void Awake () {
        JustTapped = false;
        TapPos = Vector2.zero;
        NormalizedTapPos = Vector2.zero;
        TapLength = 0f;
        tapStart = Vector2.zero;
        tapStartTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        ClearTaps();
        if (Input.touchCount == 1) {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) {
                Pressed(t.position);
            } else if (t.phase == TouchPhase.Ended) {
                Released(t.position);
            }
        } else if (Input.GetMouseButtonDown(0)) {
            Pressed(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        } else if (Input.GetMouseButtonUp(0)) {
            Released(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
	}

    void ClearTaps () {
        JustTapped = false;
    }

    void Pressed (Vector2 screenPos) {
        tapStart = screenPos;
        tapStartTime = Time.time;
    }

    void Released (Vector2 screenPos) {
        if ((screenPos - tapStart).magnitude > MAX_TAP_DIST) {
            return;
        }
        JustTapped = true;
        TapPos = tapStart;
        NormalizedTapPos = new Vector2(TapPos.x / Screen.width,
                                       TapPos.y / Screen.height);
        TapLength = Time.time - tapStartTime;
    }
}
