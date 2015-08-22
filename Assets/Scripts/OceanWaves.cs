using UnityEngine;
using System.Collections;

public class OceanWaves : MonoBehaviour {
    Mesh oceanMesh;
    Vector3[] vertices;
    const float TIME_FREQ = 0.8f;
    const float WAVE_HEIGHT = 0.6f;

    void Awake () {
        oceanMesh = GetComponent<MeshFilter>().mesh;
    }

    // Use this for initialization
    void Start () {
        vertices = oceanMesh.vertices;
    }

    // Update is called once per frame
    void Update () {
        Vector3[] tempVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
        	Vector3 vert = vertices[i];
        	tempVertices[i] = vert + WAVE_HEIGHT * Vector3.up
                    * Mathf.Sin(Time.time * TIME_FREQ + 0.5f * vert.x
                    + 0.8f * vert.z)
                    * Mathf.Cos(Time.time * TIME_FREQ * 1.2f + 0.8f * vert.x);
        }
        oceanMesh.vertices = tempVertices;
        oceanMesh.RecalculateBounds();
    }
}
