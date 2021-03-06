﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 版本检查，用于检查本地文件和服务器文件的CRC
/// </summary>
public class Updator : MonoBehaviour
{
    private static Dictionary<string, string> strLocalDicFiles = new Dictionary<string, string>();   //本地CRC校验码表
    private static Dictionary<string, string> strSeverDicFiles = new Dictionary<string, string>();   //服务器CRC校验码表
    private static List<string> DiffFiles = new List<string>();    //有差异的文件

    private static List<string> BaseResFiles = new List<string>();  //基础游戏资源

    //private string SeverPath = "file://D:/FTPUpdate";
    private string SeverPath = "http://192.168.1.7/OutPut/";

    private bool bUpDateRes = true;    //是否检查更新(无外网测试时使用)

    private string strLocalVarsion = "";
    private string strServerVarsion = "";

    private bool bLoadEnd = false;

    //private static bool bDownloadFile = false;

    private bool bCompareVarsion = false;

    private bool bUpdaterSuc = false;

    private static int iLoadingFileNum = 0;

    private bool UpdateVersion = false;

    private bool bCopyBaseFiles = false;
    private int iCopyBaseFileNum = 0;

    private bool bLinkServer = false;

    private Text LogText = null;

    private bool bOnCopyBaseFile = false;

    void Start()
    {
        var dec = ResourceManager.GetInstance().GetResPath();
        if (ResourceManager.GetInstance().bLoadFromStream)
        {
            Debug.Log("开始检查资源");
            //CopyLoaclRes();
        }
        else
        {
            Debug.Log("lua引擎初始化");
            LuaManager.GetInstance().Init();
            this.enabled = false;
        }
    }

    void Update()
    {
        if (!bCopyBaseFiles)
        {
            if (bUpDateRes)
            {
                if (strSeverDicFiles.Count != 0 && strLocalDicFiles.Count != 0)
                {
                    if (!bCompareVarsion)
                    {
                        if (!bLoadEnd && strLocalVarsion != "" && strServerVarsion != "")
                        {
                            Debug.Log("strLocalVarsion:" + strLocalVarsion);
                            Debug.Log("strServerVarsion:" + strServerVarsion);
                            bLoadEnd = true;
                        }
                        else if (bLoadEnd)
                        {
                            //比较文件版本及CRC码
                            CompareCRC();
                            bCompareVarsion = true;
                            iLoadingFileNum = 0;
                        }
                    }
                    else
                    {
                        DownloadDiffFile();
                        if (DiffFiles.Count == 0 && iLoadingFileNum == 0)
                        {
                            bUpdaterSuc = true;
                        }
                    }
                    //更新资源完成后最后刷新版本文件
                    if (bUpdaterSuc && iLoadingFileNum == 0 && !UpdateVersion)
                    {
                        UpdateVersion = true;
                        //更新版本文件和索引文件
                        string strPath = "/Android/";
                        string strRootFile = "Android";
                        string strRootmainfast = "Android.manifest";
                        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                        {
                            strPath = "/Win/";
                            strRootFile = "Win";
                            strRootmainfast = "Win.manifest";
                        }

                        string strSarvarAndroidPath = SeverPath + strPath + strRootFile;
                        Debug.Log("开始下载:" + strSarvarAndroidPath);
                        iLoadingFileNum++;
                        StartCoroutine(DownloadAndSave(strSarvarAndroidPath, strRootFile));

                        string strSarvarMainfastPath = SeverPath + strPath + strRootmainfast;
                        Debug.Log("开始下载:" + strSarvarMainfastPath);
                        iLoadingFileNum++;
                        StartCoroutine(DownloadAndSave(strSarvarMainfastPath, strRootmainfast));

                        string strVersion = "Varsion.txt";
                        string strVersionPath = SeverPath + strPath + strVersion;
                        Debug.Log("开始下载:" + strVersionPath);
                        iLoadingFileNum++;
                        StartCoroutine(DownloadAndSave(strVersionPath, strVersion));
                    }

                    if (UpdateVersion && iLoadingFileNum == 0)
                    {
                        Debug.Log("更新完成!!");
                        Debug.Log("加载游戏资源");
                        ResourceManager.GetInstance().Init();
                        Debug.Log("lua引擎初始化");
                        LuaManager.GetInstance().Init();
                        enabled = false;
                    }
                }
                else if (!bLinkServer)
                {
                    Debug.Log("检查资源更新!");
                    LoadLocalCRCDic();
                    StartCoroutine(DownLoadSeverCRCDic());
                    bLinkServer = true;
                }
            }
            else
            {
                Debug.Log("更新完成!");
                this.enabled = false;
            }
        }
    }

