using System.Collections.Generic;

namespace TarlBreuJacoBaraKnor.webapp.SharedKernel;

public abstract class BaseEntity
{
	public List<BaseDomainEvent> Events = new();
}
