using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDFile = Google.Apis.Drive.v3.Data.File;

namespace GDSync.core
{
    abstract public class DriveAction
    {
        /// <summary>
        /// 执行具体的操作
        /// </summary>
        /// <param name="card">授权信息</param>
        abstract public void Execute(ServiceCard card);
        /// <summary>
        /// 当前Action完成后引起的后续Action
        /// </summary>
        /// <returns></returns>
        abstract public List<DriveAction> CreateNewActions();
        abstract public ActionForm Dump();
        abstract public void Load(ActionForm form);


        public string GetTypeName()
        {
            return this.GetType().Name;
        }


        public void IncreaseUsageLimitError()
        {
            UsageLimit++;
        }


        public static List<DriveAction> CreateAllInstancesOf(List<ActionForm> actionForms)
        {
            var instances = new List<DriveAction>();
            IEnumerable<Type> types = typeof(DriveAction).Assembly.GetTypes() //获取当前类库下所有类型
                .Where(t => typeof(DriveAction).IsAssignableFrom(t)) //获取间接或直接继承t的所有类型
                .Where(t => !t.IsAbstract && t.IsClass); //获取非抽象类 排除接口继承

            Dictionary<string, Type> keyTypePairs = new Dictionary<string, Type>();
            foreach (var type in types)
            {
                keyTypePairs.Add(type.Name, type);
            }

            foreach (var form in actionForms)
            {
                var instance = (DriveAction)Activator.CreateInstance(keyTypePairs[form.Name]);
                instance.Load(form);
                instances.Add(instance);
            }
            return instances;
        }


        public DriveFileInfo src;
        public DriveFileInfo dst;
        public string src_uid;
        public string dst_uid;
        public string name;
        public string Mission { get; set; }
        public int UsageLimit { get; set; } = 0;
    }


    /// <summary>
    /// 创建文件夹
    /// </summary>
    public class CreateFolderAction : DriveAction
    {
        public CreateFolderAction()
        {

        }
        public CreateFolderAction(DriveFileInfo src)
        {
            this.src = src;
            this.src_uid = src.Uid;
            this.dst_uid = Global.UidFolderPairs[src.Parent];
        }


        public override void Execute(ServiceCard card)
        {
            var uid = DriveOp.CreateFolder(card.Service, src, dst_uid);
            Var.logger.Info($"(worker id: {Thread.CurrentThread.ManagedThreadId}) create folder: {src.Name}({src.Uid}) in {uid}");
            Global.AddFolderPair(src_uid, uid);
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>
            {
                new SearchFolderAction(src)
            };

            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src = Global.UidDriveFileInfoPairs[form.Params[0]];
            src_uid = src.Uid;
            dst_uid = Global.UidFolderPairs[src.Parent];
        }
    }


    /// <summary>
    /// 拷贝文件
    /// </summary>
    public class CopyFileToAction : DriveAction
    {
        public CopyFileToAction(DriveFileInfo src, string dst_folder_id)
        {
            this.src = src;
            this.src_uid = src.Uid;
            this.dst_uid = dst_folder_id;
        }


        public override void Execute(ServiceCard card)
        {
            var file = DriveOp.Copy(card.Service, src.Uid, dst_uid);
            Var.logger.Info($"(worker id: {Thread.CurrentThread.ManagedThreadId}) copy file: {src.Name}({src.Uid}) to {dst_uid}, and get {file.Id}");
            card.Remain -= (long)file.Size;
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid, dst_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src = Global.UidDriveFileInfoPairs[form.Params[0]];
            src_uid = src.Uid;
            dst_uid = form.Params[1];
        }
    }


    /// <summary>
    /// 遍历文件夹
    /// </summary>
    public class SearchFolderAction : DriveAction
    {
        public SearchFolderAction(DriveFileInfo src)
        {
            this.src = src;
            this.src_uid = src.Uid;
        }


