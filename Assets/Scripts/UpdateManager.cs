using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

public class cccccbbbb : ZipUtility.UnzipCallback
{
    public override void OnPostUnzip(ZipEntry _entry)
    {
        
    }
    public override void OnFinished(bool _result)
    {
        Test.Instance.Log(_result + " 解压完成");
    }
};

public class UpdateManager
{
    public static UpdateManager Instance = new UpdateManager();

    public float progress { get; private set; }
    public long mFileLength { get; private set; }

    private bool isStop;
    private Thread mDownloadThread;
    private Thread mUnZipThread;

    public cccccbbbb cb = new cccccbbbb();

    HttpWebRequest mRequest = null;
    Stream mHttpStream = null;
    HttpWebResponse mResponse = null;

    string mFileDirectory = string.Empty;
    public void CheckVersions()
    {
        mFileDirectory = Application.persistentDataPath;
        ServicePointManager.DefaultConnectionLimit = 200;

        //mFileDirectory = "D:/Work/Unity/Fu/zipun";
        ResourceLoadManager.Instance.LoadUrlBytes("http://10.20.10.131/versions.txt", (byte[] bytes, string sText) =>
        {
            // 比较版本号 
            // 需要更新版本 下载更新包
            if (true)
            {
                // 关机等异常退出 更新未完成或解压未完成处理
                // 如果更新包下载完成 直接到解压
                string realFilePath = Application.persistentDataPath + "/RakNet.zip";
                Test.Instance.Log(realFilePath);
                if (File.Exists(realFilePath))
                {
                    UnZip();
                    return;
                }

                Close("初始化");
                Download("http://10.20.10.131/RakNet.zip", "RakNet");
            }
        });
    }

    public void Download(string _url, string _fileName)
    {
        isStop = false;
        mDownloadThread = new Thread(delegate ()
        {
            if (!Directory.Exists(mFileDirectory))
                Directory.CreateDirectory(mFileDirectory);
            string filePath = mFileDirectory + "/" + _fileName + ".bmp";
            string realFilePath = mFileDirectory + "/" + _fileName + ".zip";

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            mFileLength = fileStream.Length;
            long totalLength = GetDownloadLength(_url);

            if (mFileLength < totalLength && totalLength > 0)
            {
                Test.Instance.Log("开始下载");
                try
                {
                    mRequest = (HttpWebRequest)HttpWebRequest.Create(_url);
                    mRequest.Timeout = 10000;
                    mRequest.ReadWriteTimeout = 10000;
                    mRequest.AddRange((int)mFileLength);

                    mResponse = (HttpWebResponse)mRequest.GetResponse();
                    fileStream.Seek(mFileLength, SeekOrigin.Begin);
                    mHttpStream = mResponse.GetResponseStream();
                    byte[] buffer = new byte[1024];
                    int length = mHttpStream.Read(buffer, 0, buffer.Length);
                    
                    while (length > 0)
                    {
                        if (isStop)
                            break;
                        fileStream.Write(buffer, 0, length);
                        mFileLength += length;
                        progress = (float)mFileLength / totalLength;
                        fileStream.Flush();

                        length = mHttpStream.Read(buffer, 0, buffer.Length);
                    }
                    mHttpStream.Close();
                    mHttpStream.Dispose();
                }
                catch (WebException e)
                {
                    mResponse.Close();

                    mHttpStream.Close();
                    mHttpStream.Dispose();

                    Test.Instance.Log("超时");
                    Test.Instance.Log(e.Message);
                }
                catch
                {
                    Test.Instance.Log("xxxxxxxxx");
                }
            }

            fileStream.Close();
            fileStream.Dispose();

            if (mFileLength >= totalLength && totalLength > 0)
            {
                Test.Instance.Log("DownLoad Fnish");
                progress = 1.0f;
                File.Move(filePath, realFilePath);
                UnZip();
            }
        });
        mDownloadThread.IsBackground = true;
        mDownloadThread.Start();
    }

    public void Close(string ssss)
    {
        Test.Instance.Log("Close    " + ssss);
        isStop = true;

        if (mRequest != null)
        {
            mRequest.Abort();
            mRequest = null;
        }

        if (mResponse != null)
        {
            mResponse.Close();
            mResponse = null;
        }

        if (mHttpStream != null)
        {
            mHttpStream.Close();
            mHttpStream.Dispose();
            mHttpStream = null;
        }
        if (mDownloadThread != null)
        {
            mDownloadThread.Abort();
        }
        ZipUtility.Stop();
    }


    public float GetCurrentProgress(string _url, string _fileName)
    {
        string filePath = mFileDirectory + "/" + _fileName + ".bmp";
        string realFilePath = mFileDirectory + "/" + _fileName + ".zip";
        float currentProgress = 0;
        if (File.Exists(realFilePath))
        {
            currentProgress = 100.0f;
        }
        else
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
            long fileLength = fileStream.Length;
            long totalLength = GetDownloadLength(_url);
            Debug.Log(fileLength + "  " + totalLength);
            currentProgress = (float)fileLength / totalLength;
            Debug.Log(currentProgress);
        }
        return currentProgress;
    }

    long GetDownloadLength(string _fileUrl)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_fileUrl);
            request.Method = "HEAD";

            HttpWebResponse res = (HttpWebResponse)request.GetResponse();

            long nLength = res.ContentLength;
            res.Close();
            res = null;

            return nLength;
        }
        catch (WebException e)
        {
            Test.Instance.Log("网络异常 是否重试？");
        }
        return 0;
    }

    // 解压更新包
    public void UnZip()
    {
        mUnZipThread = new Thread(delegate ()
        {
            string aaa = mFileDirectory + "/RakNet.zip";

            if (ZipUtility.UnzipFile(aaa, mFileDirectory, null, cb))
            {
                File.Delete(aaa);
                // 修改本地版本号
            }
        });
        mUnZipThread.IsBackground = true;
        mUnZipThread.Start();
    }
}