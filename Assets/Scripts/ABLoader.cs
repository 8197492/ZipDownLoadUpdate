using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABLoader : MonoBehaviour
{
    public static ABLoader Instance = null;

    void Awake()
    {
        Instance = this;
    }

    // 协程实现
    IEnumerator LoadAsyncCoroutine(string path, string sName, Action<UnityEngine.Object> callback)
    {
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(path);
        yield return abcr;
        OnAssetBundleLoad(abcr.assetBundle, sName, callback);
    }

    // 开启协程
    void LoadAssetbundleAsync(string finalPath, string sName, Action<UnityEngine.Object> callback)
    {
        StartCoroutine(LoadAsyncCoroutine(finalPath, sName, callback));
    }

    // 从StreamingAssetsPath异步加载
    void LoadFromStreamingAssetsPathAsync(string assetbundle, string sName, Action<UnityEngine.Object> callback)
    {
        LoadAssetbundleAsync(Application.streamingAssetsPath + "/" + assetbundle, sName, callback);
    }

    // PersistantDataPath异步加载
    //public static AssetBundle LoadFromPersistantDataPathAsync(string assetbundle)
    //{
    //    return LoadAssetbundleAsync(Application.persistentDataPath + "/" + assetbundle);
    //}

    // 同步
    //public UnityEngine.Object Load(AssetBundle assetbundle, string assetName, Type resType)
    //{
    //    return assetbundle.LoadAsset(assetName, resType);
    //}

    // 一部
    IEnumerator LoadAssetAsyncCoroutine(AssetBundle ab, string name, Type resType, Action<UnityEngine.Object> callback)
    {
        AssetBundleRequest request = ab.LoadAssetAsync(name);

        // 等待加载完成
        while (!request.isDone)
        {
            yield return false;
        }
        ab.Unload(false);
        Resources.UnloadUnusedAssets();
        callback(request.asset);   // 回调上层
    }

    public void Load(string assetbundle, string sName, Action<UnityEngine.Object> callback)
    {
        LoadFromStreamingAssetsPathAsync(assetbundle, sName, callback);
    }

    void OnAssetBundleLoad(AssetBundle ab, string sName, Action<UnityEngine.Object> callback)
    {
        StartCoroutine(LoadAssetAsyncCoroutine(ab, sName, typeof(UnityEngine.Object), callback));
    }
}