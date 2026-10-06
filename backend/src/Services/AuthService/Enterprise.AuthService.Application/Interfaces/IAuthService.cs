using Enterprise.AuthService.Application.DTOs;
using Enterprise.SharedKernel.Responses;

namespace Enterprise.AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result<string>> LoginAsync(LoginRequest request);
}