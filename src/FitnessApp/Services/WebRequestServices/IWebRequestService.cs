using FitnessApp.Constants;
using FitnessApp.Infrastucture.Enumerators;
using FitnessApp.Services.WebRequestServices.Models;
using System.Collections.Specialized;

namespace FitnessApp.Services.WebRequestServices
{
    public interface IWebRequestService
    {
        public HttpResponseModel<T> MakeRequest<T>(
    string endpointUrl,
    HttpVerb httpVerb,
    NameValueCollection queryParameters = null,
    string fragment = null,
    object requestObject = null,
    IDictionary<string, string> additionalHeaders = null,
    string token = null,
    string contentType = ContentTypes.ApplicationJson,
    int timeout = ValuesConstants.ApiTimeout);
    }
}
