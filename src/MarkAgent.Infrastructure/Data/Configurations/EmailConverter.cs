using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MarkAgent.Domain.ValueObjects;

namespace MarkAgent.Infrastructure.Data.Configurations;

public class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter() : base(
        email => email.Value,
        value => Email.Create(value))
    {
    }
}