        public override void Execute(ServiceCard card)
        {
            var files = DriveOp.ListCurrentFiles(card.Service, src_uid);
            Var.logger.Info($"(worker id: {Thread.CurrentThread.ManagedThreadId}) list folder: {src.Name}({src.Uid}), and it has {files.Count} files(folders)");
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();

            foreach (var file_id in Global.UidFolderList[src_uid])
            {
                var file_info = Global.UidDriveFileInfoPairs[file_id];
                if (file_info.IsFolder)
                {
                    if (!Flags.OnlyList)
                        ActionList.Add(new CreateFolderAction(file_info));
                    else
                        ActionList.Add(new SearchFolderAction(file_info));
                }
                else
                {
                    if (!Flags.OnlyList)
                        ActionList.Add(new CopyFileToAction(file_info, Global.UidFolderPairs[src_uid]));
                }
            }


            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src = Global.UidDriveFileInfoPairs[form.Params[0]];
            src_uid = form.Params[0];
        }
    }


    /// <summary>
    /// 获取当前文件夹信息并遍历
    /// </summary>
    public class GetAndSearchAction : DriveAction
    {
        public GetAndSearchAction(string src_uid)
        {
            this.src_uid = src_uid;
        }


        public override void Execute(ServiceCard card)
        {
            var result = DriveOp.GetDriveFileInfo(card.Service, src_uid);
            var file_info = new DriveFileInfo(result);
            Var.logger.Info($"(worker id: {Thread.CurrentThread.ManagedThreadId}) get folder information: {file_info.Name}({file_info.Uid}), and then will search the folder");

            Global.AddDriveFileInfo(file_info);
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            var file_info = Global.UidDriveFileInfoPairs[src_uid];
            ActionList.Add(new SearchFolderAction(file_info));

            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src_uid = src.Uid;
        }
    }


    /// <summary>
    /// 获取当前文件夹信息并创建对应文件夹
    /// </summary>
    public class GetAndCreateAction : DriveAction
    {
        public GetAndCreateAction(string src_uid, string dst_uid)
        {
            this.src_uid = src_uid;
            this.dst_uid = dst_uid;
        }


        public override void Execute(ServiceCard card)
        {
            var result = DriveOp.GetDriveFileInfo(card.Service, src_uid);
            var file_info = new DriveFileInfo(result);
            Var.logger.Info($"(worker id: {Thread.CurrentThread.ManagedThreadId}) get folder information: {file_info.Name}({file_info.Uid}), and then will search the folder");

            Global.AddDriveFileInfo(file_info);
            Global.AddFolderPair(file_info.Parent, dst_uid);
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            var file_info = Global.UidDriveFileInfoPairs[src_uid];
            ActionList.Add(new CreateFolderAction(file_info));

            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid, dst_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src_uid = form.Params[0];
            dst_uid = form.Params[1];
        }
    }


    public class CompareFolderAction : DriveAction
    {
        public CompareFolderAction()
        {

        }
        public CompareFolderAction(DriveFileInfo src, DriveFileInfo dst)
        {
            this.src = src;
            this.dst = dst;
            this.src_uid = src.Uid;
            this.dst_uid = dst.Uid;
        }


        public override void Execute(ServiceCard card)
        {
            Global.AddFolderPair(src_uid, dst_uid);
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            // 1 src中存在的文件夹, dst中也存在--两文件夹比对
            // 2 src中存在的文件夹, 但dst中不存在--遍历与创建文件夹
            // 3 src中存在的文件, dst中不存在--拷贝文件
            var src_fileinfo_list = Global.UidFolderList[src_uid].Select(uid => Global.UidDriveFileInfoPairs[uid]).ToList();
            var dst_fileinfo_list = Global.UidFolderList[dst_uid].Select(uid => Global.UidDriveFileInfoPairs[uid]).ToList();

            foreach (var src_fileinfo in src_fileinfo_list)
            {
                bool all_mismatch = true;
                foreach (var dst_fileinfo in dst_fileinfo_list)
                {
                    if (src_fileinfo == dst_fileinfo)
                    {
                        all_mismatch = false;
                        if (src_fileinfo.IsFolder && (!Flags.OnlyCompareTop))
                        {
                            // 1 src中存在的文件夹, dst中也存在--两文件夹比对
                            ActionList.Add(new CompareFolderAction(src_fileinfo, dst_fileinfo));
                        }
                        break;
                    }
                }
                if (all_mismatch)
                {
                    if (src_fileinfo.IsFolder)
                    {
                        // 2 src中存在的文件夹, 但dst中不存在--遍历与创建文件夹
                        ActionList.Add(new CreateFolderAction(src_fileinfo));
                    }
                    else
                    {
                        // 3 src中存在的文件, dst中不存在--拷贝文件
                        ActionList.Add(new CopyFileToAction(src_fileinfo, dst_uid));
                    }
                }
            }


            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid, dst_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src = Global.UidDriveFileInfoPairs[form.Params[0]];
            dst = Global.UidDriveFileInfoPairs[form.Params[1]];
            src_uid = form.Params[0];
            dst_uid = form.Params[1];
        }
    }


