using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.MapPost("/", async Task<IResult> (HttpRequest request, string? tamanho, bool? paisagem) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest();

    var form = await request.ReadFormAsync();
    var file = form.Files.First();

    var ticketId = await SendMessage(file.FileName);
    return Results.Ok(ticketId);
});

app.Run();

async Task<Guid> SendMessage(string fileName)
{
    var factory = new ConnectionFactory()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "user_mq",
        Password = "RabbitMQ2019!"
    };
    var queue = "FileUpload.Sent";
    var routingKey = "FileUpload.Sent";
    var ticketId = Guid.NewGuid();
    fileName = $@"{ticketId.ToString().Substring(10)}-{fileName}";

    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: queue,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        var bodyAsString = $"TicketId: {ticketId} / Arquivo: {fileName}";
        var body = Encoding.UTF8.GetBytes(bodyAsString);
        channel.BasicPublish(exchange: "",
                             routingKey: routingKey,
                             basicProperties: null,
                             body: body);
    }
    return ticketId;
}
