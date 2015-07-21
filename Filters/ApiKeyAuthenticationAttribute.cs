using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Threading;
using System.Threading.Tasks;
using ServerStatus.Models;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Security.Principal;
using System.Net.Http.Headers;

namespace ServerStatus.Filters
{
	public class ApiKeyAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		public async Task AuthenticateAsync(HttpAuthenticationContext context, System.Threading.CancellationToken cancellationToken)
		{
			//HttpContext.Current.Items["IsApi"] = true;
			HttpContext.Current.Response.TrySkipIisCustomErrors = true;
			HttpContext.Current.Response.SuppressFormsAuthenticationRedirect = true;
			var request = context.Request;
			var authorization = request.Headers.Authorization;

			if (authorization == null)
			{
				return;
			}

			if (authorization.Scheme != "ApiKey")
			{
				return;
			}

			var key = authorization.Parameter;

			if (string.IsNullOrEmpty(key))
			{
				context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
			}

			var keyRecord = await ApiKey.FetchKeyRecord(key);

			if (keyRecord == null)
			{
				context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
			}

			var principal = new ApiKeyPrincipal(keyRecord);
			context.Principal = principal;

		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, System.Threading.CancellationToken cancellationToken)
		{
			var challenge = new AuthenticationHeaderValue("ApiKey");
			context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
			return Task.FromResult(0);
		}

		public bool AllowMultiple
		{
			get { throw new NotImplementedException(); }
		}
	}

	public class AddChallengeOnUnauthorizedResult : IHttpActionResult
	{
		public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
		{
			Challenge = challenge;
			InnerResult = innerResult;
		}

		public AuthenticationHeaderValue Challenge { get; private set; }

		public IHttpActionResult InnerResult { get; private set; }

		public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				// Only add one challenge per authentication scheme.
				if (!response.Headers.WwwAuthenticate.Any((h) => h.Scheme == Challenge.Scheme))
				{
					response.Headers.WwwAuthenticate.Add(Challenge);
				}
			}

			return response;
		}
	}

	public class AuthenticationFailureResult : IHttpActionResult
	{
		public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
		{
			ReasonPhrase = reasonPhrase;
			Request = request;
		}

		public string ReasonPhrase { get; private set; }

		public HttpRequestMessage Request { get; private set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute());
		}

		private HttpResponseMessage Execute()
		{
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			response.RequestMessage = Request;
			response.ReasonPhrase = ReasonPhrase;
			return response;
		}
	}

	public class ApiKeyPrincipal : IPrincipal
	{

		public IIdentity Identity
		{
			get;
			private set;
		}

		public ApiKey ApiKey
		{
			get;
			private set;
		}

		public bool IsInRole(string role)
		{
			return true;
		}

		public ApiKeyPrincipal(ApiKey key)
		{
			ApiKey = key;
			Identity = new ServerIdentity(key.Server, "ApiKey");
		}
	}

	public class ServerIdentity : IIdentity
	{

		public string AuthenticationType
		{
			get;
			private set;
		}

		public Server Server { get; private set; }

		public bool IsAuthenticated
		{
			get
			{
				return Server != null;
			}
		}

		public string Name
		{
			get {
				if (Server != null)
				{
					return Server.Name;
				}
				return "Guest";
			}
		}

		public ServerIdentity(Server server, string authType)
		{
			Server = server;
			AuthenticationType = authType;
		}
	}
}