﻿using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Cqrs;
using Messages.Commands;
using Messages.Dto;
using Messages.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Shouldly;
using WebApiHostTests.Helpers;
using Xunit;

namespace WebApiHostTests
{
	public class TestQuery : IQuery<int>
	{
		public int Value { get; set; }
	}

	public class TestQueryHandler : IQueryHandler<TestQuery, Task<int>>
	{
		public Task<int> Handle(TestQuery query)
		{
			return Task.FromResult(query.Value * 2);
		}
	}

	class NotRegistered : IMessage
	{ }

	public class WebApiHostIntegrationTests : IClassFixture<WebApplicationFactory<WebApiHost.Startup>>
	{
		private readonly WebApplicationFactory<WebApiHost.Startup> _factory;
		private readonly HttpClient client;

		public WebApiHostIntegrationTests(WebApplicationFactory<WebApiHost.Startup> factory)
		{
			_factory = factory;
			client = _factory.CreateClient();
		}

		private async Task<(HttpStatusCode, T)> PostMessage<T>(IMessage message)
		{
			string messageAsJson = message.ToJson();
			var content = new StringContent(messageAsJson, Encoding.UTF8, "application/json");

			var response = await client.PostAsync("/CqrsBus", content);
			var responseContent = await response.Content.ReadAsStringAsync();

			if (response.IsSuccessStatusCode)
			{
				var responseObject = JsonConvert.DeserializeObject<T>(responseContent);

				return (response.StatusCode, responseObject);
			}
			else
			{
				return (response.StatusCode, (T)(object)responseContent);
			}
		}

		[Fact]
		public void Message_ToJson_extension_test()
		{
			var msg = new SampleQuery() { Foo = "bar" };

			var json = msg.ToJson();

			json.ShouldBe("{\"SampleQuery\":{\"Foo\":\"bar\"}}");
		}

		[Fact]
		public async Task Query_from_this_project_should_be_executed()
		{
			var message = new TestQuery { Value = 5 };

			var (httpStatus, response) = await PostMessage<int>(message);

			httpStatus.ShouldBe(HttpStatusCode.OK);
			response.ShouldBe(10);
		}

		[Fact]
		public async Task Command_from_another_project_should_be_executed()
		{
			var message = new SampleCommand { Foo = "Bar" };

			var (httpStatus, response) = await PostMessage<object>(message);

			httpStatus.ShouldBe(HttpStatusCode.OK);
			response.ShouldBeNull();
		}

		[Fact]
		public async Task Query_from_another_project_should_be_executed()
		{
			var message = new SampleQuery { Foo = "Bar" };

			var (httpStatus, response) = await PostMessage<SampleQueryResponse>(message);

			httpStatus.ShouldBe(HttpStatusCode.OK);
			response.Baz.ShouldBe("Bar");
		}

		[Fact]
		public async Task Not_existing_message_should_return_404()
		{
			var message = new NotRegistered();

			var (httpStatus, response) = await PostMessage<string>(message);

			httpStatus.ShouldBe(HttpStatusCode.NotFound);
			response.ShouldBe("Message NotRegistered not found");
		}

		[Fact]
		public async Task Handler_exception_should_be_catch()
		{
			var message = new SampleQuery { Foo = "Ex" }; // Foo=Ex will make handler to throw exception

			var (httpStatus, response) = await PostMessage<string>(message);

			httpStatus.ShouldBe(HttpStatusCode.InternalServerError);
			response.ShouldBe("Exception");
		}
	}
}
