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
            double closestDist = signs.Min(
                    (Sign s) => (s.transform.position - player.transform.position)
                    .sqrMagnitude);
            if (closestDist < SIGN_READ_DIST_SQR) {
                Sign closest = signs.First((Sign s) =>
                        ((s.transform.position - player.transform.position)
                        .sqrMagnitude == closestDist));
                text.text = closest.owner + " says: \"" + closest.message + "\"";
            } else {
                text.text = "";
            }
        }
	}
}
