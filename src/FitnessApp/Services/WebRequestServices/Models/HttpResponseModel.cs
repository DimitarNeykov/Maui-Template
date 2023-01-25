namespace FitnessApp.Services.WebRequestServices.Models
{
    public class HttpResponseModel<T>
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }

        public T Model { get; set; }
    }
}
