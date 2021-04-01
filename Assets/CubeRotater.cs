using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRotater : MonoBehaviour
{
    void Update()
    {
        float incr = Time.deltaTime * 10;
        transform.Rotate(new Vector3(incr, incr * 1.5f, incr * .333f), Space.Self);
    }
}
