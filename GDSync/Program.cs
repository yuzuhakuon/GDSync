using CommandLine;
using GDSync.core;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.IO;

namespace GDSync
{
    partial class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log.config"));

            if (!Directory.Exists(Var.OutputDir))
                Directory.CreateDirectory(Var.OutputDir);
            if (!Directory.Exists(Var.LogDir))
                Directory.CreateDirectory(Var.LogDir);

            //var service = DriveOp.GetServiceAccountCredential(@"D:\Code\personal\GDSync\GDSync\bin\Debug\account\sa\5db349b5239a0df6ec27472e2cfc0dcbc40f3168.json");
            //DriveOp.Move(service, "1VOO-BA09rpd83EWEKYakkvMuDqm_-5CL", "1VgSPBWzPA56N82UXJNywkkBOr7i3m-lg", "1NsKygblCLNYvp3uymoO_zdUyiGRK6YPN");



            var exitCode = -1;
            exitCode = Parser.Default.ParseArguments<DisplayOptions, SaveOptions, SyncOptions, ListCurrentOptions, RenameOptions, CheckMissOptions, AnimeSeasonOptions>(args)
            .MapResult(
                (DisplayOptions o) => DisplaySolution(o),
                (ListCurrentOptions o) => ListCurrentSolution(o),
                (SaveOptions o) => SaveSolution(o),
                (SyncOptions o) => SyncSolution(o),
                (RenameOptions o) => RenameSolution(o),
                (CheckMissOptions o) => CheckMissSolution(o),
                (AnimeSeasonOptions o) => AnimeSeasonSolution(o),
                error => 1);


            Console.WriteLine($"\nExitCode: {exitCode}");
            Console.WriteLine("All Finished!");
        }
    }
}
