using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VVVV
{
    int a = 0;
    public VVVV(int t)
    {
        a = t;
    }
    public void TTTTTT(UnityEngine.Object obj)
    {
        GameObject bb = GameObject.Instantiate(obj as GameObject);
        Debug.Log("2222");
    }
}

class TVolumeLightingSample
{
    public Vector3 Position;
    public float Radius;
    public Vector3[] Lighting = new Vector3[9];
    public Color PackedSkyBentNormal;
    public float DirectionalLightShadowing;

    public TVolumeLightingSample()
    {
        Position = Vector3.zero;
        Radius = 0.0f;
        PackedSkyBentNormal = new Color(127.0f / 255.0f, 127.0f / 255.0f, 1.0f);
        DirectionalLightShadowing = 1.0f;
    }
};



public class Test : MonoBehaviour
{
    VVVV ww1 = new VVVV(1);
    VVVV ww2 = new VVVV(2);
    
    //  List<GameObject> cubelist = new List<GameObject>();
    //   List<GameObject> Spherelist = new List<GameObject>();

    GameObject cubeobj = null;
    GameObject sphereobj = null;

    ResourceRequest cube1 = new ResourceRequest();
    ResourceRequest sphere1 = new ResourceRequest();

    List<string> mmmmLogLost = new List<string>();

    float nCurTIme = -1;
    long nLastLength = 0;

    public static Test Instance = null;
    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        ResourceLoadManager.Instance.Initialize();
        Debug.Log(Application.persistentDataPath);

        TestThread tt = gameObject.GetComponent<TestThread>();
        for (int i = 0; i < 10; ++i)
        {
            tt.AddDataReq("TestBundle  " + i, TestCB);
        }
    }

    void TestCB(byte[] bytes, string dded)
    {
        Debug.Log(dded);
    }

    public void ReadGTZ()
    {
        List<TVolumeLightingSample> mDataList = new List<TVolumeLightingSample>();

        byte[] bytes = File.ReadAllBytes("Assets/aa.gtz");

        MemoryStream ms = new MemoryStream(bytes);
        BinaryReader br = new BinaryReader(ms);
        int nCount = br.ReadInt32();

        TVolumeLightingSample pData = null;
        for (int i = 0; i < nCount; ++i)
        {
            pData = new TVolumeLightingSample();
            pData.Position.x = br.ReadSingle();
            pData.Position.y = br.ReadSingle();
            pData.Position.z = br.ReadSingle();
            pData.Radius = br.ReadSingle();
            pData.PackedSkyBentNormal.r = br.ReadByte();
            pData.PackedSkyBentNormal.g = br.ReadByte();
            pData.PackedSkyBentNormal.b = br.ReadByte();
            pData.DirectionalLightShadowing = br.ReadSingle();

            for (int n = 0; n < 9; ++n)
            {
                pData.Lighting[n].x = br.ReadSingle();
                pData.Lighting[n].y = br.ReadSingle();
                pData.Lighting[n].z = br.ReadSingle();
            }
            mDataList.Add(pData);
        }

        br.Close();
        ms.Close();
        Debug.Log("111");
    }

    void Res(UnityEngine.Object obj)
    {
        GameObject aa = obj as GameObject;
        GameObject bb = GameObject.Instantiate(aa);

    }

    public void Log(string s)
    {
        mmmmLogLost.Add(s);
        if (mmmmLogLost.Count > 20)
        {
            mmmmLogLost.RemoveAt(0);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (nCurTIme >= 0)
        {
            if (UpdateManager.Instance.progress != 1 && UpdateManager.Instance.mFileLength != nLastLength)
            {
                nCurTIme = 0;
                nLastLength = UpdateManager.Instance.mFileLength;
            }
            nCurTIme += Time.deltaTime;
            if (nCurTIme >= 5.0f)
            {
                nCurTIme = -1;
                Log(" 超时");
                UpdateManager.Instance.Close("  超时");
            }
        }
                //Debug.Log(UpdateManager.Instance.progress);
    }

    private void OnDestroy()
    {
        UpdateManager.Instance.Close("  退出");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("+ Cube1"))
        {
            //GameObject cube = GameObject.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/A/Prefab/Cube.prefab"));
            cube1.Start("a/prefab/cube", "Assets/A/Prefab/Cube.prefab", (UnityEngine.Object pAsset) =>
            {
                cubeobj = (GameObject)GameObject.Instantiate(pAsset);
                cubeobj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            });
        }
        if (GUILayout.Button("+ Sphere1"))
        {
            //GameObject sphere = GameObject.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/A/Prefab/Sphere.prefab"));
            sphere1.Start("a/prefab/sphere", "Assets/A/Prefab/sphere.prefab", (UnityEngine.Object pAsset) =>
            {
                sphereobj = (GameObject)GameObject.Instantiate(pAsset);
                sphereobj.transform.position = new Vector3(0.0f, 2.0f, 0.0f);
            });
        }
        if (GUILayout.Button("- Cube1"))
        {
            cube1.End();
            GameObject.DestroyImmediate(cubeobj, true);
            cubeobj = null;
            Resources.UnloadUnusedAssets();
        }
        if (GUILayout.Button("- Sphere2"))
        {
            sphere1.End();
            GameObject.DestroyImmediate(sphereobj, true);
            sphereobj = null;
            Resources.UnloadUnusedAssets();
        }
        if (GUILayout.Button("GC"))
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        if (GUILayout.Button("更新测试"))
        {
            nCurTIme = 0;
            UpdateManager.Instance.CheckVersions();
        }
        if (GUILayout.Button("XXXXXXXX"))
        {
            UpdateManager.Instance.Close("  xxxxxxxx");
        }

        GUILayout.TextField("下载进度: " + UpdateManager.Instance.progress);
        GUILayout.TextField("解压进度: " + ZipUtility.unzipprogress);

        for (int i = 0; i < mmmmLogLost.Count; ++i)
        {
            GUILayout.TextField(mmmmLogLost[i]);
        }
    }
}
