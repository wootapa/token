using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

try
{
    // Arguments
    var url = args.FirstOrDefault();
    var clientId = args.Skip(1)?.FirstOrDefault();
    var secret = args.Skip(2)?.FirstOrDefault();
    bool printPayload = args.Skip(3)?.FirstOrDefault()?.Equals("--decode") ?? false;

    if (url is null ||
        clientId is null ||
        secret is null)
    {
        Console.WriteLine("Usage: token.exe <token_url> <client_id> <secret> --decode (optional)");
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

    // Create tasks - request and progress-spinner
    var task1 = client.SendAsync(tokenRequest);
    var task2 = Task.Run(async () =>
    {
        Console.CursorVisible = false;
        int i = 0;

        while (!task1.IsCompleted)
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
    await Task.WhenAll(new List<Task> { task1, task2 });

    // Print token
    using var response = task1.Result.Content.ReadAsStream();
    var accessToken = JsonSerializer.Deserialize<OAuthResponse>(response).access_token;
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(accessToken);

    // Print payload?
    if (printPayload)
    {
        var payload = accessToken.Split('.')[1].Replace('_', '/').Replace('-', '+');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        var payloadDecoded = Encoding.Default.GetString(Convert.FromBase64String(payload));
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(payloadDecoded);
        var payloadPretty = JsonSerializer.Serialize(jsonElement, options);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(payloadPretty);
    }

    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine("Whoops: " + ex);
    Environment.Exit(1);
}