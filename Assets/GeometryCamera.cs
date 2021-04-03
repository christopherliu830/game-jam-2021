using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryCamera : MonoBehaviour
{

    Camera _camera;
    Camera _mainCamera;
    RenderTexture texture;
    Texture2D _perlin;

    void Start() {
        // var tex = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 24);
        _camera = GetComponent<Camera>();
        _mainCamera = Camera.main;
        texture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 24, RenderTextureFormat.ARGB32);
        texture.filterMode = FilterMode.Point;
        _camera.targetTexture = texture;
        _camera.SetReplacementShader(Shader.Find("Unlit/White Only"), "");
        Shader.SetGlobalTexture("_GeometryTex", texture);
        CreatePerlin();
    }

    void LateUpdate() {
        _camera.orthographicSize = _mainCamera.orthographicSize;
    }

    void CreatePerlin() {
        int w = 512, h = 512;
        _perlin = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for(float x = 0; x < w; x++) {
            for(float y = 0; y < w; y++) {
                float xCoord = x / w;
                float yCoord = y / h;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pixels[(int)y * w + (int)x] = new Color(sample, sample, sample);
            }
        }
        Debug.Log("?");
        _perlin.SetPixels(pixels);
        _perlin.Apply();
        Shader.SetGlobalTexture("_Perlin", _perlin);
    }
}
