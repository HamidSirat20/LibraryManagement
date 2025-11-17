using CloudinaryDotNet.Actions;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IImageService
{
    Task<ImageUploadResult> AddImageAsync(IFormFile file);
    Task<DeletionResult> DeleteImageAsync(string publicId);
}
