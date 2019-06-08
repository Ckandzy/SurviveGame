using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePositionTest : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        Application.targetFrameRate = 10;
    }

    // Update is called once per frame
    private void Update ()
    {
        GetComponent<Rigidbody2D>().MovePosition(this.transform.position + new Vector3(0, -1f, 0));
	}

    private void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().MovePosition(this.transform.position + new Vector3(0, 0.01f, 0));
    }
}
