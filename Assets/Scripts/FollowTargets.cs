using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargets : MonoBehaviour
{
    public List<Transform> targets;
    public float followDistance;
    public float farDamp;
    public float closeDamp;
    public Stack<List<Transform>> focusStack = new Stack<List<Transform>>();

    private Camera _camera;

    public void Focus(Transform t) {
        targets = new List<Transform>() { t };
    }

    public void Focus(List<Transform> t) {
        targets = t;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 pos = Vector3.zero;
        foreach(Transform target in targets) {
            pos += target.position;
        }
        pos /= targets.Count;

        pos.z = transform.position.z;
        if ((pos - transform.position).magnitude > followDistance) {
            transform.position = Vector3.Slerp(transform.position, pos, farDamp);
        } else {
            transform.position = Vector3.Slerp(transform.position, pos, closeDamp);
        }
    }

}
