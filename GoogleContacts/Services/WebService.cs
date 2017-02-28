using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace GoogleContacts.Services
{
	public abstract class WebService
	{
		protected async Task<T> SendRequestAsync<T>(Uri url, HttpMethod httpMethod = null, IDictionary<string, string> headers = null, object requestData = null)
		{
			var result = default(T);
			var method = httpMethod ?? HttpMethod.Get;
			var request = new HttpRequestMessage(method, url);

			var data = requestData == null ? null : JsonConvert.SerializeObject(requestData);
			if (data != null)
			{
				request.Content = new StringContent(data, Encoding.UTF8, "application/json");
			}
			if (headers != null)
			{
				foreach (var h in headers)
				{
					request.Headers.Add(h.Key, h.Value);
				}
			}
			var handler = new HttpClientHandler();
			var client = new HttpClient(handler);
			var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

			if (response.IsSuccessStatusCode && response.Content != null)
			{
				var content = await response.Content.ReadAsStringAsync();
				result = JsonConvert.DeserializeObject<T>(content);
			}

			return result;
		}
	}
}
