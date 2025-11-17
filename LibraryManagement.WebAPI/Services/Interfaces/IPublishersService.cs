using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IPublishersService
{
    Task<IEnumerable<PublisherReadDto>> ListAllPublisherAsync(QueryOptions queryOptions);
    Task<Publisher?> GetPublisherByIdAsync(Guid id);
    Task<Publisher?> GetPublisherByEmailAsync(string email);
    Task<Publisher?> CreatePublisherAsync(PublisherCreateDto publisherCreateDto);
    Task<Publisher> UpdatePublisherAsync(Guid id, PublisherUpdateDto publisherUpdateDto);
    Task<Publisher> DeletePublisherAsync(Guid id);
    Task<bool> EntityExistAsync(Guid id);
}
