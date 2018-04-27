using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public delegate void AsyncReadDataCallBack(byte[] bytes, string sss);
public class TestThread : MonoBehaviour
{
    public static TestThread Instance = null;
    class FileData
    {
        public string sName;
        public byte[] bytes;
        public Thread pThread;
        public int nTimeSleep = 0;
        public AsyncReadDataCallBack callBack;
    }

    List<FileData> datalist = new List<FileData>();

    private void Awake()
    {
        Instance = this;
    }
    // Use this for initialization
    void Start ()
    {
        aaa();
	}
	
	// Update is called once per frame
	void Update ()
    {
        for (int i = 0; i < datalist.Count; ++i)
        {
            datalist[i].callBack(datalist[i].bytes, datalist[i].sName);
            datalist[i].pThread.Abort();
        }
        datalist.Clear();
    }

    private void OnDestroy()
    {
        
    }

    void aaa()
    {
        
    }

    public void AddDataReq(string sBundleName, AsyncReadDataCallBack Callback)
    {
        FileData data = new FileData();
        data.sName = sBundleName;
        data.callBack = Callback;
        data.pThread = new Thread(delegate ()
        {
            lock (datalist)
            {
                data.bytes = new byte[10];
                System.Random r = new System.Random();
                data.nTimeSleep = r.Next() % 900 + 100;
                Thread.Sleep(data.nTimeSleep);
                datalist.Add(data);
            }
        });

        data.pThread.IsBackground = true;
        data.pThread.Start();
    }
}

