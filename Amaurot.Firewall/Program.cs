using System.Net.Http.Json;
using Amaurot.Firewall.Models;

var ips = await new HttpClient
{
    DefaultRequestHeaders = { { "User-Agent", "Amaurot" } },
}.GetFromJsonAsync<GitHubMetaInformation>("https://api.github.com/meta");

foreach (var ip in ips!.Hooks)
{
    await Console.Out.WriteLineAsync(ip);
}
