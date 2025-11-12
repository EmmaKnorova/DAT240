using System.Collections.Generic;

namespace TarlBreuJacoBaraKnor.webapp.SharedKernel;

public interface IValidator<T>
{
	(bool IsValid, string Error) IsValid(T item);
}
