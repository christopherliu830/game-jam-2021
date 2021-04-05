using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public bool xOnly;
    public float parallaxy;

    [SerializeField]
    public List<Transform> targets;
    private Camera _camera;
    private Vector3 _oldPosition;
    private float _dZ = 0;
    private float _size = 0;

    void Start()
    {
        _camera = Camera.main;
        targets = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++) {
            targets.Add(transform.GetChild(i));
        }

        foreach(var target in targets) {
            _dZ = Mathf.Max(_dZ, -target.localPosition.z);
        }
        _size = _camera.orthographicSize;
    }

    void LateUpdate()
    {
        Vector3 delta = _camera.transform.position - _oldPosition;
        float cameraScale = _camera.orthographicSize / _size;
        foreach(var target in targets) {
            var scaling = (_dZ + target.localPosition.z) / _dZ;
            Vector3 p = target.position;
            p.x = _camera.transform.position.x * scaling;
            if (xOnly) {
                p.y = _camera.transform.position.y;
            } else {
                p.y += delta.y * scaling;
            }
            target.position = p;
            target.localScale = Vector2.one * cameraScale;
        }
        _oldPosition = _camera.transform.position;
    }
}

[System.Serializable]
public struct Parallaxed {
    public Transform transform;
    public float scale;
}
