using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    public Texture inputTexture;
    public int width = 512;
    public int height = 512;
    public ComputeShader compute;
    public Shader drawShader;

    [Header("Draw Parameters")]
    [Range(0,5)]
    public float size = 5;
    [Range(0, 1)]
    public float strength = 0.1f;

    private int kernel;
    private bool pingPong = true;
    private RenderTexture renderTexPing, renderTexPong;
    private RaycastHit hit;
    private Material displayMaterial, drawMaterial;

    void Start()
    {
        if (height < 1 || width < 1) return;

        // Material
        displayMaterial = GetComponent<MeshRenderer>().material;

        drawMaterial    = new Material(drawShader);


        renderTexPing = CreateRenderTexture(width, height, 24);
        renderTexPong = CreateRenderTexture(width, height, 24);
        Graphics.Blit(inputTexture, renderTexPing); // Note: the input texture to compute shader should be RenderTexture

        kernel = compute.FindKernel("GameOfLife");
        compute.SetFloat("Width", width);
        compute.SetFloat("Height", height);
    }


    void Update()
    {
        // Increase
        if (Input.GetKey(KeyCode.Mouse0))
        {
            drawMaterial.SetFloat("_State", 1);
            DrawTexture();
        }
        // Decrease
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            drawMaterial.SetFloat("_State", -1);
            DrawTexture();
        }


        if (height < 1 || width < 1) return;

        if (true == pingPong)
        {
            compute.SetTexture(kernel, "Input", renderTexPing);
            compute.SetTexture(kernel, "Result", renderTexPong);
            compute.Dispatch(kernel, width / 8, height / 8, 1);

            displayMaterial.mainTexture = renderTexPong;

            pingPong = false;
        }
        else
        {
            compute.SetTexture(kernel, "Input", renderTexPong);
            compute.SetTexture(kernel, "Result", renderTexPing);
            compute.Dispatch(kernel, width / 8, height / 8, 1);

            displayMaterial.mainTexture = renderTexPing;

            pingPong = true;
        }
    }


    private RenderTexture CreateRenderTexture(int w, int h, int d)
    {
        var rt = new RenderTexture(w, h, d);
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Point;
        rt.useMipMap = false;
        rt.Create();

        return rt;
    }

    private void DrawTexture()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            drawMaterial.SetVector("_Coordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
            drawMaterial.SetFloat("_Size", size);
            drawMaterial.SetFloat("_Strength", strength);

            RenderTexture temp = RenderTexture.GetTemporary(width, height, 24);

            if (pingPong)
            {
                Graphics.Blit(renderTexPing, temp);
                Graphics.Blit(temp, renderTexPing, drawMaterial);
            }
            else
            {
                Graphics.Blit(renderTexPong, temp);
                Graphics.Blit(temp, renderTexPong, drawMaterial);
            }
            RenderTexture.ReleaseTemporary(temp);
        }
    }
}
