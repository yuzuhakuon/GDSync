using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDFile = Google.Apis.Drive.v3.Data.File;

namespace GDSync.core
{
    public class DriveFileInfo
    {
        public DriveFileInfo(FileInfoForm form, string mission = "kuon")
        {
            Mission = mission;
            FromJson(form);
        }


        public DriveFileInfo(GDFile file, string mission = "kuon")
        {
            Mission = mission;
            FromWeb(file);
        }


        private void FromJson(FileInfoForm form)
        {
            var form_props = typeof(FileInfoForm).GetProperties();

            foreach (var item in form_props)
            {
                var name = item.Name;
                var value = item.GetValue(form, null);
                this.GetType().GetProperty(name).SetValue(this, value);
            }
        }


        public FileInfoForm ToJson()
        {
            var form_props = typeof(FileInfoForm).GetProperties();
            var form = new FileInfoForm();

            foreach (var item in form_props)
            {
                var name = item.Name;
                var value = this.GetType().GetProperty(name).GetValue(this, null);
                item.SetValue(form, value);
            }

            return form;
        }


        private void FromWeb(GDFile file)
        {
            Uid = file.Id;
            Md5Checksum = file.Md5Checksum;
            Name = file.Name;
            Size = file.Size;
            if (Size == null) Size = -1;
            Parents = (List<string>)file.Parents;
            if (Parents == null || Parents.Count == 0)
            {
                Parents = new List<string>() { new string(Uid.ToCharArray().Reverse<char>().ToArray<char>()) };
            }
            Parent = Parents[0];
            MimeType = file.MimeType;
            IsTrashed = file.Trashed;


            if (MimeType.Contains("folder")) IsFolder = true;

        }


        public override string ToString()
        {
            string str;
            List<string> str_vec = new List<string>();
            var props = typeof(DriveFileInfo).GetProperties();

            foreach (var item in props)
            {
                var name = item.Name;
                if (Ignore.Contains(name)) continue;
                var value = item.GetValue(this, null);
                str_vec.Add($"\n{name}: {value}");
            }


            str = string.Join("", str_vec);
            return str;
        }


        public override bool Equals(object obj)
        {
            if (obj is DriveFileInfo)
            {
                DriveFileInfo file = (DriveFileInfo)obj;
                // 先判断文件类型
                if (IsFolder ^ file.IsFolder)
                    return false;// 如果文件类型不一致, 返回False

                if (Md5Checksum == null)// 文件夹的判断
                    return Name == file.Name;
                // 文件的判断
                else
                    return Md5Checksum == file.Md5Checksum;
            }
            return false;
        }


        public override int GetHashCode()
        {
            string str;
            if (IsFolder) str = IsFolder.ToString() + Name;
            else str = IsFolder.ToString() + Md5Checksum;
            return str.GetHashCode();
        }


        public static bool operator ==(DriveFileInfo file1, DriveFileInfo file2)
        {
            return file1.Equals(file2);
        }


        public static bool operator !=(DriveFileInfo file1, DriveFileInfo file2)
        {
            return !(file1 == file2);
        }


        /// <summary>
        /// 文件或文件夹id, 具有唯一性
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 文件md5信息, 注意, 文件夹不具有此属性
        /// </summary>
        public string Md5Checksum { get; set; }
        /// <summary>
        /// 文件或文件夹名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件大小, 文件夹不统计, 标记为-1
        /// </summary>
        public long? Size { get; set; }
        /// <summary>
        /// 父级文件夹, 有多个时选择第一个
        /// </summary>
        public string Parent { get; set; }
        /// <summary>
        /// 父级文件夹列表, 通常只有一个
        /// </summary>
        public List<string> Parents { get; set; }
        /// <summary>
        /// 文件类型, 文件夹通常为folder
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        ///  是否为文件夹类型
        /// </summary>
        public bool IsFolder { get; set; } = false;
        /// <summary>
        ///  该文件或文件夹是否已经被废弃
        /// </summary>
        public bool? IsTrashed { get; set; }
        /// <summary>
        /// 该文件信息所属的任务类别
        /// </summary>
        public string Mission { get; set; }
        /// <summary>
        /// 打印文件信息是忽略某些属性
        /// </summary>
        private readonly List<string> Ignore = new List<string>() { "Parents" };
    }


    public class FileInfoForm
    {
        public string Uid { get; set; }
        public string Md5Checksum { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Parent { get; set; }
        public List<string> Parents { get; set; }
        public string MimeType { get; set; }
        public bool IsFolder { get; set; }
        public bool IsTrashed { get; set; }
        public string Mission { get; set; }
    }

}
