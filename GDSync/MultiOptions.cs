using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDSync.core;
using Newtonsoft.Json;
using System.IO;

namespace GDSync
{
    [Verb("display", HelpText = "统计文件夹信息")]
    class DisplayOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public string SrcId { get; set; }
        [Option('n', "num-worker", Required = false, HelpText = "工作线程")]
        public int Num { get; set; } = 1;
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
    }


    [Verb("list", HelpText = "统计文件夹仅当前顶级文件信息")]
    class ListCurrentOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public string SrcId { get; set; }
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
    }


    [Verb("save", HelpText = "转存文件夹")]
    class SaveOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public IEnumerable<string> SrcIds { get; set; }
        [Option('d', "dst", Required = true, HelpText = "目标文件夹id")]
        public string DstId { get; set; }
        [Option('n', "num-worker", Required = false, HelpText = "工作线程")]
        public int Num { get; set; } = 1;
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
        [Option('w', "is-show", Required = false, HelpText = "展示信息")]
        public bool IsShow { get; set; } = true;
    }


    [Verb("sync", HelpText = "同步文件夹")]
    class SyncOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public string SrcId { get; set; }
        [Option('d', "dst", Required = true, HelpText = "目标文件夹id")]
        public string DstId { get; set; }
        [Option('n', "num-worker", Required = false, HelpText = "工作线程数量")]
        public int Num { get; set; } = 1;
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
    }


    [Verb("rename", HelpText = "批量重命名")]
    class RenameOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public string SrcId { get; set; }
        [Option('i', "name", Required = true, HelpText = "文件名")]
        public string Name { get; set; }
        [Option('t', "include-top", Required = false, HelpText = "是否重命名顶层文件夹")]
        public bool IsIncludeTop { get; set; } = true;
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
    }


    [Verb("check", HelpText = "查漏补缺")]
    class CheckMissOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public string SrcId { get; set; }
        [Option('d', "dst", Required = true, HelpText = "目标文件夹id")]
        public string DstId { get; set; }
        [Option('n', "num-worker", Required = false, HelpText = "工作线程数量")]
        public int Num { get; set; } = 1;
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
    }


    [Verb("same-info", HelpText = "信息同步")]
    class SameInfoOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public string SrcId { get; set; }
        [Option('d', "dst", Required = true, HelpText = "目标文件夹id")]
        public string DstId { get; set; }
        [Option('n', "num-worker", Required = false, HelpText = "工作线程数量")]
        public int Num { get; set; } = 1;
        [Option('a', "account-dir", Required = false, HelpText = "账号文件夹")]
        public string AccountDir { get; set; } = "./account/sa";
    }


    partial class Program
    {
        /// <summary>
        /// 统计文件夹信息
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static int DisplaySolution(DisplayOptions options)
        {
            var src_uid = options.SrcId;
            var num = options.Num;
            var account_dir = options.AccountDir;


            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(num);

            DriveTasks.ListAll(src_uid, num);

            var result = Global.DisplayFolder(src_uid);
            string JsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            var name = Global.UidDriveFileInfoPairs[src_uid].Name;
            var uid = Global.UidDriveFileInfoPairs[src_uid].Uid;
            File.WriteAllText(Path.Combine(Var.OutputDir, $"{name}({uid}).json"), JsonString);
            Console.WriteLine(JsonString);
            Console.WriteLine($"result is written to {name}({uid}).json");


            return 0;
        }


        /// <summary>
        /// 展示文件夹当前目录
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static int ListCurrentSolution(ListCurrentOptions options)
        {
            var src_uid = options.SrcId;
            var account_dir = options.AccountDir;

            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(1);

            DriveTasks.ListCurrent(src_uid);

            var form_list = new List<FileInfoForm>();
            var uid_name = new Dictionary<string, string>();
            foreach (var item in Global.UidDriveFileInfoPairs)
            {
                form_list.Add(item.Value.ToJson());
                uid_name.Add(item.Key, item.Value.Name);
            }
            string JsonString = JsonConvert.SerializeObject(form_list, Formatting.Indented);
            string JsonString2 = JsonConvert.SerializeObject(uid_name, Formatting.Indented);
            File.WriteAllText(Path.Combine(Var.OutputDir, $"{src_uid}.json"), JsonString);
            File.WriteAllText(Path.Combine(Var.OutputDir, $"{src_uid}-simple.json"), JsonString2);
            Console.WriteLine($"result is written to {src_uid}.json and {src_uid}-simple.json in dir {Var.OutputDir}");

            return 0;
        }


        /// <summary>
        /// 转存文件夹
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static int SaveSolution(SaveOptions options)
        {
            var src_uid_list = options.SrcIds;
            var dst_uid = options.DstId;
            var num = options.Num;
            var account_dir = options.AccountDir;
            var is_show = options.IsShow;


            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(num);

            DriveTasks.MultiSaveTo(src_uid_list, dst_uid, num);
            foreach (var src_uid in src_uid_list)
            {
                Console.WriteLine($"the destination folder https://drive.google.com/drive/u/0/folders/{Global.UidFolderPairs[src_uid]}");
            }

            if (is_show)
            {
                foreach (var src_uid in src_uid_list)
                {
                    var result = Global.DisplayFolder(src_uid);
                    result.FolderName = Global.UidDriveFileInfoPairs[src_uid].Name;
                    string JsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
                    Console.WriteLine(JsonString);
                    Console.WriteLine("****************************************************");
                }
            }


            return 0;
        }


        /// <summary>
        /// 文件夹同步
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static int SyncSolution(SyncOptions options)
        {
            var src_uid = options.SrcId;
            var dst_uid = options.DstId;
            var num = options.Num;
            var account_dir = options.AccountDir;

            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(num);

            DriveTasks.Backup(src_uid, dst_uid, num);
            return 0;
        }


        /// <summary>
        /// 文件批量重命名
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static int RenameSolution(RenameOptions options)
        {
            var src_uid = options.SrcId;
            var name = options.Name;
            var account_dir = options.AccountDir;
            var include_top = options.IsIncludeTop;

            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(1);

            DriveTasks.Rename(src_uid, name, include_top);
            return 0;
        }


        private static int CheckMissSolution(CheckMissOptions options)
        {
            var src_uid = options.SrcId;
            var dst_uid = options.DstId;
            var num = options.Num;
            var account_dir = options.AccountDir;

            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(num);

            DriveTasks.CheckMiss(src_uid, dst_uid);
            return 0;
        }


        private static int SameInfoSolotion(SameInfoOptions options)
        {
            var src_uid = options.SrcId;
            var dst_uid = options.DstId;
            var num = options.Num;
            var account_dir = options.AccountDir;

            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(num);

            DriveTasks.SameInfo(src_uid, dst_uid);
            return 0;
        }
    }
}
