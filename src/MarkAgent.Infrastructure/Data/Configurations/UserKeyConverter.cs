using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MarkAgent.Domain.ValueObjects;

namespace MarkAgent.Infrastructure.Data.Configurations;

public class UserKeyConverter : ValueConverter<UserKey, string>
{
    public UserKeyConverter() : base(
        userKey => userKey.Value,
        value => UserKey.Create(value))
    {
    }
}