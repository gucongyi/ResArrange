using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ArtResArrange : Editor
{
    const string fileAllArt = "Assets/../fileAllArt.txt";
    const string fileUsedArt = "Assets/../fileUsedArt.txt";
    const string fileNoUsedArt = "Assets/../fileNoUsedArt.txt";
    public static void ShowProgress(int total, int cur,string processName)
    {
        float val = 0;
        if (total>0)
        {
            val = (float)cur / (float)total;
        }
        EditorUtility.DisplayProgressBar("Processing", processName, val);
    }

    static List<string> idsPrefabsAndUnity = new List<string>();
    static List<string> allResDependencies = new List<string>();
    /// <summary>
    /// 查找art_resource下所有引用文件
    /// </summary>
    [MenuItem("PackageTools/ResArrange/Find All Art Used Files", false, 0)]
    public static void FindObjectDependencies()
    {
        ShowProgress(0, 0,"开始处理");
        if (File.Exists(fileUsedArt) == false)
        {
            File.Create(fileUsedArt).Close();
        }
        string[] idsPrefab = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/dgame/AddressableRes" });
        string[] idsScene = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets/dgame/scene" });
        idsPrefabsAndUnity.Clear();
        allResDependencies.Clear();
        idsPrefabsAndUnity.AddRange(idsPrefab);
        idsPrefabsAndUnity.AddRange(idsScene);
        int count = 0;
        if (idsPrefabsAndUnity.Count>0)
        {
            foreach (var eachId in idsPrefabsAndUnity)
            {
                string resPath = AssetDatabase.GUIDToAssetPath(eachId);
                count++;
                ShowProgress(idsPrefabsAndUnity.Count, count, resPath);
                string[] names = AssetDatabase.GetDependencies(new string[] { resPath });  //依赖的东东
                for (int i = 0; i < names.Length; i++)
                {
                    if (allResDependencies.Contains(names[i]) ==false)
                    {
                        if (names[i].Contains("Assets/dgame/art_resource"))
                        {
                            allResDependencies.Add(names[i]);
                        }
                    }
                }
            }
        }

        if (allResDependencies.Count > 0)
        {
            File.WriteAllLines(fileUsedArt, allResDependencies);
            Debug.LogError($"写入文件{fileUsedArt}成功！");
        }

        EditorUtility.ClearProgressBar();
    }

    static List<string> allResArt = new List<string>();
    /// <summary>
    /// 查找art_resource下所有文件
    /// </summary>
    [MenuItem("PackageTools/ResArrange/Get All Art Files", false, 0)]
    public static void GetAllArtFiles()
    {
        ShowProgress(0, 0, "开始处理");
        if (File.Exists(fileAllArt) == false)
        {
            File.Create(fileAllArt).Close();
        }
        int count = 0;
        string directoryPath = "Assets/dgame/art_resource";
        allResArt.Clear();
        string[] files=Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta")==false)
            {
                files[i]=files[i].Replace("\\","/");
                allResArt.Add(files[i]);
                count++;
                ShowProgress(idsPrefabsAndUnity.Count, count, files[i]);
            }
        }
        
        if (allResArt.Count > 0)
        {
            File.WriteAllLines(fileAllArt, allResArt);
            Debug.LogError($"写入文件{fileAllArt}成功！");
        }

        EditorUtility.ClearProgressBar();
    }

    static List<string> ResArtNoUsed = new List<string>();
    /// <summary>
    /// 查找art_resource下所有没有引用文件
    /// </summary>
    [MenuItem("PackageTools/ResArrange/Get All Art No Used Files", false, 0)]
    public static void GetAllArtNoUsedFiles()
    {
        if (File.Exists(fileNoUsedArt) == false)
        {
            File.Create(fileNoUsedArt).Close();
        }

        allResArt.Clear();
        allResDependencies.Clear();
        ResArtNoUsed.Clear();
        allResDependencies.AddRange(File.ReadAllLines(fileUsedArt));
        allResArt.AddRange(File.ReadAllLines(fileAllArt));

        for (int i = 0; i < allResArt.Count; i++)
        {
            if (allResDependencies.Contains(allResArt[i]) == false)
            {
                ResArtNoUsed.Add(allResArt[i]);
            }
        }
        if (ResArtNoUsed.Count > 0)
        {
            File.WriteAllLines(fileNoUsedArt, ResArtNoUsed);
            Debug.LogError($"写入文件{fileNoUsedArt}成功！");
        }
    }

    /// <summary>
    /// 删除art_resource下所有没有引用的文件
    /// </summary>
    [MenuItem("PackageTools/ResArrange/Delete All Art No Used Files", false, 0)]
    public static void DeleteAllArtNoUsedFiles()
    {
        ShowProgress(0, 0, "开始处理");
        ResArtNoUsed.Clear();
        ResArtNoUsed.AddRange(File.ReadAllLines(fileNoUsedArt));
        int count = 0;
        for (int i = 0; i < ResArtNoUsed.Count; i++)
        {
            try
            {
                count++;
                ShowProgress(ResArtNoUsed.Count, count, $"删除{ResArtNoUsed[i]}");
                if (File.Exists(ResArtNoUsed[i]))
                {
                    File.Delete(ResArtNoUsed[i]);
                }
                if (File.Exists($"{ResArtNoUsed[i]}.meta"))
                {
                    File.Delete($"{ResArtNoUsed[i]}.meta");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"删除{ResArtNoUsed[i]}文件异常：{e.Message}");
            }
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        Debug.LogError($"删除art_resource下无效文件完成");
    }


    /// <summary>
    /// 删除art_resource/PostLogin下所有文件，只保留对应的文件夹结构
    /// </summary>
    [MenuItem("PackageTools/ResArrange/Delete Art PostLogin Files", false, 0)]
    public static void DeleteAllArtPostLoginFiles()
    {
        ShowProgress(0, 0, "开始处理");
        //收集
        string directoryPath = "Assets/dgame/art_resource/PostLogin";
        List<string> allResArtPostLogin = new List<string>();
        string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta") == false)
            {
                files[i] = files[i].Replace("\\", "/");
                allResArtPostLogin.Add(files[i]);
            }
        }
        //删除
        int count = 0;
        for (int i = 0; i < allResArtPostLogin.Count; i++)
        {
            try
            {
                count++;
                ShowProgress(allResArtPostLogin.Count, count, $"删除{allResArtPostLogin[i]}");
                if (File.Exists(allResArtPostLogin[i]))
                {
                    File.Delete(allResArtPostLogin[i]);
                }
                if (File.Exists($"{allResArtPostLogin[i]}.meta"))
                {
                    File.Delete($"{allResArtPostLogin[i]}.meta");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"删除{allResArtPostLogin[i]}文件异常：{e.Message}");
            }
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        Debug.LogError($"删除art_resource/PostLogin下所有文件完成");
    }

}