    /// <summary>
    /// 遍历当前文件
    /// </summary>
    public class ListCurrentFolderAction : DriveAction
    {
        public ListCurrentFolderAction(string src_folder_id)
        {
            this.src_uid = src_folder_id;
        }


        public override void Execute(ServiceCard card)
        {
            var files = DriveOp.ListCurrentFiles(card.Service, src_uid);
            Var.logger.Info($"list folder: {src_uid}, and get {files.Count} files(folder)");
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src_uid = form.Params[0];
        }
    }


    public class RenameAction : DriveAction
    {
        public RenameAction()
        {

        }


        public RenameAction(string src_file_uid, string name)
        {
            src_uid = src_file_uid;
            this.changed_name = name;
        }


        public override void Execute(ServiceCard card)
        {
            DriveOp.Rename(card.Service, src_uid, changed_name);
            Var.logger.Info($"rename {src_uid} to {changed_name}");
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            return ActionList;
        }


        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid, changed_name }
            };
            return form;
        }


        public override void Load(ActionForm form)
        {
            src_uid = form.Params[0];
            changed_name = form.Params[1];
        }

        public string changed_name;
    }


    public class SameInfoAction : DriveAction
    {
        public SameInfoAction()
        {

        }
        public SameInfoAction(string src_folder_uid, string dst_folder_uid)
        {
            src_uid = src_folder_uid;
            dst_uid = dst_folder_uid;
        }

        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            var srcUidMD5Pairs = Common.GetAllFileChildren(src_uid);
            var dstUidMD5Pairs = Common.GetAllFileChildren(dst_uid);

            foreach (var dst_info in dstUidMD5Pairs)
            {
                foreach (var src_info in srcUidMD5Pairs)
                {
                    if (src_info == dst_info)
                    {
                        ActionList.Add(new RenameAction(dst.Uid, src.Name));
                        break;
                    }
                }
            }


            return ActionList;
        }

        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid, dst_uid }
            };
            return form;
        }

        public override void Execute(ServiceCard card)
        {
            Global.AddFolderPair(src_uid, dst_uid);
        }

        public override void Load(ActionForm form)
        {
            src_uid = form.Params[0];
            dst_uid = form.Params[1];
        }
    }


    public class CheckMissAction : DriveAction
    {
        public CheckMissAction()
        {

        }


        public CheckMissAction(string src_folder_uid, string dst_folder_uid)
        {
            src_uid = src_folder_uid;
            dst_uid = dst_folder_uid;
        }


        public override void Execute(ServiceCard card)
        {
            Global.AddFolderPair(src_uid, dst_uid);
        }


        public override List<DriveAction> CreateNewActions()
        {
            List<DriveAction> ActionList = new List<DriveAction>();
            var src_fileinfo_list = Global.UidFolderList[src_uid].Select(uid => Global.UidDriveFileInfoPairs[uid]).ToList();
            var dst_fileinfo_list = Global.UidFolderList[dst_uid].Select(uid => Global.UidDriveFileInfoPairs[uid]).ToList();

            foreach (var dst_fileinfo in dst_fileinfo_list)
            {
                if (!dst_fileinfo.IsFolder) continue;
                foreach (var src_fileinfo in src_fileinfo_list)
                {
                    if (src_fileinfo == dst_fileinfo)
                    {
                        // dst中存在的文件夹, src中也存在--两文件夹比对
                        ActionList.Add(new CompareFolderAction(src_fileinfo, dst_fileinfo));
                        break;
                    }
                }
            }

            return ActionList;
        }

        public override ActionForm Dump()
        {
            var form = new ActionForm()
            {
                Name = GetTypeName(),
                Params = new List<string> { src_uid, dst_uid }
            };
            return form;
        }

        public override void Load(ActionForm form)
        {
            src_uid = form.Params[0];
            dst_uid = form.Params[1];
        }
    }
}
