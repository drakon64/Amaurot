using System.Net.Http.Json;
using Amaurot.Firewall.Models;
using Google.Cloud.Compute.V1;

var firewall =
    Environment.GetEnvironmentVariable("FIREWALL")
    ?? throw new InvalidOperationException("FIREWALL is null");
var project =
    Environment.GetEnvironmentVariable("PROJECT")
    ?? throw new InvalidOperationException("PROJECT is null");

var gitHubMetaInformation = await new HttpClient
{
    DefaultRequestHeaders =
    {
        { "User-Agent", "Amaurot" },
        { "Accept", "application/vnd.github+json" },
        { "X-GitHub-Api-Version", "2022-11-28" },
    },
}.GetFromJsonAsync<GitHubMetaInformation>("https://api.github.com/meta");

// Remove IPv6 addresses
var ips = gitHubMetaInformation!.Hooks.Where(ip => !ip.Contains(':')).ToArray();

var firewallsClient = await FirewallsClient.CreateAsync();

var oldIps = await firewallsClient.GetAsync(
    new GetFirewallRequest { Firewall = firewall, Project = project }
);

await firewallsClient.PatchAsync(
    new PatchFirewallRequest
    {
        Firewall = firewall,
        FirewallResource = new Firewall
        {
            Allowed =
            {
                new Allowed { IPProtocol = "tcp", Ports = { "443" } },
            },
            Direction = "INGRESS",
            SourceRanges = { ips },
        },
        Project = project,
    }
);

await Console.Out.WriteLineAsync(
    $"""
    Old IP ranges: {string.Join(", ", oldIps.SourceRanges)}
    New IP ranges: {string.Join(", ", ips)}
    """
);
