using System.Threading.Tasks;
using TBalans.Domain.Entities;

namespace TBalans.Application.Services;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(User user);
}
