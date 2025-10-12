using System.CommandLine;

var repository = new Argument<string>("repository") { Description = "The repository to pull from" };

var pullRequest = new Argument<ulong>("pull request")
{
    Description = "The pull request number to comment on",
};

var commit = new Argument<string>("commit")
{
    Description = "The commit hash to perform a run against",
};

var plan = new Command("plan", "Perform an OpenTofu plan run");
var apply = new Command("apply", "Perform an OpenTofu apply run");

var path = new Option<string>("--path", "-p")
{
    Description = "The subdirectory in the repository to perform the run in",
};

var varsFile = new Option<string[]>("--vars-file")
{
    Description = "Path to a vars file to be used during the run",
};

var rootCommand = new RootCommand("Sample app for System.CommandLine");
rootCommand.Arguments.Add(repository);
rootCommand.Arguments.Add(pullRequest);
rootCommand.Arguments.Add(commit);
rootCommand.Subcommands.Add(plan);
rootCommand.Subcommands.Add(apply);
rootCommand.Options.Add(path);
rootCommand.Options.Add(varsFile);

return rootCommand.Parse(args).Invoke();
