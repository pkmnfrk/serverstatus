using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Owin;

namespace ServerStatus
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

			config.SuppressHostPrincipal();
			
            // Web API routes
            config.MapHttpAttributeRoutes();
			config.SuppressDefaultHostAuthentication();

			//config.Routes.MapHttpRoute(
			//	name: "DefaultApi",
			//	routeTemplate: "api/{controller}/{id}",
			//	defaults: new { id = RouteParameter.Optional }
			//);

			
        }
    }
}
