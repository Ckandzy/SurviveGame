using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTest : MonoBehaviour
{
    public Vector2 moveVector;
    // Angular speed in radians per sec.
    public float speed;
    protected Rigidbody2D m_Rigidbody2D;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //Debug.Log(transform.right);
        //Debug.DrawLine(transform.position, transform.position + new Vector3(1, -0.1f, 0).normalized * 2f, Color.cyan);
        transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(1, -0.1f, 0).normalized);
        //m_Rigidbody2D.MovePosition(m_Rigidbody2D.position + moveVector * Time.deltaTime);
        //Vector3 rotation = Vector3.RotateTowards(
        //        moveVector.normalized,
        //        moveVector.,
        //        projectData.trackSensitivity,
        //        0f
        //     );

        //Debug.DrawRay(transform.position, moveVector, Color.red);
        //moveVector = projectData.shootSpeed * rotation;
        //Debug.DrawRay(transform.position, rotation);
        //Debug.DrawRay(transform.position, transform.right, Color.blue);
        ////Debug.Log()
        //transform.rotation = Quaternion.FromToRotation(transform.right, rotation);
        //Debug.DrawRay(transform.position, transform.right, Color.cyan);
    }
}
