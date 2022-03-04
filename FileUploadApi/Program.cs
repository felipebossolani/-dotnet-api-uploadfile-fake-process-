using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.MapPost("/files", async Task<IResult> (HttpRequest request, string? tamanho, bool? paisagem) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest();

    var form = await request.ReadFormAsync();
    var file = form.Files.First();

    var ticketId = Guid.NewGuid();
    var fileName = $@"{ticketId.ToString().Substring(10)}-{file.FileName}";

    return Results.Ok(ticketId);
});

app.MapPost("/", async
    ([FromForm(Name = "file")] IFormFile file) =>
{
    if (file == null) throw new ArgumentNullException(nameof(IFormFile), "Informe o arquivo");

    var tickerId = Guid.NewGuid();
    var fileName = $@"{tickerId.ToString().Substring(10)}-{file.FileName}";

    return Results.Created("", tickerId);
})
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();
