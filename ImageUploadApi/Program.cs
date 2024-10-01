using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ImageUploadApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Options>(builder.Configuration.Get<Options>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", async (Options options) =>
{
    BlobContainerClient containerClient = await GetCloudBlobContainer(options);

    BlobClient blobClient;
    BlobSasBuilder blobSasBuilder;

    List<string> results = new List<string>();
    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
    {

        blobClient = containerClient.GetBlobClient(blobItem.Name);
        blobSasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = options.FullImageContainerName,
            BlobName = blobItem.Name,
            ExpiresOn = DateTime.UtcNow.AddMinutes(5), // Let SAS token expire after 5 minutes.
            Protocol = SasProtocol.Https
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read);


        results.Add(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri);

    }
    Console.Out.WriteLine("Got Images");
    return Results.Ok(results);
})
.WithName("GetImages")
.WithOpenApi();

app.MapPost("/", async (Options options, HttpRequest request) =>
{
    Stream image = request.Body;
    BlobContainerClient containerClient = await GetCloudBlobContainer(options);
    string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty);
    BlobClient blobClient = containerClient.GetBlobClient(blobName);
    await blobClient.UploadAsync(image);
    return Results.Created(blobClient.Uri, null);
})
.WithName("PostImages")
.WithOpenApi();

app.Run();

async Task<BlobContainerClient> GetCloudBlobContainer(Options options)
{
    BlobServiceClient serviceClient = new BlobServiceClient(options.StorageConnectionString);
    BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(options.FullImageContainerName);
    await containerClient.CreateIfNotExistsAsync();
    return containerClient;
}