using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GDSync.core
{
    public class Adventurer
    {
        public Adventurer(string name = null)
        {
            if (name == null)
            {
                name = System.Guid.NewGuid().ToString();
            }
            int id = Thread.CurrentThread.ManagedThreadId;


            Name = $"{id}-{name}";
            IsExit = false;
            IsBusy = false;
            publisher = null;
            card = null;
        }


        /// <summary>
        /// 向Guild注册自身
        /// </summary>
        /// <param name="publisher">Guild实例</param>
        public void Register(ref Guild publisher)
        {
            if (!(this.publisher == null))
            {
                this.UnRegister();
            }
            this.publisher = publisher;
            this.publisher.AddAdventurer(this);
            this.IsExit = false;
        }


        /// <summary>
        /// 注销身份
        /// </summary>
        public void UnRegister()
        {
            this.publisher = null;
            this.publisher.RemoveAdventurer(Name);
            this.IsExit = true;
        }


        /// <summary>
        /// 申请身份认证信息
        /// </summary>
        public void ObtainCertificate()
        {
            card = SAManage.Request();
        }


        /// <summary>
        /// 执行任务
        /// </summary>
        public void Start()
        {
            ObtainCertificate();
            while (!IsExit)
            {
                if (IsPause)
                {
                    Thread.Sleep(500);
                    continue;
                }


                var action = publisher.GetDriveAction(out bool success);
                if (!success)
                {
                    Thread.Sleep(1000);
                    continue;
                }


                IsBusy = true;
                try
                {
                    card.OpNum++;
                    Var.logger.Info($"(worker id: {Thread.CurrentThread.ManagedThreadId}) Execute: {action.GetTypeName()}");
                    action.Execute(card);
                    this.publisher.ReportDriveActions(action.CreateNewActions());

                }
                catch (Exception e)
                {
                    IsBusy = false;
                    Var.logger.Error($"(worker id: {Thread.CurrentThread.ManagedThreadId}) {action.GetTypeName()}\n{e}");
                    if (e is Google.GoogleApiException)
                    {
                        if (Common.IsStorageLimited((Google.GoogleApiException)e)) action.IncreaseUsageLimitError();
                    }
                    this.publisher.AddAction(action);
                    SAManage.Recycle(card);
                    card = SAManage.Request();
                }
                IsBusy = false;
            }
            SAManage.Recycle(card);
        }


        public void Pause(bool status)
        {
            IsPause = status;
        }


        /// <summary>
        /// 设置忙碌状态
        /// </summary>
        /// <param name="status"></param>
        public void SetBusy(bool status = true)
        {
            IsBusy = status;
        }


        public string Name { get; set; }
        public bool IsExit { get; set; }
        public bool IsBusy { get; set; }
        public bool IsPause { get; set; }
        public bool Status { get; set; }


        private Guild publisher;
        private ServiceCard card;
    }
}
