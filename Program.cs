using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pastel;

try
{
    // Arguments
    var url = args.FirstOrDefault();
    var clientId = args.Skip(1)?.FirstOrDefault();
    var secret = args.Skip(2)?.FirstOrDefault();
    bool printPayload = args.Skip(3)?.FirstOrDefault()?.Equals("--decode") ?? false;
    bool printPayloadOnly = args.Skip(3)?.FirstOrDefault()?.Equals("--decodeonly") ?? false;

    if (url is null ||
        clientId is null ||
        secret is null)
    {
        Console.WriteLine("Usage: token.exe <token_url> <client_id> <secret> [options...]");
        Console.WriteLine("--decode, print the decoded payload");
        Console.WriteLine("--decodeonly, print only the decoded payload");
        Environment.Exit(1);
    }

    // Setup request
    var client = new HttpClient();
    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
        })
    };
    tokenRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(clientId + ":" + secret)));

    // Create tasks - accessToken and progress-spinner
    var accessTokenTask = Task.Run(async () =>
    {
        using var response = await client.SendAsync(tokenRequest);
        response.EnsureSuccessStatusCode();
        var str = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OAuthResponse>(str).access_token;
    });
    var spinnerTask = Task.Run(async () =>
    {
        Console.CursorVisible = false;
        int i = 0;

        while (!accessTokenTask.IsCompleted)
        {
            switch (i++ % 4)
            {
                case 0: Console.Write("\r|"); break;
                case 1: Console.Write("\r/"); break;
                case 2: Console.Write("\r-"); break;
                case 3: Console.Write("\r\\"); break;
            }
            await Task.Delay(100);
        }
        Console.Write("\r");
        Console.CursorVisible = true;
    });

    // Wait...
    await Task.WhenAll(new List<Task> { spinnerTask, accessTokenTask });

    // Print token?
    var accessToken = accessTokenTask.Result;
    var parts = accessToken.Split('.');

    if (!printPayloadOnly)
    {
        Console.WriteLine(
            parts[0].Pastel(Color.FromArgb(251, 1, 91)) + "." +
            parts[1].Pastel(Color.FromArgb(214, 58, 255)) + "." +
            parts[2].Pastel(Color.FromArgb(0, 185, 241)));

        if (printPayload)
        {
            Console.WriteLine();
        }
    }

    // Print payload?
    if (printPayload || printPayloadOnly)
    {
        var payload = parts[1].Replace('_', '/').Replace('-', '+');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        var payloadDecoded = Encoding.Default.GetString(Convert.FromBase64String(payload));
        var payloadPretty = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<JsonElement>(payloadDecoded),
            new JsonSerializerOptions { WriteIndented = true }
        );
        Console.WriteLine(payloadPretty.Pastel(Color.FromArgb(214, 58, 255)));
    }

    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine("Whoops: " + ex.ToString());
    Environment.Exit(1);
}