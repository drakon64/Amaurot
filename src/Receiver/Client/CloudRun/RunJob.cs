namespace Amaurot.Receiver.Client.CloudRun;

internal static partial class CloudRunClient
{
    private static readonly string Processor =
        Environment.GetEnvironmentVariable("AMAUROT_PROCESSOR")
        ?? throw new InvalidOperationException("AMAUROT_PROCESSOR is null");

    internal static async Task RunJob()
    {
        var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Content = JsonContent.Create(
                    new RunJobWithOverrides
                    {
                        Overrides = new Overrides
                        {
                            ContainerOverrides = [new ContainerOverride { Args = [""] }],
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
    }

    internal sealed class ContainerOverride
    {
        public required string[] Args { get; init; }
    }
}
