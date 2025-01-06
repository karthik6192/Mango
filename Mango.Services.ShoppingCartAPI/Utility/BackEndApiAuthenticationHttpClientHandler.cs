using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Mango.Services.ShoppingCartAPI.Utility
{
    public class BackEndApiAuthenticationHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor accessor;

        public BackEndApiAuthenticationHttpClientHandler(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await accessor.HttpContext.GetTokenAsync("access-token");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
