namespace FitnessApp.Services.WebRequestServices.Models
{
    public class HttpRequestModel
    {
        public string Url { get; set; }

        public object RequestData { get; set; }

        public byte[] RequestBytes { get; set; }

        public HeadersServiceModel Headers { get; set; }
    }
}
