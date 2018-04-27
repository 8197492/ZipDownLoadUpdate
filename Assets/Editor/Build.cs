using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System;

public class Build
{
    private static string[] Paths = {"A", "B"};
    private static int mModityNum = 0;
    [MenuItem("AssetBundles/UpdateABName")]
    static void UpdateABName()
    {
        mModityNum = 0;
        for (int i = 0; i < Paths.Length; ++i)
        {
            TraversalFile("Assets/" + Paths[i]);
        }
        Debug.Log("刷新AB名字 Done!!! 修改 " + mModityNum + " 文件!!!");
    }

    [MenuItem("Tools/BuildPackage")]
    static void BuildPackage()
    {
        Debug.Log(Application.dataPath.Replace("Assets", "AssetBundles"));
        //FileBuilder.Instance.MakePackage();
    }

    static void TraversalFile(string sPath)
    {
        if (!Directory.Exists(sPath))
        {
            return;
        }
        string[] files = Directory.GetFiles(sPath, "*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            if (file.Contains(".meta"))
            {
                continue;
            }
            
            SetABName(file);
        }
    }

    static string GetName(string sPath)
    {
        AssetImporter importer = AssetImporter.GetAtPath(sPath);
        if (importer == null)
        {
            return string.Empty;
        }

        string sName = sPath.Replace("Assets/", string.Empty);
        sName = sName.Replace("\\", "/");
        int fixIdxPos = sName.IndexOf('.');
        if (fixIdxPos != -1)
        {
            sName = sName.Substring(0, fixIdxPos);
        }

        return sName.ToLower();
    }

    static void SetABName(string sPath)
    {
        AssetImporter importer = AssetImporter.GetAtPath(sPath);
        if (importer != null)
        {
            string sName = GetName(sPath);
            if (string.Compare(importer.assetBundleName, sName) != 0)
            {
                importer.assetBundleName = sName;
                importer.SaveAndReimport();
                ++mModityNum;
            }
        }
    }

    public static string AssetBundlesOutputPath
    {
        get
        {
            return "Assets/StreamingAssets";
        }
    }

    [MenuItem("AssetBundles/BuildAB")]
    static void BuildAB()
    {
        string outputPath = AssetBundlesOutputPath;
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
    }
}
