using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class UnderwaterEffect : MonoBehaviour
{
    public LayerMask waterLayers;
    public UnityEngine.Shader shader;
    bool inWater;
    [Header("Depth Effect")]

    public Color depthColour = new Color(0f, 0.42f, 0.87f);
    public float depthStart = -12, depthEnd = 98;
    public LayerMask depthLayers = ~0;

    Camera cam, depthCam;
    RenderTexture depthTexture, colourTexture;
    Material material;

    void Start()
    {
        cam = GetComponent<Camera>();
        //cam.depthTextureMode = DepthTextureMode.Depth;
        if (shader) { material = new Material(shader); }

        depthTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 16, RenderTextureFormat.Depth);
        colourTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.Default);

        GameObject go = new GameObject("Depth Cam");
        depthCam = go.AddComponent<Camera>();
        go.transform.SetParent(transform);
        go.transform.position = transform.position;

        depthCam.CopyFrom(cam);
        depthCam.cullingMask = depthLayers;
        depthCam.depthTextureMode = DepthTextureMode.Depth;

        depthCam.SetTargetBuffers(colourTexture.colorBuffer, depthTexture.depthBuffer);
        depthCam.enabled = false;

        material.SetTexture("_DepthMap", depthTexture);
    }

    private void OnApplicationQuit() { 
        RenderTexture.ReleaseTemporary(depthTexture);
        RenderTexture.ReleaseTemporary(colourTexture);
    }

    private void FixedUpdate() {
        Vector3[] corners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0,0,1,1), cam.nearClipPlane, cam.stereoActiveEye, corners);

        RaycastHit hit;
        Vector3 start = transform.position + transform.TransformVector(corners[1]), end = transform.position + transform.TransformVector(corners[0]);
        Collider[] c = Physics.OverlapSphere(end, 0.01f, waterLayers);
        if (c.Length > 0)
        { // underwater
            inWater = true;
            c = Physics.OverlapSphere(start, 0.01f, waterLayers);
            if (c.Length > 0)
            {
                material.SetVector("_WaterLevel", new Vector2(0, 1));
            }
            else
            { // interpolation value for delta
                if (Physics.Linecast(start, end, out hit, waterLayers))
                {
                    float delta = hit.distance / (end - start).magnitude;
                    material.SetVector("_WaterLevel", new Vector2(0, 1 - delta));
                }

            }
        }
        else { inWater = false; }

        //Collider[] c = Physics.OverlapSphere(transform.position, 0.01f, waterLayers);
        //inWater = c.Length > 0;
    }

    void Reset()
    {
        Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach (Shader s in shaders) {
            if (s.name.Contains(this.GetType().Name)) {
                shader = s;
                return;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null && inWater)
        {
            depthCam.Render();

            material.SetColor("_DepthColour", depthColour);
            material.SetFloat("_DepthStart", depthStart);
            material.SetFloat("_DepthEnd", depthEnd);
            Graphics.Blit(source, destination, material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
