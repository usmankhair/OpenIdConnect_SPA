using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    /// <summary>
    /// HTTP Client Handler Class
    /// </summary>
    public static class ClientHttpHandler
    {
        private static readonly double DEFAULT_REQUEST_TIMEOUT = 100;   // we can setup this to get from configuration in future

        public static async Task<T> GetAsync<T>(string endPoint, Dictionary<string, string> headers, double? timeOut = null)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                if (timeOut != null)
                {
                    double timoutSpan = (double)timeOut;
                    client.Timeout = TimeSpan.FromSeconds(timoutSpan);
                }
                else
                {
                    double RequestTimeOut = DEFAULT_REQUEST_TIMEOUT;  
                    if (RequestTimeOut > 0)
                    {
                        client.Timeout = TimeSpan.FromSeconds(RequestTimeOut);
                    }
                }
                var taskCompletionSource = new TaskCompletionSource<T>();

                var request = MakeRequest(endPoint, HttpMethod.Get, headers);

                var task = client.SendAsync(request)
                                        .ContinueWith((taskwithmsg) =>
                                        {
                                            if (taskwithmsg.Status != TaskStatus.Canceled)
                                            {
                                                HttpResponseMessage response = taskwithmsg.Result;
                                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                                {
                                                    taskCompletionSource.SetResult(SetResult<T>(response));
                                                }
                                                else
                                                {
                                                    taskCompletionSource.SetException(SetException(response));
                                                }
                                            }
                                            else
                                            {
                                                taskCompletionSource.TrySetCanceled();
                                            }
                                        });
                task.Wait();

                return await taskCompletionSource.Task;
            }
        }

        public static async Task<T> PostAsync<T>(string endPoint, object request, Dictionary<string, string> headers, double? timeOut = null)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                if (timeOut != null)
                {
                    double timoutSpan = (double)timeOut;
                    client.Timeout = TimeSpan.FromSeconds(timoutSpan);
                }
                else
                {
                    double RequestTimeOut = DEFAULT_REQUEST_TIMEOUT;  
                    if (RequestTimeOut > 0)
                    {
                        client.Timeout = TimeSpan.FromSeconds(RequestTimeOut);
                    }
                }
                var taskCompletionSource = new TaskCompletionSource<T>();

                var requestNew = MakeRequest(endPoint, HttpMethod.Post, headers);

                var json = JsonConvert.SerializeObject(request);

                requestNew.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var task = client.SendAsync(requestNew)
                                            .ContinueWith((taskwithmsg) =>
                                            {
                                                if (taskwithmsg.Status != TaskStatus.Canceled)
                                                {
                                                    HttpResponseMessage response = taskwithmsg.Result;
                                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                                    {
                                                        taskCompletionSource.SetResult(SetResult<T>(response));
                                                    }
                                                    else
                                                    {
                                                        taskCompletionSource.SetException(SetException(response));
                                                    }
                                                }
                                                else
                                                {
                                                    taskCompletionSource.TrySetCanceled();
                                                }
                                            });

                task.Wait();

                return await taskCompletionSource.Task;
            }

        }

        public static async Task<T> PostAsync<T>(string endPoint, Dictionary<string, string> formBody, Dictionary<string, string> headers, double? timeOut = null)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                if (timeOut != null)
                {
                    double timoutSpan = (double)timeOut;
                    client.Timeout = TimeSpan.FromSeconds(timoutSpan);
                }
                else
                {
                    double RequestTimeOut = DEFAULT_REQUEST_TIMEOUT;       
                    if (RequestTimeOut > 0)
                    {
                        client.Timeout = TimeSpan.FromSeconds(RequestTimeOut);
                    }
                }
                var taskCompletionSource = new TaskCompletionSource<T>();

                var requestNew = MakeRequest(endPoint, HttpMethod.Post, headers);

                var content = new FormUrlEncodedContent(formBody);

                var task = client.PostAsync(endPoint, content)
                                            .ContinueWith((taskwithmsg) =>
                                            {
                                                if (taskwithmsg.Status != TaskStatus.Canceled)
                                                {
                                                    HttpResponseMessage response = taskwithmsg.Result;
                                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                                    {
                                                        taskCompletionSource.SetResult(SetResult<T>(response));
                                                    }
                                                    else
                                                    {
                                                        taskCompletionSource.SetException(SetException(response));
                                                    }
                                                }
                                                else
                                                {
                                                    taskCompletionSource.TrySetCanceled();
                                                }
                                            });
                task.Wait();
                return await taskCompletionSource.Task;
            }
        }

        private static HttpRequestMessage MakeRequest(string endPoint, HttpMethod method, Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(endPoint),
                Method = method               
            };

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            return request;
        }

        private static Exception SetException(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                || response.StatusCode == System.Net.HttpStatusCode.Forbidden
                || response.StatusCode == System.Net.HttpStatusCode.BadRequest
                )
            {
                return new HttpClientAccesDeniedException(response.ReasonPhrase);
            }
            else
            {
                const string message = "Error retrieving response.";

                return new ApplicationException(message, new Exception(message));
            }
        }

        private static T SetResult<T>(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
        }
    }

    /// <summary>
    /// HTTP Client AccesDenied Exception
    /// </summary>
    public class HttpClientAccesDeniedException : Exception
    {
        public HttpClientAccesDeniedException() : base() { }
        public HttpClientAccesDeniedException(string message) : base(message) { }
        public HttpClientAccesDeniedException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// HTTP Client Exception Class
    /// </summary>
    public class HttpClientException : Exception
    {
        public HttpClientException() : base() { }
        public HttpClientException(string message) : base(message) { }
        public HttpClientException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