    /// <summary>
    /// 根据平台设置路径
    /// </summary>
    void LoadLocalCRCDic()
    {
        string strPath = "/Android/";
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            strPath = "/Win/";
        }
        string sPath = ResourceManager.GetInstance().GetResPath() + strPath + "Varsion.txt";
        var strLocalCRC = File.ReadAllText(sPath);
        LoadLocalCRCDic(strLocalCRC);
    }

    /// <summary>
    /// 读取CRC配置表
    /// </summary>
    void LoadLocalCRCDic(string filetext)
    {
        strLocalDicFiles.Clear();
        if (filetext != null)
        {
            string[] Lines = filetext.Split('\n');
            string[] ZeroItems = Lines[0].Split(':');
            strLocalVarsion = ZeroItems[1];
            for (int i = 1; i < Lines.Length; i++)
            {
                Lines[i] = Lines[i].Replace(" ", "");
                if (string.IsNullOrEmpty(Lines[i]))
                    continue;
                string[] Items = Lines[i].Split('\t');
                strLocalDicFiles.Add(Items[0], Items[1]);
            }
        }
    }

    /// <summary>
    /// 下载服务器的CRC表
    /// </summary>
    /// <returns></returns>
    IEnumerator DownLoadSeverCRCDic()
    {
        string strPath = "/Android/";
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            strPath = "/Win/";
        }
        string sPath = SeverPath + strPath + "Varsion.txt";
        WWW www = new WWW(sPath);
        yield return www;
        LoadSeverCRCDic(www.text);
    }

    /// <summary>
    /// 读取服务器的CRC表
    /// </summary>
    void LoadSeverCRCDic(string filetext)
    {
        strSeverDicFiles.Clear();
            if (filetext != "")
            {
                string[] Lines = filetext.Split('\n');
                string[] ZeroItems = Lines[0].Split(':');
                strServerVarsion = ZeroItems[1];
                for (int i = 1; i < Lines.Length; i++)
                {
                    Lines[i] = Lines[i].Replace(" ", "");
                    if (string.IsNullOrEmpty(Lines[i]))
                        continue;
                    string[] Items = Lines[i].Split('\t');
                    strSeverDicFiles.Add(Items[0], Items[1]);
                }
            }
    }

    /// <summary>
    /// CRC马的比较
    /// </summary>
    void CompareCRC()
    {
        DiffFiles.Clear();
        foreach (var item in strSeverDicFiles)
        {
            //如果本地文件不存在或CRC码不同,加入差异列表
            if (!strLocalDicFiles.ContainsKey(item.Key) || item.Value != strLocalDicFiles[item.Key])
            {
                DiffFiles.Add(item.Key);
            }
        }
    }


    /// <summary>
    /// 下载并保存资源
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IEnumerator DownloadAndSave(string url, string name, Action<bool, string> Finish = null)
    {
        string strPath = "/Android";
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            strPath = "/Win";
        }
        url = Uri.EscapeUriString(url);
        string Loading = string.Empty;
        bool b = false;
        WWW www = new WWW(url);
        if (www.error != null)
        {
            Debug.LogError("error:" + www.error);

        }
        while (!www.isDone)
        {
            Loading = (((int)(www.progress * 100)) % 100) + "%";
            Finish?.Invoke(b, Loading);

            yield return 1;
        }
        if (www.isDone)
        {
            Debug.Log(name + ":下载完成");
            Loading = "100%";
            byte[] bytes = www.bytes;
            b = SaveAssets(ResourceManager.GetInstance().GetResPath() + strPath, name, bytes);
            Finish?.Invoke(b, Loading);
        }
    }

    /// <summary>
    /// 保存资源到本地
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="info"></param>
    /// <param name="length"></param>
    public static bool SaveAssets(string path, string name, byte[] bytes)
    {
        string FullPath = path + "/" + name;
        Debug.Log("path:" + path);
        Debug.Log("name:" + name);
        CheckDirectory(FullPath);
        FileInfo t = new FileInfo(FullPath);
        try
        {
            FileStream sw = t.Create();
            sw.Write(bytes, 0, bytes.Length);
            sw.Close();
            sw.Dispose();
            iLoadingFileNum--;
            Debug.Log(name + ":保存成功\n\r路径" + FullPath);
            return true;

        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            return false;
        }
    }

    /// <summary>
    ///从服务器下载有差异的文件
    /// </summary>
    void DownloadDiffFile()
    {
        string strPath = "/Android/";
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            strPath = "/Win/";
        }
        if (iLoadingFileNum == 0)
        {
            if (DiffFiles.Count == 0)
                return;
            string strFile = DiffFiles[0];
            string strSarvarPath = SeverPath + strPath + strFile;

            Debug.Log("开始下载:" + strFile);
            iLoadingFileNum++;
            StartCoroutine(DownloadAndSave(strSarvarPath, strFile));
            //同时下载.mainfast文件
            string strmainfast = strFile + ".manifest";
            string strSarvarMainfastPath = SeverPath + strPath + strmainfast;
            Debug.Log("开始下载:" + strSarvarMainfastPath);
            iLoadingFileNum++;
            StartCoroutine(DownloadAndSave(strSarvarMainfastPath, strmainfast));
            DiffFiles.Remove(DiffFiles[0]);
        }
    }

    /// <summary>
    /// 检查文件夹是否存在，如果不存在就创建
    /// </summary>
    /// <param name="strFilePath"></param>
    static void CheckDirectory(string strFilePath)
    {
        int LastPos = strFilePath.LastIndexOf('/');

        string strPath = strFilePath.Remove(LastPos);   //获得文件路径

        if (!Directory.Exists(strPath))
        {
            //不存在该文件夹，就创建一个
            Directory.CreateDirectory(strPath);
        }
    }

    //游戏第一次运行时解压资源数据至个人目录(!该步骤缺少本地文件校验步骤!)
    /*void CopyLoaclRes()
    {
        string strPath = "/Android/";
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            strPath = "/Win/";
        }
        var PersonPath = ResourceManager.GetInstance().GetResPath() + strPath;
        //如果不存在这个目录 就建立一个,并开始解压
        if (!Directory.Exists(PersonPath))
        {
            Debug.Log("开始解压！！");
            bCopyBaseFiles = true;
            //读取游戏基础资源
            LoadBaseResList();
            CopyBaseFilesInfo();
        }
        else
        {
            bCopyBaseFiles = false;
        }
    }

    /// <summary>
    /// 读取游戏基础资源
    /// </summary>
    void LoadBaseResList()
    {
        Debug.Log("读取原始包资源！！");
        string sPath = "";
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            sPath = "file://" + Application.streamingAssetsPath + "/" + "BaseFileInfo.txt";
        else if (Application.platform == RuntimePlatform.Android)
            sPath = Application.streamingAssetsPath + "/" + "BaseFileInfo.txt";
        WWW www = new WWW(sPath);
        while (!www.isDone) { }

        //因为缺少文件校验所以该功能未完善
        //LoadBaseResFiles();
    }

    void LoadBaseResFiles(AssetBundle assetBundle)
    {
        TextAsset asset = (TextAsset)assetBundle.LoadAsset("Varsion", typeof(TextAsset));
        BaseResFiles.Clear();
        if (asset != null)
        {
            string filetext = asset.text;
            if (filetext != null)
            {
                string[] Lines = filetext.Split('\n');
                for (int i = 0; i < Lines.Length; i++)
                {
                    Lines[i] = Lines[i].Replace(" ", "");
                    if (string.IsNullOrEmpty(Lines[i]))
                        continue;
                    BaseResFiles.Add(Lines[i]);
                }
            }
        }
    }

    void CopyBaseFilesInfo()
    {
        StartCoroutine(CopyBaseFilesToPerDir());
    }

    IEnumerator CopyBaseFilesToPerDir()
    {
        int CurBaseFileIndex = 0;
        while (bCopyBaseFiles)
        {
            if (!bOnCopyBaseFile)
            {
                if (CurBaseFileIndex < BaseResFiles.Count)
                {
                    bOnCopyBaseFile = true;
                    string FileName = BaseResFiles[CurBaseFileIndex].Replace("\r", "");
                    var sec = "";
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                        sec = "file://" + Application.streamingAssetsPath + FileName;
                    else if (Application.platform == RuntimePlatform.Android)
                        sec = Application.streamingAssetsPath + FileName;
                    var dec = ResourceManager.GetInstance().GetResPath() + FileName;

                    CurBaseFileIndex++;

                    StartCoroutine(CopyBFiles(FileName));
                }
            }
            yield return 0.2;
        }
    }


    IEnumerator CopyBFiles(string name)
    {
        var sec = "";
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            sec = "file://" + Application.streamingAssetsPath + name;
        else if (Application.platform == RuntimePlatform.Android)
            sec = Application.streamingAssetsPath + name;
        var dec = ResourceManager.GetInstance().GetResPath() + name;
        WWW www = new WWW(sec);
        yield return www;
        iCopyBaseFileNum++;
        Debug.Log("拷贝文件:" + name);
        SaveBaseFile(www.bytes, dec);
        if (iCopyBaseFileNum == BaseResFiles.Count)
            bCopyBaseFiles = false;
        bOnCopyBaseFile = false;
    }

    void SaveBaseFile(byte[] bytes, string decDic)
    {
        string FullPath = decDic;
        CheckDirectory(FullPath);
        Stream sw;
        FileInfo t = new FileInfo(decDic);
        {
            try
            {
                sw = t.Create();
                sw.Write(bytes, 0, bytes.Length);
                sw.Close();
                sw.Dispose();
            }
            catch(Exception  ex)
            {
                Debug.Log(ex);
            }
        }
    }*/
}

public class UpdatorLog
{
    static UpdatorLog Instance;

    static public UpdatorLog GetInstance()
    {
        if (Instance == null)
        {
            Instance = new UpdatorLog();
        }
        return Instance;
    }

    public string strLog = "";
}
