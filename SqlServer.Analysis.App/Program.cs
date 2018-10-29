// ***********************************************************************
// Assembly         : SqlServer.ExecutionPlan.Analysis.App
// Author           : tdcart
// Created          : 06-17-2016
//
// Last Modified By : tdcart
// Last Modified On : 06-17-2016
// ***********************************************************************
// <copyright file="Program.cs" company="Tim Cartwright">
//     Copyright © Tim Cartwright 2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;
using SqlServer.ExecutionPlan.Analysis.App.Properties;

namespace SqlServer.ExecutionPlan.Analysis.App
{
    /// <summary>
    /// Class Program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentException">Directory not found.</exception>
        [STAThread]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sqlplan"),
            System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Tim Cartwright"),
            System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SqlServer")]
        static void Main(string[] args)
        {
            string dir;
            var openDir = false;

            var parser = new Parser((settings) =>
            {
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Error;
                settings.IgnoreUnknownArguments = true;
            });

            var options = new CmdLineOptions();
            if (!parser.ParseArguments(args, options))
            {
                Console.WriteLine(HelpText.AutoBuild(options).ToString());
                return;
            }

            dir = options.PlanDirectory.Trim('\\', '"');

            if (!Directory.Exists(dir))
            {
                Console.WriteLine(Resources.Program_Main_Could_not_find_the_supplied_directory____0, dir);
                Console.WriteLine(Resources.Program_Main_Continue);
                Console.ReadKey(false);
                return;
            }

            var files = Directory.EnumerateFiles(dir, "*.sqlplan").ToList();

            if (files.Count == 0)
            {
                Console.WriteLine(Resources.Program_Main_No_execution_plan_files_found_at___0, dir);
                Console.WriteLine(Resources.Program_Main_Continue);
                Console.ReadKey(false);
                return;
            }


            try
            {
                Console.WriteLine(Resources.Program_Main_Validating);
                var sw = Stopwatch.StartNew();

                var planValidator = new PlanValidator();

                var writeXml = options.OutPutTypes == OutPutType.Both || options.OutPutTypes == OutPutType.Xml;
                var writeHtml = options.OutPutTypes == OutPutType.Both || options.OutPutTypes == OutPutType.Html;

                if (writeHtml)
                {
                    PlanTransformer.ExtractResources(dir);
                }

                foreach (var file in files)
                {
                    var planXml = File.ReadAllText(file);
                    var results = planValidator.ValidateSqlPlan(planXml).ToList();

                    var xmlFile = $"{file}.analysis.xml";
                    var htmlFile = $"{file}.analysis.html";

                    //first delete any old files in case this is a re-run
                    if (File.Exists(xmlFile)) { File.Delete(xmlFile); }
                    if (File.Exists(htmlFile)) { File.Delete(htmlFile); }

                    var hasChecks = results.Any(r => r.Result != PlanResult.StatementCost && r.Category != PlanCategory.Trace);
                    if (!hasChecks)
                    {
                        Console.WriteLine($"No broken checks for: {Path.GetFileName(file)}");
                        continue;
                    }

                    if (writeXml)
                    {
                        var resultElement = planValidator.GenerateResultsElement(results);
                        File.WriteAllText(xmlFile, resultElement.ToString());
                    }
                    if (writeHtml)
                    {
                        var planAnalysisXml = planValidator.UpdatePlanWithAnalysis(planXml, results);
                        File.WriteAllText(htmlFile, PlanTransformer.TransformHtmlPage(file, planAnalysisXml));
                    }

                    Console.WriteLine(Resources.Program_Main_Validated_File, Path.GetFileName(file));
                }

                sw.Stop();
                Console.WriteLine(Resources.Program_Main_Validated, files.Count(), sw.Elapsed.TotalSeconds);

                //they browsed to the directory, so lets open it for them
                if (openDir) { Process.Start(dir); }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.WriteLine();
                Console.ResetColor();
                Console.WriteLine(Resources.Program_Main_Continue);
                Console.ReadKey(false);
            }

            if (!Debugger.IsAttached) return;

            Console.WriteLine(Resources.Program_Main_Continue);
            Console.ReadKey(false);
        }
    }
}
