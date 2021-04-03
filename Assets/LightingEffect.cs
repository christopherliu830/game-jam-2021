using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingEffect : MonoBehaviour
{
    Camera _camera; 
    RenderTexture texture;
    public Material effect;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest, effect);
    }
}
