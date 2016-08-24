using UnityEngine;
using System.Collections;
using System;

public class LevelPiece : MonoBehaviour {

    public enum PivotType
    {
        Vertex,
        Edge,
        Center,
        Grid,
    }

    public PivotType pivot;
    public bool isStair = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
