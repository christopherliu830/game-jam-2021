using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField]
    public List<Parallaxed> targets;
    private Camera _camera;
    private Vector3 _oldPosition;

    void Start()
    {
        _camera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 delta = _camera.transform.position - _oldPosition;
        foreach(var target in targets) {
            Vector3 p = target.transform.position;
            p.x += delta.x * target.scale;
            p.y += delta.y * target.scale;
            target.transform.position = p;
        }
        _oldPosition = _camera.transform.position;
    }
}

[System.Serializable]
public struct Parallaxed {
    public Transform transform;
    public float scale;
}
