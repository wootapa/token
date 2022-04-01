using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

try
{
    var url = args.FirstOrDefault();
    var clientId = args.Skip(1)?.FirstOrDefault();
    var secret = args.Skip(2)?.FirstOrDefault();

    if (url is null ||
        clientId is null ||
        secret is null)
    {
        Console.WriteLine("Usage: token.exe <token_url> <client_id> <secret>");
        Environment.Exit(1);
    }

    var client = new HttpClient();
    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
        })
    };
    tokenRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(clientId + ":" + secret)));

    using var responseMessage = client.Send(tokenRequest);
    var response = responseMessage.Content.ReadAsStream();
    var accessToken = JsonSerializer.Deserialize<OAuthResponse>(response).access_token;

    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(accessToken);
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine("Whoops: " + ex);
    Environment.Exit(1);
}