using System.Collections.Concurrent;
using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly bool _useSharedAccessSignature;

        // SAS URLs are valid for 1 year; cache them for 23 h to avoid per-request regeneration
        // while still rotating well before expiry.
        private static readonly TimeSpan SasCacheDuration = TimeSpan.FromHours(23);
        private readonly ConcurrentDictionary<string, (string Url, DateTimeOffset ValidUntil)> _sasCache = new();

        public AzureBlobStorageService(
            IConfiguration configuration,
            ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureStorage:ConnectionString"];
            _containerName = configuration["AzureStorage:ContainerName"] ?? "item-images";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is not configured. " +
                    "Please set 'AzureStorage:ConnectionString' in user secrets or configuration.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure container exists - handle both public and private storage accounts
            try
            {
                _containerClient.CreateIfNotExists(PublicAccessType.Blob);
                _useSharedAccessSignature = false;
                _logger.LogInformation("Azure Blob Storage initialized with container: {ContainerName} (public access)", _containerName);
            }
            catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
            {
                // Storage account has public access disabled - use SAS URLs
                _containerClient.CreateIfNotExists(PublicAccessType.None);
                _useSharedAccessSignature = true;
                _logger.LogWarning("Public access disabled on storage account. Using SAS URLs for image access.");
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string? fileName = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            try
            {
                // Generate unique filename
                var extension = Path.GetExtension(file.FileName);
                var uniqueFileName = string.IsNullOrWhiteSpace(fileName)
                    ? $"{Guid.NewGuid()}{extension}"
                    : $"{Guid.NewGuid()}_{fileName}";

                var blobClient = _containerClient.GetBlobClient(uniqueFileName);

                // Upload with content type
                var blobHttpHeaders = new BlobHttpHeaders 
                { 
                    ContentType = file.ContentType ?? "image/jpeg",
                    CacheControl = "public, max-age=31536000" // Cache for 1 year
                };

                await using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

                _logger.LogInformation("Image uploaded successfully: {FileName} -> {BlobName}", file.FileName, uniqueFileName);

                // Return only the filename (not full URL) for dynamic SAS generation
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return false;
            }

            try
            {
                string fileName;

                // Check if it's just a filename or a full URL
                if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract filename from full URL (remove SAS token if present)
                    var uri = new Uri(imageUrl);
                    fileName = Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    // Already just a filename
                    fileName = imageUrl;
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    _logger.LogWarning("Could not extract filename from: {Input}", imageUrl);
                    return false;
                }

                var blobClient = _containerClient.GetBlobClient(fileName);
                var result = await blobClient.DeleteIfExistsAsync();

                // Evict cached SAS URL so a deleted blob is never served from cache
                _sasCache.TryRemove(fileName, out _);

                if (result.Value)
                {
                    _logger.LogInformation("Image deleted successfully: {FileName}", fileName);
                }
                else
                {
                    _logger.LogWarning("Image not found for deletion: {FileName}", fileName);
                }

                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {Input}", imageUrl);
                return false;
            }
        }

        public string GetImageUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            if (!_useSharedAccessSignature)
            {
                return _containerClient.GetBlobClient(fileName).Uri.ToString();
            }

            var now = DateTimeOffset.UtcNow;

            if (_sasCache.TryGetValue(fileName, out var cached) && cached.ValidUntil > now)
            {
                return cached.Url;
            }

            var blobClient = _containerClient.GetBlobClient(fileName);
            var url = GenerateSasUrl(blobClient);
            _sasCache[fileName] = (url, now.Add(SasCacheDuration));
            return url;
        }

        /// <summary>
        /// Generates a SAS URL with read permissions valid for 1 year
        /// </summary>
        private string GenerateSasUrl(BlobClient blobClient)
        {
            // Check if the client can generate SAS token
            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogWarning("BlobClient cannot generate SAS URI. Returning base URI.");
                return blobClient.Uri.ToString();
            }

            // Create SAS token that's valid for 1 year
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobClient.Name,
                Resource = "b", // b = blob
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Allow 5 min clock skew
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1) // Valid for 1 year
            };

            // Set permissions - read only
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the SAS URI
            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }
    }
}
