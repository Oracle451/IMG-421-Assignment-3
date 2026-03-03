using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    static public Vector3 POS = Vector3.zero;

    [Header("Inscribed")]

    public Vector3 range = new Vector3( 40, 10, 40 );
    public Vector3 phase = new Vector3( 0.5f, 0.4f, 0.1f );

    // FixedUpdate is called once per physics update (i.e., 50x/second)
    void FixedUpdate() 
    {
        Vector3 tPos = transform.position;
        tPos.x = Mathf.Sin( phase.x * Time.time ) * range.x;
        tPos.y = Mathf.Sin( phase.y * Time.time ) * range.y;
        tPos.z = Mathf.Sin( phase.z * Time.time ) * range.z;
        transform.position = tPos;
        POS = tPos;
    }
}
