using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace LibraryManagement.WebAPI.ActionConstraints;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
{
    private readonly string _requestHeaderToMatch;
    private readonly MediaTypeCollection _mediaTypes = new MediaTypeCollection();

    public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
        string mediaType, params string[] otherMediaTypes)
    {
        _requestHeaderToMatch = requestHeaderToMatch
            ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

        if (MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue? parsedMediaType))
        {
            _mediaTypes.Add(parsedMediaType);
        }
        else
        {
            throw new ArgumentException($"Invalid media type: {mediaType}", nameof(mediaType));
        }


        foreach (var otherMediaType in otherMediaTypes)
        {
            if (MediaTypeHeaderValue.TryParse(otherMediaType, out MediaTypeHeaderValue? parsedOtherMediaType))
            {
                _mediaTypes.Add(parsedOtherMediaType);
            }
            else
            {
                throw new ArgumentException($"Invalid media type: {otherMediaType}", nameof(otherMediaTypes));
            }
        }
    }

    public int Order => 0;

    public bool Accept(ActionConstraintContext context)
    {
        var requestHeaders = context.RouteContext.HttpContext.Request.Headers;

        if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
        {
            return false;
        }

        var headerValue = requestHeaders[_requestHeaderToMatch].ToString();

        if (string.IsNullOrEmpty(headerValue))
        {
            return false;
        }

        foreach (var mediaType in _mediaTypes)
        {
            if (headerValue.Contains(mediaType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
