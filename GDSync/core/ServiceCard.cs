using Google.Apis.Drive.v3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDSync.core
{
    public class ServiceCard
    {
        public ServiceCard(string path)
        {
            JsonPath = path;
            Uid = Path.GetFileNameWithoutExtension(path);
            GetCredential(path);


        }


        private void GetCredential(string path)
        {
            var text = File.ReadAllText(path);

            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);
            if (result.ContainsKey("type"))
            {
                Service = DriveOp.GetServiceAccountCredential(path);
                IsUserAccount = false;
            }
            else
            {
                Service = DriveOp.GetUserAccountCredential(path);
                IsUserAccount = true;
            }

        }


        public void FromJson()
        {

        }


        public void ToJson()
        {

        }


        public void Enter()
        {

        }

        public void Close(int cum_error)
        {
            CumError = cum_error;
            if (CumError > Var.CumErrorNum || Remain < 2000)
                IsAbandoned = true;
            IsUsing = false;
        }


        public void Close(ServiceCard card)
        {
            CumError = card.CumError + 1;
            OpNum = card.OpNum;
            Remain = card.Remain;
            if (CumError > Var.CumErrorNum || Remain < 2000)
                IsAbandoned = true;
            IsUsing = false;
        }

        // uid
        public string Uid { get; set; }
        // 账号文件路径
        public string JsonPath { get; set; }
        // 账号信息
        public DriveService Service { get; set; }
        // 账号总容量
        public long Usage { get; } = 805306368000;
        // 账号剩余容量
        public long Remain { get; set; } = 805306368000;
        // 剩余过期时间
        public int Expire { get; set; } = 7200;
        // 账号创建时间
        public DateTime Timestamp { get; } = DateTime.Now;
        // 该账号使用次数
        public int OpNum { get; set; } = 0;
        // 是否使用中
        public bool IsUsing { get; set; } = false;
        // 是否已被废弃
        public bool IsAbandoned { get; set; } = false;
        // 累积报错数量
        public int CumError { get; set; } = 0;
        public bool IsUserAccount { get; set; } = false;
    }



    public class SAManage
    {
        public static void ReadSAFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FileNotFound($"{path} is not exist");
            }
            var sa_files = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly).ToList();
            if (sa_files.Count == 0)
            {
                throw new FileNotFound($"could not find any valid service account json file in \'{path}\'");
            }
            sa_files = sa_files.Shuffle();
            var user_files = sa_files.Where(sa => Common.IsUserAccountJson(sa)).ToList();
            SAFiles = new Queue<string>(sa_files);
            UserFiles = new Queue<string>(user_files);
            SAInfos.Clear();
        }


        public static void PreAllocated(int num = 5)
        {
            if (SAFiles.Count == 0)
            {
                throw new ServiceAccountExhausted();
            }
            if (SAFiles.Count < num)
            {
                Var.logger.Warn($"warning: you tried to assign {num} account(sa), but only {SAFiles.Count} account file is available");
                num = SAFiles.Count;
            }


            for (int i = 0; i < num; i++)
            {
                string path = SAFiles.Dequeue();
                SAInfos.Add(new ServiceCard(path));
            }
        }


        public static void PreAllocatedUser(int num = 1)
        {
            if (UserFiles.Count == 0)
            {
                throw new ServiceAccountExhausted();
            }
            if (UserFiles.Count < num)
            {
                Var.logger.Warn($"warning: you tried to assign {num} account(user), but only {UserFiles.Count} account file is available");
                num = UserFiles.Count;
            }


            for (int i = 0; i < num; i++)
            {
                string path = UserFiles.Dequeue();
                SAInfos.Add(new ServiceCard(path));
            }
        }


        public static ServiceCard Request()
        {
            ServiceCard card = null;
            string select_id = null;
            lock (sainfo_locker)
            {
                var valid_sa = SAInfos.Where(sa => !(sa.IsAbandoned || sa.IsUsing)).Select(sa => sa.Uid).ToList();
                if (valid_sa.Count == 0)
                {
                    if (SAFiles.Count != 0)
                    {
                        var path = SAFiles.Dequeue();
                        card = new ServiceCard(path);
                        card.IsUsing = true;
                        SAInfos.Add(card);
                    }
                }
                else
                {
                    select_id = valid_sa[0];
                    int index = SAInfos.FindIndex(sa => (sa.Uid == select_id));
                    SAInfos[index].IsUsing = true;
                    card = SAInfos[index];
                }
            }

            if (card == null)
                throw new ServiceAccountExhausted();

            return card;
        }


        public static void Recycle(ServiceCard card)
        {
            int index = SAInfos.FindIndex(sa => (sa.Uid == card.Uid));
            SAInfos[index].Close(card);
        }

        public static Queue<string> SAFiles;
        public static Queue<string> UserFiles;
        public static List<ServiceCard> SAInfos = new List<ServiceCard>();
        public static object sainfo_locker = new Object();
    }
}
