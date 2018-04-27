using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class FileBuilder
{
    struct PakFile
    {
        public bool bIsExist;
        public uint nHashA;
        public uint nHashB;
        public uint nFilePos;
        public uint nFileSize;
        public uint nHashIndex;
    };

    public static FileBuilder Instance = new FileBuilder();

    const int nHashTableSize = 65535;
    PakFile[] pFileHashTable = new PakFile[nHashTableSize];
    bool[] pHashMapIndex = new bool[nHashTableSize];
    uint[] cryptTable = new uint[0x500];

    int nPakHeadPos = 0;
    int nPakSeekPos = 0;

    public FileBuilder()
    {
        PrepareCryptTable();
    }

    void PrepareCryptTable()
    {
        uint seed = 0x00100001, index1 = 0, index2 = 0, i;
        for (index1 = 0; index1 < 0x100; index1++)
        {
            for (index2 = index1, i = 0; i < 5; i++, index2 += 0x100)
            {
                uint temp1, temp2;
                seed = (seed * 125 + 3) % 0x2AAAAB;
                temp1 = (seed & 0xFFFF) << 0x10;
                seed = (seed * 125 + 3) % 0x2AAAAB;
                temp2 = (seed & 0xFFFF);
                cryptTable[index2] = (temp1 | temp2);
            }
        }
    }

    uint HashString(string lpszFileName, ulong dwHashType)
    {
        string lpszString = lpszFileName.ToUpper();
        uint seed1 = 0x7FED7FED;
        uint seed2 = 0xEEEEEEEE;
        uint ch;
        for (int i = 0; i < lpszString.Length; ++i)
        {
            ch = lpszString[i];
            seed1 = cryptTable[(dwHashType << 8) + ch] ^ (seed1 + seed2);
            seed2 = ch + seed1 + seed2 + (seed2 << 5) + 3;
        }
        return seed1;
    }

    int GetHashTablePos(string lpszString)
    {
        const int HASH_OFFSET = 0, HASH_A = 1, HASH_B = 2;

        uint nHash = HashString(lpszString, HASH_OFFSET);
        uint nHashA = HashString(lpszString, HASH_A);
        uint nHashB = HashString(lpszString, HASH_B);
        uint nHashStart = nHash % (uint)pFileHashTable.Length;
        int nHashPos = (int)nHashStart;

        while (pFileHashTable[nHashPos].bIsExist)
        {
            if (pFileHashTable[nHashPos].nHashA == nHashA && pFileHashTable[nHashPos].nHashB == nHashB)
            {
                return nHashPos;
            }
            else
            {
                nHashPos = (nHashPos + 1) % pFileHashTable.Length;
            }

            if (nHashPos == nHashStart)
                break;
        }
        return -1;
    }

    int GetHashTableIndex(uint nHashPos)
    {
        uint nHashStart = nHashPos % nHashTableSize;
        uint nHashIndex = nHashStart;
        while (true)
        {
            if (!pHashMapIndex[nHashIndex])
            {
                pHashMapIndex[nHashIndex] = true;
                return (int)nHashIndex;
            }
            else
            {
                Debug.Log("重复   " + nHashStart);
                nHashIndex = (nHashIndex + 1) % nHashTableSize;
            }

            if (nHashIndex == nHashStart)
                break;
        }
        Debug.LogError("Hash 索引值 = -1!!!!!!");
        return -1;
    }
    
    public void MakePackage()
    {
        TimeHelp t1 = new TimeHelp();
        t1.Start();

        List<string> sFileList = Directory.GetFiles(Application.streamingAssetsPath + "/res", "*.*", SearchOption.AllDirectories).Where(f => !(f.ToLower().EndsWith(".meta") || f.ToLower().EndsWith(".manifest"))).ToList();
        
        Array.Clear(pHashMapIndex, 0, pHashMapIndex.Length);

        nPakHeadPos = 0;
        nPakSeekPos = sizeof(uint) * 5 * sFileList.Count;

        FileStream fs = File.Create(Application.streamingAssetsPath + "/ss.raw", nPakSeekPos);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(sFileList.Count);
        nPakHeadPos += sizeof(int);
        for (int i = 0; i < sFileList.Count; ++i)
        {
            PackFileToPackage(sFileList[i], bw);
        }
        bw.Close();
        fs.Close();

        t1.Stop();
        Debug.Log("完成 耗时 " + t1.Duration);
    }

    public void PackFileToPackage(string sPath, BinaryWriter bw)
    {
        const int HASH_OFFSET = 0, HASH_A = 1, HASH_B = 2;

        FileInfo pFileInfo = new FileInfo(sPath);
        string sKey = sPath.Replace(Application.streamingAssetsPath + "/res\\", "");
        sKey = sKey.ToLower().Replace("\\", "/");

        uint nHashPos = HashString(sKey, HASH_OFFSET);
        uint nHashIndex = (uint)GetHashTableIndex(nHashPos);

        bw.Seek(nPakHeadPos, SeekOrigin.Begin);
        bw.Write(nHashIndex);               // hash表索引
        bw.Write(HashString(sKey, HASH_A)); // hash 校验值A
        bw.Write(HashString(sKey, HASH_B)); // hash 校验值B
        bw.Write(nPakHeadPos);              // 文件位置
        bw.Write(pFileInfo.Length);         // 文件大小

        nPakHeadPos += sizeof(uint) * 5;

        bw.Seek(nPakSeekPos, SeekOrigin.Begin);
        byte[] data = new byte[pFileInfo.Length];
        pFileInfo.OpenRead().Read(data, 0, (int)pFileInfo.Length);
        bw.Write(data);

        nPakSeekPos += (int)pFileInfo.Length;
    }
}