using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[ExecuteInEditMode]
public class ColorSpill : MonoBehaviour {
    [Range(0,10)] public int times = 3;
    [Range(0,3)] public float blurSpread = 0.6f;
    [Range(0,10)] public int sampleTime = 8;
    [Range(1,32)] public int sampleSpace = 1;
    [Range(0,8)] public float bright = 0.25f;

    public Shader blurEnergyShader;
    public Shader blurShader;
    Material mat;
    Material bmat;

	// Use this for initialization
	void Start () {
        mat = new Material(blurEnergyShader);
        mat.hideFlags = HideFlags.HideAndDontSave;

        bmat = new Material(blurShader);
        bmat.hideFlags = HideFlags.HideAndDontSave;

        gameObject.GetComponent<Camera>().depthTextureMode = gameObject.GetComponent<Camera>().depthTextureMode | DepthTextureMode.DepthNormals;
	}
	
	// Update is called once per frame
	void Update () {
    }

    void OnRenderImage(RenderTexture src,RenderTexture dst)
    {
        mat.SetInt ("sampleTime", sampleTime);
        mat.SetInt ("sampleSpace", sampleSpace);
        mat.SetFloat ("_bright2", bright);
        RenderTexture blurTex = RenderTexture.GetTemporary(Screen.width /1, Screen.height / 1);
        //
        Graphics.Blit(src, blurTex, mat,0);
        //Graphics.Blit(blurTex, dst);
        RenderTexture buffer = RenderTexture.GetTemporary(src.width / 4, src.height / 4, 0);
        RenderTexture buffer2 = RenderTexture.GetTemporary(src.width / 4, src.height / 4, 0);

        DownSample4x(blurTex, buffer);//
        // Graphics.Blit(buffer, dst);
        bool oddEven = true;
        for (int i = 0; i < times; i++){
            if (oddEven)
                FourTapCone(buffer, buffer2, i);
            else
                FourTapCone(buffer2, buffer, i);
            oddEven = !oddEven;
        }
        if (oddEven){
            mat.SetTexture("_Energy", buffer);
            Graphics.Blit(src, dst,mat,1);
        } else {
            mat.SetTexture("_Energy", buffer2);
            Graphics.Blit(src, dst,mat,1);
        }
        RenderTexture.ReleaseTemporary(buffer);
        RenderTexture.ReleaseTemporary(buffer2);

        RenderTexture.ReleaseTemporary(blurTex);
    }

    public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
    {
        float off = 0.5f + iteration * blurSpread;
        Graphics.BlitMultiTap(source, dest, bmat, new Vector2(-off, -off), new Vector2(-off, off), new Vector2(off, off), new Vector2(off, -off));
    }//
    void DownSample4x(RenderTexture src, RenderTexture dst)
    {
        float off = 1;
        Graphics.BlitMultiTap(src, dst, bmat, new Vector2(off, off), new Vector2(off, -off), new Vector2(-off, off), new Vector2(-off, -off));
        //Graphics.BlitMultiTap(src, dst, mat, new Vector2(off, off));
    }
}
