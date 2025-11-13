using System;

namespace TarlBreuJacoBaraKnor.webapp.Core.Exceptions;

public class BaseException : Exception
{
	public BaseException()
	{
	}

	public BaseException(string? message) : base(message)
	{
	}

	public BaseException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}

public class EntityNotFoundException : BaseException
{
	public EntityNotFoundException()
	{
	}

	public EntityNotFoundException(string? message) : base(message)
	{
	}

	public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}

