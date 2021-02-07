using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GDSync.core
{
    public class FolderDisplayForm
    {
        [JsonProperty(PropertyName = "文件夹名称")]
        public string FolderName { get; set; } = "None";
        [JsonProperty(PropertyName = "文件夹数量")]
        public long FolderCount { get; set; } = 0;
        [JsonProperty(PropertyName = "文件数量")]
        public long FileCount { get; set; } = 0;
        [JsonProperty(PropertyName = "文件扩展分类数量")]
        public Dictionary<string, long> ExtCount { get; set; } = new Dictionary<string, long>();
        [JsonProperty(PropertyName = "文件扩展分类大小(B)")]
        public Dictionary<string, long> ExtSize { get; set; } = new Dictionary<string, long>();
        [JsonProperty(PropertyName = "KB级别文件数量")]
        public long KB { get; set; } = 0;
        [JsonProperty(PropertyName = "MB级别文件数量")]
        public long MB { get; set; } = 0;
        [JsonProperty(PropertyName = "GB级别文件数量")]
        public long GB { get; set; } = 0;
        [JsonProperty(PropertyName = "总大小(B)")]
        public long Size { get; set; } = 0;


        public static FolderDisplayForm operator +(FolderDisplayForm form1, FolderDisplayForm form2)
        {
            FolderDisplayForm form = new FolderDisplayForm
            {
                FolderCount = form1.FolderCount + form2.FolderCount,
                FileCount = form1.FileCount + form2.FileCount,
                ExtCount = Plus(form1.ExtCount, form2.ExtCount),
                ExtSize = Plus(form1.ExtSize, form2.ExtSize),
                KB = form1.KB + form2.KB,
                MB = form1.MB + form2.MB,
                GB = form1.GB + form2.GB,
                Size = form1.Size + form2.Size
            };


            return form;
        }


        public static Dictionary<string, long> Plus(Dictionary<string, long> first, Dictionary<string, long> second)
        {
            foreach (var item in second)
            {
                if (first.ContainsKey(item.Key))
                {
                    first[item.Key] += item.Value;
                }
                else
                {
                    first.Add(item.Key, item.Value);
                }
            }


            return first;
        }


        public static long Plus(long first, long second)
        {
            return first + second;
        }
    }


    public class ActionForm
    {
        public string Name { get; set; }
        public List<string> Params { get; set; }
    }

}
