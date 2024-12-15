using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Primitives;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var clientId = "xxxxxxxxxxxxxxxxx";
var clientSecret = "xxxxxxxxxxxxxxxxx";

app.MapGet("/get_token", (string verificationCode) =>
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.yandex.ru/token");
        var collection = new List<KeyValuePair<string, string>>();
        collection.Add(new("grant_type", "authorization_code"));
        collection.Add(new("code", verificationCode));
        collection.Add(new("client_id", clientId));
        collection.Add(new("client_secret", clientSecret));
        var content = new FormUrlEncodedContent(collection);
        request.Content = content;
        var response = client.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    })
    .WithOpenApi();

app.MapGet("/authorize", () =>
    {
        return new Uri($"https://oauth.yandex.ru/authorize?response_type=code&client_id={clientId}");
    })
    .WithOpenApi();

app.MapGet("/whoami", (string accessToken) =>
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://login.yandex.ru/info?&format=json");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = client.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    })
    .WithOpenApi();

app.MapGet("/doc", () =>
    {
        return new StringBuilder()
            .Append("Вот ссылка на YandexId \n")
            .Append("https://oauth.yandex.ru/ \n")
            .Append("Чтобы это заработало - надо:\n")
            .Append("1 - Создать тут новое приложение: https://oauth.yandex.ru/client/new/id \n")
            .Append("2 - Скопировать clientId и clientSecret своего приложения на строки 23 и 24 этого приложения \n")
            .Append("3 - Пройти по ссылке из эндпоинта /authorize авторизоваться яндексом и забрать ключ (он работает 10 минут, после чего обновится) - вот документация на эндпоинт https://yandex.ru/dev/id/doc/ru/codes/code-url \n")
            .Append("4 - Используя ключ из /authorize забрать AccessToken из /get_token документация тоже тут: https://yandex.ru/dev/id/doc/ru/codes/code-url \n")
            .Append("5 - Используя AccessToken из /get_token можно получить клеймы пользователя по эндпоинту /whoami и по ним строить авторизацию в вашем приложении https://yandex.ru/dev/id/doc/ru/user-information !!! таже тут можно вместо json получить JWT\n")
            .Append("6 - Обновить AccessToken можно используя refresh_token: https://yandex.ru/dev/id/doc/ru/tokens/refresh-client \n")

            .ToString();
    })
    .WithOpenApi();


app.Run();
