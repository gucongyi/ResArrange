using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ResBasePackageCollect : Editor
{
    const string fileResBasePackageName = "Assets/../fileResBasePackage";
    const string filePostFix = ".txt";
    const string fileResBasePackageCombine = "Assets/../fileResBasePackageCombine.txt";
    static List<string> listPath = new List<string>();
    const int filePreserveCount = 20;
    /// <summary>
    /// 合并所有跑出来的路径
    /// </summary>
    [MenuItem("PackageTools/ResCollect/CombineRes", false, 0)]
    public static void CombineResBasePackage()
    {
        ArtResArrange.ShowProgress(0, 0,"开始处理");
        if (File.Exists(fileResBasePackageCombine) == false)
        {
            File.Create(fileResBasePackageCombine).Close();
        }
        listPath.Clear();
        //20个文件，可以合并20个文件
        for (int i = 1; i < filePreserveCount+1; i++)
        {
            string fileName = $"{fileResBasePackageName}{i}{filePostFix}";
            if (File.Exists(fileName))
            {
                string[] pathArray=File.ReadAllLines(fileName);
                for (int j = 0; j < pathArray.Length; j++)
                {
                    if (listPath.Contains(pathArray[j]) == false)
                    {
                        listPath.Add(pathArray[j]);
                    }
                }
            }
        }
        File.WriteAllLines(fileResBasePackageCombine, listPath);
        Debug.LogError($"写入文件{fileResBasePackageCombine}成功！");
        EditorUtility.ClearProgressBar();
    }

    const string fileResBasePackageCombineSupply = "Assets/../fileResBasePackageCombineSupply.txt";
    /// <summary>
    /// 查找art_resource,AddressableRes,scene下CombineRes文件中的所有文件的全路径
    /// </summary>
    [MenuItem("PackageTools/ResCollect/Supply Full Path To Combine", false, 0)]
    public static void SupplyFullPathToCombine()
    {
        ArtResArrange.ShowProgress(0, 0, "开始处理");
        if (File.Exists(fileResBasePackageCombineSupply) == false)
        {
            File.Create(fileResBasePackageCombineSupply).Close();
        }
        int count = 0;
        string directoryPath_art_resource = "Assets/dgame/art_resource";
        string directoryPath_AddressableRes = "Assets/dgame/AddressableRes";
        string directoryPath_ab_scene = "Assets/dgame/scene/ab_scene";


        List<string> listOrginCombine = new List<string>();
        listOrginCombine.AddRange(File.ReadAllLines(fileResBasePackageCombine));
        string[] files_art_resource = Directory.GetFiles(directoryPath_art_resource, "*", SearchOption.AllDirectories);
        string[] files_AddressableRes = Directory.GetFiles(directoryPath_AddressableRes, "*", SearchOption.AllDirectories);
        string[] files_ab_scene = Directory.GetFiles(directoryPath_ab_scene, "*", SearchOption.AllDirectories);
        List<string> files = new List<string>();//所有文件
        files.AddRange(files_art_resource);
        files.AddRange(files_AddressableRes);
        files.AddRange(files_ab_scene);
        //遍历全文件路径补全太慢了，使用字典方式，用文件名做key,字典一对多
        Dictionary<string, List<string>> dicAllFile = new Dictionary<string, List<string>>();
        for (int i = 0; i < files.Count; i++)
        {
            if (files[i].EndsWith(".meta") == false)
            {
                files[i] = files[i].Replace("\\", "/");
                int stripIndex = files[i].LastIndexOf('/');
                if (stripIndex >= 0)
                {
                    string key = files[i].Substring(stripIndex + 1).Trim();
                    List<string> dicValues = null;
                    if (dicAllFile.TryGetValue(key, out dicValues))
                    {
                        if (dicValues.Contains(files[i]) == false)
                        {
                            dicValues.Add(files[i]);
                        }
                    }
                    else
                    {
                        dicValues=new List<string>();
                        dicValues.Add(files[i]);
                        dicAllFile.Add(key,dicValues);
                    }
                }
            }
        }

        List<string> listSupplyFile = new List<string>();
        foreach (var eachOriginPath in listOrginCombine)
        {
            string willProcessKey = eachOriginPath;
            if (eachOriginPath.Contains("/"))
            {
                int stripIndex = eachOriginPath.LastIndexOf('/');
                if (stripIndex >= 0)
                {
                    willProcessKey = eachOriginPath.Substring(stripIndex + 1).Trim();
                }
            }
            List<string> dicValues = null;
            if (dicAllFile.TryGetValue(willProcessKey, out dicValues))
            {
                for (int i=0;i<dicValues.Count;i++)
                {
                    if (listSupplyFile.Contains(dicValues[i]) == false)
                    {
                        listSupplyFile.Add(dicValues[i]);
                    }
                }
            }
            count++;
            ArtResArrange.ShowProgress(listOrginCombine.Count, count, eachOriginPath);
        }
        

        if (listSupplyFile.Count > 0)
        {
            File.WriteAllLines(fileResBasePackageCombineSupply, listSupplyFile);
            Debug.LogError($"写入文件{fileResBasePackageCombineSupply}成功！");
        }
        EditorUtility.ClearProgressBar();
    }



    const string fileResCollectAllDependenciesPre = "Assets/../fileResCollectAllDependenciesPreLogin.txt";
    const string fileResCollectAllDependenciesPost = "Assets/../fileResCollectAllDependenciesPostLogin.txt";
    static List<string> allResDependenciesPreLogin = new List<string>();
    //这一部分是放错了的，需要矫正
    static List<string> allResDependenciesPostLogin = new List<string>();
    /// <summary>
    /// 查找art_resource,AddressableRes,scene文件夹下所有引用文件
    /// </summary>
    [MenuItem("PackageTools/ResCollect/Find All Dependencies", false, 0)]
    public static void FindAllFileDependencies()
    {
        ArtResArrange.ShowProgress(0, 0, "开始处理");
        if (File.Exists(fileResCollectAllDependenciesPre) == false)
        {
            File.Create(fileResCollectAllDependenciesPre).Close();
        }
        if (File.Exists(fileResCollectAllDependenciesPost) == false)
        {
            File.Create(fileResCollectAllDependenciesPost).Close();
        }
        int count = 0;
        allResDependenciesPreLogin.Clear();
        allResDependenciesPostLogin.Clear();
        string[] fileBasePackageCombine=File.ReadAllLines(fileResBasePackageCombineSupply);
        if (fileBasePackageCombine.Length > 0)
        {
            foreach (var resPath in fileBasePackageCombine)
            {
                count++;
                ArtResArrange.ShowProgress(fileBasePackageCombine.Length, count, resPath);
                string[] names = AssetDatabase.GetDependencies(new string[] { resPath });  //依赖的东东,有的没有包含自身，比如场景，文件类的是包含了自身
                bool isContainSelf = false;
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == resPath)
                    {
                        isContainSelf = true;
                        break;
                    }
                }
                if (isContainSelf == false)
                {
                    //数组扩容，扩容后自身Length会变化
                    Array.Resize(ref names, names.Length + 1);
                    //把本身加进来
                    names[names.Length-1] = resPath;
                }
                
                for (int i = 0; i < names.Length; i++)
                {
                    //PreLogin
                    if (allResDependenciesPreLogin.Contains(names[i]) == false)
                    {
                        if (names[i].Contains("Assets/dgame/art_resource/PreLogin")
                            || names[i].Contains("Assets/dgame/AddressableRes/PreLogin")
                            || names[i].Contains("Assets/dgame/scene/ab_scene/PreLogin"))
                        {
                            allResDependenciesPreLogin.Add(names[i]);
                        }
                    }
                    //PostLogin
                    if (allResDependenciesPostLogin.Contains(names[i]) == false)
                    {
                        if (names[i].Contains("Assets/dgame/art_resource/PostLogin")
                            || names[i].Contains("Assets/dgame/AddressableRes/PostLogin")
                            || names[i].Contains("Assets/dgame/scene/ab_scene/PostLogin"))
                        {
                            allResDependenciesPostLogin.Add(names[i]);
                        }
                    }
                }
            }
        }

        if (allResDependenciesPreLogin.Count > 0)
        {
            File.WriteAllLines(fileResCollectAllDependenciesPre, allResDependenciesPreLogin);
            Debug.LogError($"写入文件{fileResCollectAllDependenciesPre}成功！");
        }

        if (allResDependenciesPostLogin.Count > 0)
        {
            File.WriteAllLines(fileResCollectAllDependenciesPost, allResDependenciesPostLogin);
            Debug.LogError($"写入文件{fileResCollectAllDependenciesPost}成功！");
        }

        EditorUtility.ClearProgressBar();
    }


    const string fileResCollectAllPreLogin = "Assets/../fileResCollectAllPreLogin.txt";
    static List<string> allResPreLogin = new List<string>();
    /// <summary>
    /// 查找art_resource,AddressableRes,scene下PreLogin中所有文件，PostLogin不用管
    /// </summary>
    [MenuItem("PackageTools/ResCollect/Get All PreLogin Files", false, 0)]
    public static void GetAllPreLoginFiles()
    {
        ArtResArrange.ShowProgress(0, 0, "开始处理");
        if (File.Exists(fileResCollectAllPreLogin) == false)
        {
            File.Create(fileResCollectAllPreLogin).Close();
        }
        int count = 0;
        string directoryPath_art_resource = "Assets/dgame/art_resource/PreLogin";
        string directoryPath_AddressableRes = "Assets/dgame/AddressableRes/PreLogin";
        string directoryPath_ab_scene = "Assets/dgame/scene/ab_scene/PreLogin";


        allResPreLogin.Clear();
        string[] files_art_resource = Directory.GetFiles(directoryPath_art_resource, "*", SearchOption.AllDirectories);
        string[] files_AddressableRes = Directory.GetFiles(directoryPath_AddressableRes, "*", SearchOption.AllDirectories);
        string[] files_ab_scene = Directory.GetFiles(directoryPath_ab_scene, "*", SearchOption.AllDirectories);
        List<string> files = new List<string>();
        files.AddRange(files_art_resource);
        files.AddRange(files_AddressableRes);
        files.AddRange(files_ab_scene);

        for (int i = 0; i < files.Count; i++)
        {
            if (files[i].EndsWith(".meta") == false)
            {
                files[i] = files[i].Replace("\\", "/");
                allResPreLogin.Add(files[i]);
                count++;
                ArtResArrange.ShowProgress(files.Count/2, count, files[i]);
            }
        }

        if (allResPreLogin.Count > 0)
        {
            File.WriteAllLines(fileResCollectAllPreLogin, allResPreLogin);
            Debug.LogError($"写入文件{fileResCollectAllPreLogin}成功！");
        }
        EditorUtility.ClearProgressBar();
    }


    const string filesResNeedMoveToPostLogin = "Assets/../filesResNeedMoveToPostLogin.txt";
    static List<string> ResNeedMoveToPostLoginFiles = new List<string>();
    /// <summary>
    /// 查找需要移到PostLogin中的下所有没有引用文件
    /// </summary>
    [MenuItem("PackageTools/ResCollect/Get Need Move TO PostLogin Files", false, 0)]
    public static void GetAllArtNoUsedFiles()
    {
        if (File.Exists(filesResNeedMoveToPostLogin) == false)
        {
            File.Create(filesResNeedMoveToPostLogin).Close();
        }

        allResPreLogin.Clear();
        allResDependenciesPreLogin.Clear();
        ResNeedMoveToPostLoginFiles.Clear();
        allResDependenciesPreLogin.AddRange(File.ReadAllLines(fileResCollectAllDependenciesPre));
        allResPreLogin.AddRange(File.ReadAllLines(fileResCollectAllPreLogin));

        for (int i = 0; i < allResPreLogin.Count; i++)
        {
            if (allResDependenciesPreLogin.Contains(allResPreLogin[i]) == false)
            {
                ResNeedMoveToPostLoginFiles.Add(allResPreLogin[i]);
            }
        }
        if (ResNeedMoveToPostLoginFiles.Count > 0)
        {
            File.WriteAllLines(filesResNeedMoveToPostLogin, ResNeedMoveToPostLoginFiles);
            Debug.LogError($"写入文件{filesResNeedMoveToPostLogin}成功！");
        }
    }

    /// <summary>
    /// 之前放错到PostLogin的,有引用的,需要移动到PreLogin中
    /// 移动PreLogin中没有引用的资源到PostLogin文件夹中
    /// 需要将meta文件一并移动
    /// </summary>
    [MenuItem("PackageTools/ResCollect/Move PreLogin UnRef To PostLogin", false, 0)]
    public static void MovePreLoginUnRefToPostLoginFiles()
    {
        ArtResArrange.ShowProgress(0, 0, "开始处理");
        //之前放错到PostLogin的,有引用的,需要移动到PreLogin中
        allResDependenciesPostLogin.Clear();
        allResDependenciesPostLogin.AddRange(File.ReadAllLines(fileResCollectAllDependenciesPost));
        //PreLogin中没有引用的，放到PostLogin中
        ResNeedMoveToPostLoginFiles.Clear();
        ResNeedMoveToPostLoginFiles.AddRange(File.ReadAllLines(filesResNeedMoveToPostLogin));
        int count = 0;
        int totalCount = allResDependenciesPostLogin.Count + ResNeedMoveToPostLoginFiles.Count;
        for (int i = 0; i < allResDependenciesPostLogin.Count; i++)
        {
            string oldPath = allResDependenciesPostLogin[i];
            string newPath = allResDependenciesPostLogin[i].Replace("/PostLogin/", "/PreLogin/");
            MoveFile(oldPath, newPath);
            count++;
            ArtResArrange.ShowProgress(totalCount, count, allResDependenciesPostLogin[i]);
        }

        for (int i = 0; i < ResNeedMoveToPostLoginFiles.Count; i++)
        {
            string oldPath = ResNeedMoveToPostLoginFiles[i];
            string newPath = ResNeedMoveToPostLoginFiles[i].Replace("/PreLogin/", "/PostLogin/");
            MoveFile(oldPath, newPath);
            count++;
            ArtResArrange.ShowProgress(totalCount, count, ResNeedMoveToPostLoginFiles[i]);
        }

        Debug.LogError($"移动所有文件完成！");
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
    }


    const string fileEmptyFolders = "Assets/../fileEmptyFolders.txt";
    /// <summary>
    /// 遍历PreLogin下的所有空文件夹，方便删除,自己决定删除还是不删除，因为有很多是资源分组用的，不能删除
    /// </summary>
    [MenuItem("PackageTools/ResCollect/PreLogin Empty Folder Collect", false, 0)]
    public static void PreLoginEmptyFolderCollect()
    {
        if (File.Exists(fileEmptyFolders) == false)
        {
            File.Create(fileEmptyFolders).Close();
        }
        string directoryPath_art_resource = "Assets/dgame/art_resource/PreLogin";
        string directoryPath_AddressableRes = "Assets/dgame/AddressableRes/PreLogin";
        string directoryPath_ab_scene = "Assets/dgame/scene/ab_scene/PreLogin";
        string[] dirs_art_resource = Directory.GetDirectories(directoryPath_art_resource, "*", SearchOption.AllDirectories);
        string[] dirs_AddressableRes = Directory.GetDirectories(directoryPath_AddressableRes, "*", SearchOption.AllDirectories);
        string[] dirs_ab_scene = Directory.GetDirectories(directoryPath_ab_scene, "*", SearchOption.AllDirectories);
        List<string> allDir = new List<string>();
        allDir.AddRange(dirs_art_resource);
        allDir.AddRange(dirs_AddressableRes);
        allDir.AddRange(dirs_ab_scene);

        List<string> dirEmptyList = new List<string>();
        for (int i = 0; i < allDir.Count; i++)
        {
            var info = new DirectoryInfo(allDir[i]);
            if (info.GetFileSystemInfos().Length==0)
            {
                dirEmptyList.Add(allDir[i]);
            }
        }

        if (dirEmptyList.Count > 0)
        {
            File.WriteAllLines(fileEmptyFolders, dirEmptyList);
            Debug.LogError($"写入文件{fileEmptyFolders}成功！");
        }
    }

    /// <summary>
    /// 移动Excel文件到指定位置
    /// </summary>
    /// <param name="oldPath">原文件位置+文件名(例如E:\TEST\1.txt)</param>
    /// <param name="newPath">移动文件位置+文件名(例如E:\TEST\File\1.txt)</param>
    /// <param name="path">移动文件夹位置(例如E:\TEST\File)</param>>
    /// 需要将meta文件一并移动
    public static void MoveFile(string oldPath, string newPath)
    {
           
        try
        {
            string pathDir = String.Empty;
            int stripIndex = newPath.LastIndexOf('/');
            if (stripIndex >= 0)
            {
                pathDir = newPath.Substring(0, stripIndex).Trim();
            }
             
            //判断移动的文件夹是否存在,不存在则创建
            if (Directory.Exists(pathDir))
            {
                //先删除再移动
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
                if (File.Exists($"{newPath}.meta"))
                {
                    File.Delete($"{newPath}.meta");
                }
            }
            else
            {
                Directory.CreateDirectory(pathDir);
            }
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
            }
            if (File.Exists($"{oldPath}.meta"))
            {
                File.Move($"{oldPath}.meta", $"{newPath}.meta");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"{oldPath}文件移动失败:{ex.ToString()}");
        }
    }



}