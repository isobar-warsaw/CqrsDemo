﻿namespace tBlabs.Cqrs.Core.Interfaces
{
	public interface IQueryWithStream<out T> : IQuery<T>, IMessageWithStream
	{ }
}