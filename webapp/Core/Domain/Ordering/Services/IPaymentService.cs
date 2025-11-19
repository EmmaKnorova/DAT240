namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services
{
    public interface IPaymentService
    {
        string CreatePaymentSession(decimal amount, string currency = "nok");
    }
}