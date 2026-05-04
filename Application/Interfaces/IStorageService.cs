using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads an image file to cloud storage
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <param name="fileName">Optional custom file name (will still get unique prefix)</param>
        /// <returns>The full URL of the uploaded image</returns>
        Task<string> UploadImageAsync(IFormFile file, string? fileName = null);

        /// <summary>
        /// Deletes an image from cloud storage
        /// </summary>
        /// <param name="imageUrl">The full URL of the image to delete</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteImageAsync(string imageUrl);

        /// <summary>
        /// Gets the full URL for an image by its filename
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The full URL</returns>
        string GetImageUrl(string fileName);
    }
}
