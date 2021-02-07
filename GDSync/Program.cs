using CommandLine;
using GDSync.core;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.IO;

namespace DotnetCore
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


            var exitCode = -1;
            exitCode = Parser.Default.ParseArguments<DisplayOptions, SaveOptions, SyncOptions, ListCurrentOptions, RenameOptions, CheckMissOptions>(args)
            .MapResult(
                (DisplayOptions o) => DisplaySolution(o),
                (ListCurrentOptions o) => ListCurrentSolution(o),
                (SaveOptions o) => SaveSolution(o),
                (SyncOptions o) => SyncSolution(o),
                (RenameOptions o) => RenameSolution(o),
                (CheckMissOptions o) => CheckMissSolution(o),
                error => 1);


            Console.WriteLine($"\nExitCode: {exitCode}");
            Console.WriteLine("All Finished!");
        }
    }
}
