using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Aggregate Root - Customer who places orders.
/// Does NOT hold a list of Orders — Order is a separate aggregate referenced by CustomerId.
/// </summary>
public class Customer : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string Phone { get; private set; } = string.Empty;
    public Address? DefaultAddress { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private Customer() { }

    public Customer(string firstName, string lastName, Email email, string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new EmptyNameException("Customer first name");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new EmptyNameException("Customer last name");

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
    }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new EmptyNameException("Customer first name");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new EmptyNameException("Customer last name");

        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangeEmail(Email email) => Email = email;

    public void ChangePhone(string phone) => Phone = phone;

    public void SetDefaultAddress(Address address) => DefaultAddress = address;
}
