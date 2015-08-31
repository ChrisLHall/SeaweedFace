using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class SignGUI : MonoBehaviour {
    const float SIGN_READ_DIST_SQR = 3f;
    Player player;
    Text text;
    void Awake () {
        player = FindObjectOfType<Player>();
        text = GetComponent<Text>();
    }
	// Update is called once per frame
	void Update () {
        Sign[] signs = FindObjectsOfType<Sign>();
        if (signs.Length > 0) {
            text.text = "";
            float closestDist = 100f;
            Sign closest = signs[0];
            foreach (Sign sign in signs) {
                float dist = (sign.transform.position - player.transform.position).sqrMagnitude;
                if (dist < closestDist) {
                    closestDist = dist;
                    closest = sign;
                }
            }
            if (closestDist < SIGN_READ_DIST_SQR) {
                text.text = closest.owner + " says: \"" + closest.message + "\"";
            }
        }
	}
}
