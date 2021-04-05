using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargets : MonoBehaviour
{
    public List<Transform> targets;
    public float followDistance;
    public float farDamp;
    public float closeDamp;
    public float Size => _currentSize;
    public Stack<List<Transform>> focusStack = new Stack<List<Transform>>();

    private Camera _camera;
    private float _currentSize;

    public float mouseStrength;

    public void Focus(Transform t) {
        targets = new List<Transform>() { t };
    }

    public void Focus(List<Transform> t) {
        targets = t;
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
        _currentSize = _camera.orthographicSize;
    }

    public void SetSize(float size) {
        _currentSize = size;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var mousePoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _currentSize, 0.03f);
        Vector3 pos = Vector2.zero;
        foreach(Transform target in targets) {
            pos += target.position;
        }
        pos /= targets.Count;
        pos = Vector2.Lerp(pos, mousePoint, mouseStrength);

        pos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, pos, farDamp);
    }

}
