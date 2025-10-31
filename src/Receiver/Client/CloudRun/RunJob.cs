namespace Amaurot.Receiver.Client.CloudRun;

internal static partial class CloudRunClient
{
    private static readonly string Processor =
        Environment.GetEnvironmentVariable("AMAUROT_PROCESSOR")
        ?? throw new InvalidOperationException("AMAUROT_PROCESSOR is null");

    internal static async Task RunJob(
        string repo,
        long number,
        string headCommit,
        string mergeCommit,
        long installationId
    )
    {
        var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Content = JsonContent.Create(
                    new RunJobWithOverrides
                    {
                        Overrides = new Overrides
                        {
                            ContainerOverrides =
                            [
                                new ContainerOverride
                                {
                                    Args =
                                    [
                                        repo,
                                        number.ToString(),
                                        headCommit,
                                        mergeCommit,
                                        installationId.ToString(),
                                    ],
                                },
                            ],
                            TaskCount = 1,
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

    internal sealed class RunJobWithOverrides
    {
        public required Overrides Overrides { get; init; }
    }

    internal sealed class Overrides
    {
        public required ContainerOverride[] ContainerOverrides { get; init; }
        public required ushort TaskCount { get; init; }
    }

    internal sealed class ContainerOverride
    {
        public required string[] Args { get; init; }
    }
}
