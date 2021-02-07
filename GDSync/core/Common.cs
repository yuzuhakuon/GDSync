using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDSync.core
{
    public enum Mode { Init, OnlyList, SaveTo, CompareCurrent }


    public class Common
    {
        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="fileName">文件全路径</param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            string text = File.ReadAllText(fileName);
            byte[] byteValue, byteHash;

            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byteValue = System.Text.Encoding.UTF8.GetBytes(text);
            byteHash = md5.ComputeHash(byteValue);
            md5.Clear();
            string str = "";
            for (int i = 0; i < byteHash.Length; i++)
            {
                str += byteHash[i].ToString("X").PadLeft(2, '0');
            }
            return str.ToLower();
        }


        public static bool IsUserAccountJson(string path)
        {
            bool status = true;
            var text = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);
            if (result.ContainsKey("type"))
            {
                status = false;
            }
            return status;
        }


        public static bool IsStorageLimited(Google.GoogleApiException e)
        {
            var target = "userRateLimitExceeded";
            foreach (var item in e.Error.Errors)
            {
                if (item.Reason == target)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 创建工作线程
        /// </summary>
        /// <param name="num">工作线程数量</param>
        /// <returns></returns>
        public static Guild CreateWorkers(int num = 1)
        {
            var publisher = new Guild();
            for (int i = 0; i < num; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var worker = new Adventurer();
                    worker.Register(ref publisher);
                    worker.Start();
                });
            }
            return publisher;
        }


        /// <summary>
        /// 设置任务状态
        /// </summary>
        /// <param name="mode"></param>
        public static void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Init:
                    SetInitMode();
                    break;
                case Mode.SaveTo:
                    SetSaveToMode();
                    break;
                case Mode.OnlyList:
                    SetListMode();
                    break;
                case Mode.CompareCurrent:
                    SetCompareCurrentMode();
                    break;
                default:
                    SetInitMode();
                    break;
            }
        }


        /// <summary>
        /// 初始化状态
        /// </summary>
        private static void SetInitMode()
        {
            Flags.OnlyList = false;
            Flags.OnlyCompareTop = false;
        }


        /// <summary>
        /// 遍历文件夹模式
        /// </summary>
        private static void SetListMode()
        {
            Flags.OnlyList = true;
        }


        /// <summary>
        /// 转存模式
        /// </summary>
        private static void SetSaveToMode()
        {
            Flags.OnlyList = false;
        }


        /// <summary>
        /// 备份模式
        /// </summary>
        private static void SetCompareCurrentMode()
        {
            Flags.OnlyList = false;
            Flags.OnlyCompareTop = true;
        }


        public static List<DriveFileInfo> GetAllFileChildren(string uid)
        {
            var fileList = new List<DriveFileInfo>();
            foreach (var item in Global.UidFolderList[uid])
            {
                var file_info = Global.UidDriveFileInfoPairs[item];
                if (file_info.IsFolder)
                {
                    fileList.AddRange(GetAllFileChildren(item));
                }
                else
                {
                    fileList.Add(file_info);
                }
            }


            return fileList;
        }


        public static void LoadCache()
        {

        }
        public static void SaveCache()
        {

        }
    }


    public static class Global
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public static Dictionary<string, DriveFileInfo> UidDriveFileInfoPairs { get; set; } = new Dictionary<string, DriveFileInfo>();
        public static Object UidDriveFileInfoPairsLocker { get; set; } = new Object();
        /// <summary>
        /// 文件夹内容列表
        /// </summary>
        public static Dictionary<string, List<string>> UidFolderList { get; set; } = new Dictionary<string, List<string>>();
        public static Object UidFolderListLocker { get; set; } = new Object();
        /// <summary>
        /// 源文件夹与目标文件夹一一对应
        /// </summary>
        public static Dictionary<string, string> UidFolderPairs { get; set; } = new Dictionary<string, string>();
        public static Object UidFolderPairsLocker { get; set; } = new Object();

        public static void ClearFileCache()
        {
            UidDriveFileInfoPairs.Clear();
            UidFolderList.Clear();
            UidFolderPairs.Clear();
        }
        /// <summary>
        /// 在遍历查找完文件夹后, 将该文件夹下的所有文件信息存放至全局变量中, 存放内容皆为文件uid.
        /// </summary>
        /// <param name="uid">父文件夹</param>
        /// <param name="files">子文件</param>
        public static void AddFolderList(string uid, List<Google.Apis.Drive.v3.Data.File> files)
        {
            var uidList = new List<string>();


            foreach (var file in files)
            {
                var info = new DriveFileInfo(file);
                AddDriveFileInfo(info);
                uidList.Add(info.Uid);
            }


            lock (UidFolderListLocker)
            {
                if (UidFolderList.ContainsKey(uid))
                {
                    UidFolderList[uid] = uidList;
                }
                else
                {
                    UidFolderList.Add(uid, uidList);
                }
            }
        }


        /// <summary>
        /// 添加文件id与文件信息的一一对应关系
        /// </summary>
        /// <param name="file"></param>
        public static void AddDriveFileInfo(DriveFileInfo file)
        {
            lock (UidDriveFileInfoPairsLocker)
            {
                if (UidDriveFileInfoPairs.ContainsKey(file.Uid))
                {
                    UidDriveFileInfoPairs[file.Uid] = file;
                }
                else
                {
                    UidDriveFileInfoPairs.Add(file.Uid, file);
                }
            }
        }


        /// <summary>
        /// 添加源文件夹与目标文件夹的对应关系
        /// </summary>
        /// <param name="src_uid"></param>
        /// <param name="dst_uid"></param>
        public static void AddFolderPair(string src_uid, string dst_uid)
        {
            lock (UidFolderPairsLocker)
            {
                if (UidFolderPairs.ContainsKey(src_uid))
                {
                    UidFolderPairs[src_uid] = dst_uid;
                }
                else
                {
                    UidFolderPairs.Add(src_uid, dst_uid);
                }
            }
        }


        public static List<T> Shuffle<T>(this List<T> list)
        {
            return list.OrderBy(x => Guid.NewGuid()).ToList();
        }


        public static FolderDisplayForm DisplayFolder(string src_uid)
        {
            var result = new FolderDisplayForm();
            foreach (var uid in UidFolderList[src_uid])
            {
                var file_info = UidDriveFileInfoPairs[uid];
                if (file_info.IsFolder)
                {
                    result.FolderCount++;
                    result += DisplayFolder(file_info.Uid);
                }
                else
                {
                    result.FileCount++;
                    var ext = Path.GetExtension(file_info.Name);
                    if (result.ExtCount.ContainsKey(ext))
                    {
                        result.ExtCount[ext]++;
                        result.ExtSize[ext] += (long)file_info.Size;
                    }
                    else
                    {
                        result.ExtCount.Add(ext, 1);
                        result.ExtSize.Add(ext, (long)file_info.Size);
                    }
                    if (IsGB((long)file_info.Size))
                    {
                        result.GB++;
                    }
                    else if (IsMB((long)file_info.Size))
                    {
                        result.MB++;
                    }
                    else
                    {
                        result.KB++;
                    }
                    result.Size += (long)file_info.Size;
                }
            }

            return result;
        }


        private static bool IsGB(long size)
        {
            return size / (1024 * 1024 * 1024) > 0;
        }
        private static bool IsMB(long size)
        {
            return size / (1024 * 1024) > 0;
        }
    }

    /// <summary>
    /// Global Variable
    /// </summary>
    public class Var
    {
        public static string OutputDir { get; } = "./output";
        public static string LogDir { get; } = "./Log";
        public static string FIELDS { get; } = "kind, parents, id, name, mimeType, size, md5Checksum, trashed";
        public static string Search { get; } = "mimeType='application/vnd.google-apps.folder' and trashed=false";
        public static int CumErrorNum { get; } = 1;
        public static log4net.ILog logger = log4net.LogManager.GetLogger("Logger");
    }


    public class Flags
    {
        /// <summary>
        /// 仅遍历当前文件夹
        /// </summary>
        public static bool OnlyList { get; set; } = false;
        public static bool OnlyCompareTop { get; set; } = false;
    }
}
