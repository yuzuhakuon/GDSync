using CommandLine;
using GDSync.core;
using GDSync.MultiTasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDSync
{
    [Verb("anime-season", HelpText = "添加季度文件夹")]
    class AnimeSeasonOptions
    {
        [Option('s', "src", Required = true, HelpText = "源文件夹id")]
        public IEnumerable<string> SrcIds { get; set; }
        [Option('c', "season-count", Required = false, HelpText = "季度序号")]
        public string SeasonCount { get; set; } = "1";
        [Option('n', "num-worker", Required = false, HelpText = "工作线程")]
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
        private static int AnimeSeasonSolution(AnimeSeasonOptions options)
        {
            var src_uid_list = options.SrcIds;
            var season_count = options.SeasonCount;
            var num = options.Num;
            var account_dir = options.AccountDir;


            SAManage.ReadSAFiles(account_dir);
            SAManage.PreAllocated(num);

            AnimeTask.AddSeason(src_uid_list, season_count, num);


            return 0;
        }
    }
}
