namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;

public enum AccountStates
{
    Pending, // The default state for all accounts
    Declined, // The admin has manually declined your account (relevant for Couriers)
    Approved // The admin has approved of your account
}