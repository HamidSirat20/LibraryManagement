using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces
{
    public interface IPublishersMapper
    {
        PublisherReadDto ToPublisherReadDto(Publisher publisher);
        PublisherCreateDto ToPublisherCreateDto(Publisher publisher);
        PublisherUpdateDto ToPublisherUpdateDto(Publisher publisher);
        Publisher ToPublisher(PublisherCreateDto publisherCreateDto);
        Publisher UpdateFromDto(Publisher publisher, PublisherUpdateDto publisherUpdateDto);
    }
}