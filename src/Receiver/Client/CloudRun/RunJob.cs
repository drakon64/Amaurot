using System.Text;
using System.Text.Json;

using Amaurot.Receiver.SourceGenerationContext;

namespace Amaurot.Receiver.Client.CloudRun;

internal static partial class CloudRunClient
{
    private static readonly string Processor =
        Environment.GetEnvironmentVariable("AMAUROT_PROCESSOR")
        ?? throw new InvalidOperationException("AMAUROT_PROCESSOR is null");

    internal static async Task RunJob(
        string action,
        string repo,
        long number,
        string[] deployments,
        string headCommit,
        string mergeCommit,
        long installationId
    )
    {
        var containerOverrides = deployments
            .Select(deployment => new ContainerOverride
            {
                Args =
                [
                    Convert.ToBase64String(
                        Encoding.Default.GetBytes(
                            JsonSerializer.Serialize(
                                new Arguments
                                {
                                    Action = action,
                                    Repository = repo,
                                    Number = number,
                                    Deployment = deployment,
                                    HeadCommit = headCommit,
                                    MergeCommit = mergeCommit,
                                    InstallationId = installationId,
                                },
                                SnakeCaseLowerSourceGenerationContext.Default.Arguments
                            )
                        )
                    ),
                ],
            })
            .ToArray();

        using var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Content = JsonContent.Create(
                    new RunJobWithOverrides
                    {
                        Overrides = new Overrides
                        {
                            ContainerOverrides = containerOverrides,
                            TaskCount = containerOverrides.Length,
                        },
                    },
                    CamelCaseSourceGenerationContext.Default.RunJobWithOverrides
                ),
                Headers = { { "Authorization", await GetAccessToken() } },
                RequestUri = new Uri(Processor),
            }
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception();
    }

    internal sealed class Arguments
    {
        public required string Action { get; init; }
        public required string Repository { get; init; }
        public required long Number { get; init; }
        public required string Deployment { get; init; }
        public required string HeadCommit { get; init; }
        public required string MergeCommit { get; init; }
        public required long InstallationId { get; init; }
    }

    internal sealed class RunJobWithOverrides
    {
        public required Overrides Overrides { get; init; }
    }

    internal sealed class Overrides
    {
        public required ContainerOverride[] ContainerOverrides { get; init; }
        public required int TaskCount { get; init; }
    }

    internal sealed class ContainerOverride
    {
        public required string[] Args { get; init; }
    }
}
