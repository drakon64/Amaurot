using System.Net.Http.Json;
using Amaurot.Firewall.Models;

var ips = await new HttpClient
{
    DefaultRequestHeaders =
    {
        { "User-Agent", "Amaurot" },
        { "Accept", "application/vnd.github+json" },
        { "X-GitHub-Api-Version", "2022-11-28" },
    },
}.GetFromJsonAsync<GitHubMetaInformation>("https://api.github.com/meta");

foreach (var ip in ips!.Hooks)
{
    await Console.Out.WriteLineAsync(ip);
}
