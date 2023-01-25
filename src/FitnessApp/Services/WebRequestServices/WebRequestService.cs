namespace FitnessApp.Services.WebRequestServices
{
    using FitnessApp.Constants;
    using FitnessApp.Infrastucture.Enumerators;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Web;
    using WebRequestServices.Models;

    public class WebRequestService : IWebRequestService
    {
        private readonly HttpRequestModel httpRequestModel;
        private string requestData;

        public WebRequestService()
        {
            this.httpRequestModel = new HttpRequestModel();
            this.requestData = string.Empty;
        }

        public HttpResponseModel<T> MakeRequest<T>(
            string endpointUrl,
            HttpVerb httpVerb,
            NameValueCollection queryParameters = null,
            string fragment = null,
            object requestObject = null,
            IDictionary<string, string> additionalHeaders = null,
            string token = null,
            string contentType = ContentTypes.ApplicationJson,
            int timeout = ValuesConstants.ApiTimeout)
        {
            this.SetUrl(endpointUrl, queryParameters, fragment);

            this.SetRequestData(requestObject, contentType);
            this.SetHeaders(httpVerb,contentType,timeout, additionalHeaders, token);

            HttpResponseModel<T> response = this.MakeWebRequest<T>();

            return response;
        }

        private void SetRequestData<T>(T requestObject, string contentType)
        {
            if (requestObject == null)
            {
                return;
            }

            if (contentType == ContentTypes.ApplicationJson)
            {
                this.requestData = JsonSerializer.Serialize(requestObject);
            }
            else if (contentType == ContentTypes.ApplicationWwwFormUrlEncoded)
            {
                this.requestData = requestObject.ToString();
            }

            byte[] requestBytes = Encoding.UTF8.GetBytes(this.requestData);
            this.httpRequestModel.RequestBytes = requestBytes;
        }

        private void SetUrl(string endpointUrl, NameValueCollection queryParameters = null, string fragment = null)
        {
            string url = $"{ValuesConstants.ApiUrl}/{endpointUrl}";

            if (queryParameters != null || fragment != null)
            {
                var uriBuilder =
                    queryParameters != null && fragment == null ?
                    new UriBuilder(url) { Query = GetQueryParameters(queryParameters) }
                    : queryParameters == null && fragment != null ?
                    new UriBuilder(url) { Fragment = fragment }
                    : new UriBuilder(url) { Query = GetQueryParameters(queryParameters), Fragment = fragment };

                url = uriBuilder.ToString();
            }

            this.httpRequestModel.Url = url;
        }

        private void SetHeaders(HttpVerb httpVerb, string contentType, int timeout, IDictionary<string, string> additionalHeaders, string token)
        {
            HeadersServiceModel headers = new HeadersServiceModel
            {
                RequestMethod = httpVerb,
                ContentType = contentType,
                ContentLength = this.httpRequestModel.RequestBytes != null ? this.httpRequestModel.RequestBytes.Length : 0,
                Timeout = timeout,
                AdditionalHeaders = additionalHeaders,
                Token = token,
            };

            this.httpRequestModel.Headers = headers;
        }

        private string GetQueryParameters(NameValueCollection queryparameters)
        {
            string[] parameters = queryparameters.AllKeys.Select(k => string.Format(
                "{0}={1}",
                HttpUtility.UrlEncode(k),
                HttpUtility.UrlEncode(queryparameters[k]))).ToArray();

            return string.Join("&", parameters);
        }

        private HttpResponseModel<T> MakeWebRequest<T>()
        {
            HttpResponseModel<T> response = new()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.httpRequestModel.Url);
                if (this.httpRequestModel.Headers.RequestMethod != HttpVerb.GET)
                {
                    byte[] requestBytes = this.httpRequestModel.RequestBytes;
                    SetHttpRequestHeaders(request: request, headers: this.httpRequestModel.Headers);

                    using Stream stream = request.GetRequestStream();
                    stream.Write(requestBytes, 0, requestBytes.Length);
                    stream.Close();
                }
                else
                {
                    SetHttpRequestHeaders(request: request, headers: this.httpRequestModel.Headers);
                }

                string content = string.Empty;
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse())
                {
                    response.StatusCode = (int)httpWebResponse.StatusCode;
                    Stream stream = httpWebResponse.GetResponseStream();
                    StreamReader sr = new(stream: stream);
                    content = sr.ReadToEnd();
                }

                response.Model = JsonSerializer.Deserialize<T>(content);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (ex.Response is HttpWebResponse exResponse)
                    {
                        response.StatusCode = (int)exResponse.StatusCode;
                        response.Message = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                }
                else if (ex.Status == WebExceptionStatus.Timeout)
                {
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Message = ex.Message;
                }
                else
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        private static void SetHttpRequestHeaders(HttpWebRequest request, HeadersServiceModel headers)
        {
            request.Method = headers.RequestMethod.ToString();
            request.ContentType = headers.ContentType;
            request.Timeout = headers.Timeout;
            request.Timeout = ValuesConstants.ApiTimeout;

            request.Headers.Add("Authorization", $"Bearer {headers.Token}");

            if (headers.AdditionalHeaders != null && headers.AdditionalHeaders.Any())
            {
                foreach (var header in headers.AdditionalHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            request.Accept = headers.Accept;
            request.KeepAlive = false;
        }
    }
}
