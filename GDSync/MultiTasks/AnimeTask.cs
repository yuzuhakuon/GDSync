using GDSync.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDSync.MultiTasks
{
    class AnimeTask
    {
        public static void LocalImage(string src_uid, int num_worker = 1)
        {

        }



        public static void AddSeason(IEnumerable<string> src_uid_list, string season_count, int num_worker = 1)
        {
            Common.SetMode(Mode.OnlyList);

            var publisher = Common.CreateWorkers(num_worker);
            foreach (var src_uid in src_uid_list)
            {
                publisher.AddAction(new ListCurrentFolderAction(src_uid));
            }
            publisher.WaitForStageOver();

            foreach (var src_uid in src_uid_list)
            {
                publisher.AddAction(new AddSeasonAction(src_uid, season_count));
            }


            publisher.WaitForAllIdle();
        }
    }
}

