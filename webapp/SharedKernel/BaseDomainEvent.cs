using System;
using MediatR;

namespace TarlBreuJacoBaraKnor.webapp.SharedKernel;

public abstract record BaseDomainEvent :INotification
{
	public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
}
