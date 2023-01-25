namespace FitnessApp.Services.WebRequestServices.Models
{
    using FitnessApp.Constants;
    using FitnessApp.Infrastucture.Enumerators;

    public sealed class HeadersServiceModel
    {
        public string Token { get; set; }

        public HttpVerb RequestMethod { get; set; } = HttpVerb.GET;

        public string ContentType { get; set; } = ContentTypes.ApplicationJson;

        public int ContentLength { get; set; }

        public int Timeout { get; set; } = ValuesConstants.ApiTimeout;

        public string Accept { get; set; } = ContentTypes.ApplicationJson;

        public IDictionary<string, string> AdditionalHeaders { get; set; } = new Dictionary<string, string>();
    }
}
