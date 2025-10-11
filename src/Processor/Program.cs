using System.CommandLine;

var plan = new Command("plan", "Perform an OpenTofu plan run");

var path = new Option<DirectoryInfo>("--path", "-p")
{
    Description =
        "The subdirectory in the repository to perform the plan run in. Defaults to the repository root.",
};

var varsFile = new Option<FileInfo[]>("--vars-file")
{
    Description =
        "Path to a vars file to be used during the plan run. Can be specified multiple times.",
};

plan.Options.Add(path);
plan.Options.Add(varsFile);

var apply = new Command("apply", "Perform an OpenTofu apply run");

plan.SetAction(parseResult =>
{
    foreach (var file in parseResult.GetValue(varsFile))
    {
        foreach (var line in File.ReadLines(file.FullName))
        {
            Console.WriteLine(line);
        }
    }

    return 0;
});

var rootCommand = new RootCommand("Sample app for System.CommandLine");

var repository = new Argument<string>("repository")
{
    Description = "The GitHub repository to pull from",
};

var pullRequest = new Argument<ulong>("pull request")
{
    Description = "The pull request number to comment on",
};

var commit = new Argument<string>("commit")
{
    Description = "The commit hash to perform a run against",
};

rootCommand.Arguments.Add(repository);
rootCommand.Arguments.Add(pullRequest);
rootCommand.Arguments.Add(commit);
rootCommand.Subcommands.Add(plan);
rootCommand.Subcommands.Add(apply);

return rootCommand.Parse(args).Invoke();
