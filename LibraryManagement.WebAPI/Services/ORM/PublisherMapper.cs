using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM
{
    public class PublisherMapper : IPublisherMapper
    {
        public PublisherReadDto ToPublisherReadDto(Publisher publisher)
        {
            if (publisher == null) return null;

            return new PublisherReadDto
            {
                Id = publisher.Id,
                Name = publisher.Name,
                Address = publisher.Address,
                Email = publisher.Email,
                Website = publisher.Website,
                //should be set in service
                //BookCount = publisher.Books?.Count
            };
        }

        public PublisherCreateDto ToPublisherCreateDto(Publisher publisher)
        {
            if (publisher == null) return null;

            return new PublisherCreateDto
            {
                Name = publisher.Name,
                Address = publisher.Address,
                Email = publisher.Email,
                Website = publisher.Website
            };
        }

        public PublisherUpdateDto ToPublisherUpdateDto(Publisher publisher)
        {
            if (publisher == null) return null;

            return new PublisherUpdateDto
            {
                Name = publisher.Name,
                Address = publisher.Address,
                Email = publisher.Email,
                Website = publisher.Website
            };
        }

        public Publisher ToPublisher(PublisherCreateDto publisherCreateDto)
        {
            if (publisherCreateDto == null) return null;

            return new Publisher(
                publisherCreateDto.Name,
                publisherCreateDto.Address,
                publisherCreateDto.Website,
                publisherCreateDto.Email
            );
        }

        public Publisher UpdateFromDto(Publisher publisher, PublisherUpdateDto publisherUpdateDto)
        {
            if (publisherUpdateDto == null) return publisher;

            if (!string.IsNullOrEmpty(publisherUpdateDto.Name))
                publisher.Name = publisherUpdateDto.Name;

            if (!string.IsNullOrEmpty(publisherUpdateDto.Address))
                publisher.Address = publisherUpdateDto.Address;

            if (!string.IsNullOrEmpty(publisherUpdateDto.Email))
                publisher.Email = publisherUpdateDto.Email;

            if (!string.IsNullOrEmpty(publisherUpdateDto.Website))
                publisher.Website = publisherUpdateDto.Website;

            publisher.UpdatedAt = DateTime.UtcNow;
            return publisher;
        }
    }
}