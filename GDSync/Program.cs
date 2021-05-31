using CommandLine;
using GDSync.core;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace GDSync
{
    partial class Program
    {
        static void Main(string[] args)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(100);
            //        Console.WriteLine("----------");
            //        if (Console.KeyAvailable)
            //        {
            //            ConsoleKeyInfo key = Console.ReadKey(true);
            //            Console.WriteLine(key.Key);
            //        }
            //    }
            //});
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log.config"));

            if (!Directory.Exists(Var.OutputDir))
                Directory.CreateDirectory(Var.OutputDir);
            if (!Directory.Exists(Var.LogDir))
                Directory.CreateDirectory(Var.LogDir);


            var exitCode = -1;
            exitCode = Parser.Default.ParseArguments<DisplayOptions, SaveOptions,
                SyncOptions, ListCurrentOptions, RenameOptions, CheckMissOptions,
                SameInfoOptions, AnimeSeasonOptions>(args)
            .MapResult(
                (DisplayOptions o) => DisplaySolution(o),
                (ListCurrentOptions o) => ListCurrentSolution(o),
                (SaveOptions o) => SaveSolution(o),
                (SyncOptions o) => SyncSolution(o),
                (RenameOptions o) => RenameSolution(o),
                (CheckMissOptions o) => CheckMissSolution(o),
                (SameInfoOptions o) => SameInfoSolotion(o),
                (AnimeSeasonOptions o) => AnimeSeasonSolution(o),
                error => 1);


            Console.WriteLine($"\nExitCode: {exitCode}");
            Console.WriteLine("All Finished!");
            //Console.ReadLine();
        }
    }
}
