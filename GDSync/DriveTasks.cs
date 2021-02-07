using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using GDSync.core;
using System.IO;

namespace GDSync
{
    public class DriveTasks
    {
        public static void SaveTo(string src_uid, string dst_uid, int num_worker = 1)
        {
            Common.SetMode(Mode.SaveTo);

            var publisher = Common.CreateWorkers(num_worker);
            publisher.AddAction(new GetAndCreateAction(src_uid, dst_uid));
            publisher.WaitForAllIdle();
        }


        public static void MultiSaveTo(IEnumerable<string> src_uid_list, string dst_uid, int num_worker = 1)
        {
            Common.SetMode(Mode.SaveTo);

            var publisher = Common.CreateWorkers(num_worker);
            foreach (var src_uid in src_uid_list)
            {
                publisher.AddAction(new GetAndCreateAction(src_uid, dst_uid));
            }
            publisher.WaitForAllIdle();
        }


        public static void ListAll(string src_uid, int num_worker = 1)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(num_worker);
            publisher.AddAction(new GetAndSearchAction(src_uid));
            publisher.WaitForAllIdle();
        }


        public static void ListCurrent(string src_uid)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(1);
            publisher.AddAction(new ListCurrentFolderAction(src_uid));
            publisher.WaitForAllIdle();
        }


        public static void Backup(string src_uid, string dst_uid, int num_worker = 1)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(num_worker);
            publisher.AddAction(new GetAndSearchAction(src_uid));
            publisher.AddAction(new GetAndSearchAction(dst_uid));
            publisher.WaitForAllIdle(false);

            Common.SetMode(Mode.Init);
            publisher.AddAction(new CompareFolderAction(Global.UidDriveFileInfoPairs[src_uid], Global.UidDriveFileInfoPairs[dst_uid]));
            publisher.WaitForAllIdle(true);
        }


        public static void Rename(string src_uid, string name, bool include_top = true)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(1);
            publisher.AddAction(new ListCurrentFolderAction(src_uid));
            publisher.WaitForAllIdle(false);


            string pattern = @"\D([0,1,2]?\d{1})\D*\(";
            Dictionary<string, List<string>> uidNamePairs = new Dictionary<string, List<string>>();
            if (include_top) uidNamePairs.Add(src_uid, new List<string> { "#Parent Folder#", name });

            foreach (var item in Global.UidDriveFileInfoPairs)
            {
                var file_info = item.Value;
                if (file_info.IsFolder)
                    continue;
                if (file_info.Size < 200 * 1024 * 1024)
                    continue;

                var matches = Regex.Matches(file_info.Name, pattern);
                if (matches.Count < 1)
                    continue;
                var ep = matches[0].Groups[1];
                var ext = Path.GetExtension(file_info.Name);
                var changed_name = $"{name}.Ep{ep}.1080p{ext}";
                uidNamePairs.Add(item.Key, new List<string> { file_info.Name, changed_name });
            }


            foreach (var item in uidNamePairs)
            {
                Console.WriteLine($"{item.Value[0]}\n=> {item.Value[1]}\n");
            }

            Console.WriteLine($"Be sure the changes({uidNamePairs.Count} change), enter y/n to continue: ");
            var word = Console.ReadKey();
            while (!((word.KeyChar == 'y') || (word.KeyChar == 'n')))
            {
                Console.WriteLine($"Be sure the changes({uidNamePairs.Count} change), enter y/n to continue: ");
                word = Console.ReadKey();
            }
            Console.WriteLine("\n");
            if (word.KeyChar == 'y')
            {
                foreach (var item in uidNamePairs)
                {
                    publisher.AddAction(new RenameAction(item.Key, item.Value[1]));
                }
            }
            publisher.WaitForAllIdle(true);
        }


        public static void CheckMiss(string src_uid, string dst_uid, int num_worker = 1)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(num_worker);
            publisher.AddAction(new ListCurrentFolderAction(src_uid));
            publisher.AddAction(new ListCurrentFolderAction(dst_uid));
            publisher.WaitForStageOver();


            var src_fileinfo_list = Global.UidFolderList[src_uid]
                .Select(uid => Global.UidDriveFileInfoPairs[uid])
                .Where(info => info.IsFolder).ToList();
            var dst_fileinfo_list = Global.UidFolderList[dst_uid]
                .Select(uid => Global.UidDriveFileInfoPairs[uid])
                .Where(info => info.IsFolder).ToList();

            List<DriveAction> ActionList = new List<DriveAction>();
            foreach (var dst_fileinfo in dst_fileinfo_list)
            {
                foreach (var src_fileinfo in src_fileinfo_list)
                {
                    if (src_fileinfo == dst_fileinfo)
                    {
                        // dst中存在的文件夹, src中也存在--遍历两文件夹方便比对
                        publisher.AddAction(new SearchFolderAction(src_fileinfo));
                        publisher.AddAction(new SearchFolderAction(dst_fileinfo));
                        ActionList.Add(new CompareFolderAction(src_fileinfo, dst_fileinfo));
                        break;
                    }
                }
            }
            publisher.WaitForStageOver();


            Common.SetMode(Mode.SaveTo);

            publisher.ReportDriveActions(ActionList);
            publisher.WaitForAllIdle(true);
        }


        public static void SameInfo(string src_uid, string dst_uid, int num_worker = 1)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(num_worker);
            publisher.AddAction(new GetAndSearchAction(src_uid));
            publisher.AddAction(new GetAndSearchAction(dst_uid));
            publisher.WaitForStageOver();




        }
    }
}
