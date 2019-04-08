﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Extensions;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Middlewares.Extensions;
using Newtonsoft.Json;

namespace Middlewares
{
	public class CqrsBusMiddleware
	{
		private readonly RequestDelegate _nextMiddleware;

		private readonly CqrsBusMiddlewareOptions _options;

		public CqrsBusMiddleware(
			RequestDelegate nextMiddleware,
			CqrsBusMiddlewareOptions options)
		{
			_nextMiddleware = nextMiddleware;
			_options = options;
		}

		public async Task InvokeAsync(HttpContext httpContext, IMessageBus messageBus)
		{
			var requestPath = httpContext.Request.Path;

			if (requestPath == "/CqrsBus")
			{
				try
				{
					if (IsMessageWithStream(httpContext))
					{
						await ProcessMessageWithStream(httpContext, messageBus);
					}
					else
					{
						await ProcessMessage(httpContext, messageBus);
					}
				}
				catch (MessageNotFoundException e)
				{
					httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
					await httpContext.Response.WriteAsync(e.Message);
				}
				catch (Exception e)
				{
					httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					await httpContext.Response.WriteAsync(e.Message);
				}
			}
			else
			{
				await _nextMiddleware(httpContext);
			}
		}

		private bool IsMessageWithStream(HttpContext context)
		{
			return context.Request.Headers["Message"].ToString().IsNotEmpty();
		}

		private async Task ProcessMessage(HttpContext context, IMessageBus messageBus)
		{
			string message = context.Request.Body.ReadAsString();

			var messageExecutionResult = await messageBus.Execute(message);

			context.Response.StatusCode = (int)HttpStatusCode.OK;
			await context.Response.WriteAsync(JsonConvert.SerializeObject(messageExecutionResult));
		}

		private async Task ProcessMessageWithStream(HttpContext context, IMessageBus messageBus)
		{
			Stream stream = context.Request.Body;
			MemoryStream ms = new MemoryStream();
			stream.CopyTo(ms);
			var x = ms.Seek(0, SeekOrigin.Begin);

			string message = context.Request.Headers["Message"];

			var messageExecutionResult = await messageBus.Execute(message, ms);


			ms.Dispose();

			context.Response.StatusCode = (int)HttpStatusCode.OK;
			await context.Response.WriteAsync(JsonConvert.SerializeObject(messageExecutionResult));
		}
	}
}