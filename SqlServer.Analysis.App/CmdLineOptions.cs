using CommandLine;

namespace SqlServer.ExecutionPlan.Analysis.App
{
    public enum OutPutType
    {
        Both,
        Html,
        Xml
    }

    /// <summary>
    /// 
    /// </summary>
    class CmdLineOptions
    {
        [Option('d', "dir", Required = true, HelpText = "The path to the directory containing the plans to analyze.")]
        public string PlanDirectory { get; set; }

        [Option('o', "outPutType", Required = false, HelpText = "The type of files to output. Possible values: Both | Html | Xml", DefaultValue = OutPutType.Both)]
        public OutPutType OutPutTypes { get; set; }
    }

}
