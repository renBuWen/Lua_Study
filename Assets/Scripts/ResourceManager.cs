﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 加载预置对像，替代resource.load.
/// </summary>
public class ResourceManager
{
    static ResourceManager instance = null;

    AssetBundleManifest manifest = null;

    public bool bLoadFromStream = true;

    private string CurPlatform = "/Android/";

    string strManifest = "Android";

    Dictionary<string, GameObject> Dic_GameObjectRes = new Dictionary<string, GameObject>();  //原始资源文件
    Dictionary<string, TextAsset> Dic_TextAssetRes = new Dictionary<string, TextAsset>();  //原始资源文件
    Dictionary<string, Sprite> Dic_SpriteRendererRes = new Dictionary<string, Sprite>();  //原始资源文件
    Dictionary<string, AssetBundle> Dic_SceneRes = new Dictionary<string, AssetBundle>();  //场景资源文件
    Dictionary<string, int> Dic_DependsRes = new Dictionary<string, int>();    //依赖文件列表

    static public ResourceManager GetInstance()
    {
        if (instance == null)
        {
            instance = new ResourceManager();
        }
        return instance;
    }

    /// <summary>
    /// 根据平台判断初始化平台名
    /// </summary>
    private ResourceManager()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            CurPlatform = "/Win/";
            strManifest = "Win";
        }
    }
    /// <summary>
    /// 得到当前平台文件绝对路径
    /// </summary>
    /// <returns></returns>
    public string GetResPath()
    {
        return Application.streamingAssetsPath;
    }
    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    public TextAsset LoadtextAsset(string strPath)
    {
        if (bLoadFromStream)
        {
            if (!Dic_TextAssetRes.ContainsKey(strPath))
            {
                //获取依赖文件列表;
                string[] cubedepends = manifest.GetAllDependencies(strPath + ".data");

                //Debug.Log(cubedepends.Length);
                //foreach (var item in cubedepends)
                //{
                //    Debug.Log("depends " + item);
                //}
                AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];

                for (int index = 0; index < cubedepends.Length; index++)
                {
                    //加载所有的依赖文件;
                    if (!Dic_DependsRes.ContainsKey(cubedepends[index]))
                    {
                        dependsAssetbundle[index] = AssetBundle.LoadFromFile(GetResPath() + CurPlatform + cubedepends[index]);
                        Dic_DependsRes.Add(cubedepends[index], 1);
                    }
                }
                string[] strs = strPath.Split('/');
                string strName = strs[strs.Length - 1];
                string strResource = CurPlatform + strPath + ".data";
                AssetBundle cubeBundle = null;
                cubeBundle = AssetBundle.LoadFromFile(ResourceManager.GetInstance().GetResPath() + strResource);
                if (cubeBundle == null)
                    return null;
                TextAsset cube = cubeBundle.LoadAsset(strName, typeof(TextAsset)) as TextAsset;
                Dic_TextAssetRes.Add(strPath, cube);
                return cube;
            }
            else
            {
                return Dic_TextAssetRes[strPath];
            }
        }
        else
        {
            TextAsset cube = Resources.Load(strPath, typeof(TextAsset)) as TextAsset;
            return cube;
        }
    }
    /// <summary>
    /// 读取预置对象
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    public GameObject Load(string strPath)
    {
        if (bLoadFromStream)
        {
            if (!Dic_GameObjectRes.ContainsKey(strPath))
            {
                //获取依赖文件列表;
                string[] cubedepends = manifest.GetAllDependencies(strPath + ".data");
                //Debug.Log(cubedepends.Length);
                //foreach (var item in cubedepends)
                //{
                //    Debug.Log("depends " + item);
                //}
                AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];
                for (int index = 0; index < cubedepends.Length; index++)
                {
                    //加载所有的依赖文件;
                    if (!Dic_DependsRes.ContainsKey(cubedepends[index]))
                    {
                        dependsAssetbundle[index] = AssetBundle.LoadFromFile(GetResPath() + CurPlatform + cubedepends[index]);
                        Dic_DependsRes.Add(cubedepends[index], 1);
                    }
                }
                string[] strs = strPath.Split('/');
                string strName = strs[strs.Length - 1];
                string strResource = CurPlatform + strPath + ".data";
                AssetBundle cubeBundle = AssetBundle.LoadFromFile(GetResPath() + strResource);
                if (cubeBundle == null)
                    return null;
                GameObject cube = cubeBundle.LoadAsset(strName) as GameObject;
                Dic_GameObjectRes.Add(strPath, cube);
                return cube;
            }
            else
            {
                return Dic_GameObjectRes[strPath];
            }

        }
        else
        {
            GameObject cube = Resources.Load(strPath) as GameObject;
            return cube;
        }
    }

    /// <summary>
    /// 读取Lua AB文件
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    public AssetBundle LoadAssetBundle(string strPath)
    {
        string[] strs = strPath.Split('/');
        string strName = strs[strs.Length - 1];
        string strResource = CurPlatform + strPath + ".data";

        AssetBundle cubeBundle = AssetBundle.LoadFromFile(GetResPath() + strResource);
        return cubeBundle;
    }

    /// <summary>
    /// 读取图片 AB文件
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    public Sprite LoadSprite(string strPath)
    {
        if (bLoadFromStream)
        {
            if (!Dic_SpriteRendererRes.ContainsKey(strPath))
            {

                //获取依赖文件列表;
                string[] cubedepends = manifest.GetAllDependencies(strPath + ".data");

                //Debug.Log(cubedepends.Length);
                //foreach (var item in cubedepends)
                //{
                //    Debug.Log("depends " + item);
                //}
                AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];

                for (int index = 0; index < cubedepends.Length; index++)
                {
                    //加载所有的依赖文件;
                    if (!Dic_DependsRes.ContainsKey(cubedepends[index]))
                    {
                        dependsAssetbundle[index] = AssetBundle.LoadFromFile(GetResPath() + CurPlatform + cubedepends[index]);
                        Dic_DependsRes.Add(cubedepends[index], 1);
                    }
                }

                string[] strs = strPath.Split('/');
                string strName = strs[strs.Length - 1];
                string strResource = CurPlatform + strPath + ".data";
                AssetBundle cubeBundle = null;
                cubeBundle = AssetBundle.LoadFromFile(GetResPath() + strResource);
                if (cubeBundle == null)
                    return null;
                Sprite cube = cubeBundle.LoadAsset(strName, typeof(Sprite)) as Sprite;
                Dic_SpriteRendererRes.Add(strPath, cube);
                Debug.Log("AssetBundle添加文件:" + strResource);
                return cube;
            }
            else
            {
                return Dic_SpriteRendererRes[strPath];
            }
        }
        else
        {
            Sprite cube = Resources.Load(strPath, typeof(Sprite)) as Sprite;
            return cube;
        }
    }

    /// <summary>
    /// 初始化配置文件Manifest依赖资源
    /// </summary>
    public void Init()
    {
        if (bLoadFromStream)
        {
            //首先加载Manifest文件;
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(GetResPath() + CurPlatform + strManifest);
            //Debug.Log(manifestBundle == null);
            if (manifestBundle != null)
            {
                manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
            }
        }
    }

    /// <summary>
    /// 读取场景
    /// </summary>
    /// <param name="strSceneName"></param>
    /// <returns></returns>
    public IEnumerator LoadScene(string strSceneName)
    {
        AssetBundleCreateRequest request = null;
        if (bLoadFromStream)
        {
            if (!Dic_SceneRes.ContainsKey(strSceneName))
            {
                string strPath = "scene/" + strSceneName;
                string[] cubedepends = manifest.GetAllDependencies(strPath + ".unity3d");
                AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];

                for (int index = 0; index < cubedepends.Length; index++)
                {
                    //加载所有的依赖文件;
                    if (!Dic_DependsRes.ContainsKey(cubedepends[index]))
                    {
                        dependsAssetbundle[index] = AssetBundle.LoadFromFile(GetResPath() + CurPlatform + cubedepends[index]);
                        Dic_DependsRes.Add(cubedepends[index], 1);
                    }
                }

                request = AssetBundle.LoadFromFileAsync(GetResPath() + CurPlatform + "/scene/" + strSceneName + ".unity3d");
                yield return request;
                var bundle = request.assetBundle;
                SceneManager.LoadScene(strSceneName);
                Dic_SceneRes.Add(strSceneName, bundle);
            }
            else
            {
                SceneManager.LoadScene(strSceneName);
            }
            Debug.Log("切换场景成功!!!!");
        }
        /*else
        {
            //request = AssetBundle.LoadFromFileAsync(Application.dataPath + "/Resources/" + "Scene/" + strSceneName+".unity");
            // SceneManager.LoadScene("/Resources/" + "Scene/" + strSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        // request = AssetBundle.LoadFromFileAsync(Application.dataPath + "/Scene/" + strSceneName + ".unity");
        //WWW download = WWW.LoadFromCacheOrDownload("file://" + Application.streamingAssetsPath + "/scene/"+ strSceneName + ".unity3d", 1);
        //yield return request;
        //var bundle = request.assetBundle;
        //SceneManager.LoadScene(strSceneName);
        //Debug.Log("切换场景成功!!!!");*/
    }
}
