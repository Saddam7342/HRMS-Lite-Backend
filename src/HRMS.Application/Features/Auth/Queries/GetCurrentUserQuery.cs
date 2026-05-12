using HRMS.Application.Common.Interfaces;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Shared.Models;
using MediatR;

namespace HRMS.Application.Features.Auth.Queries;

public record GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>;

public class GetCurrentUserHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue) return Result<CurrentUserDto>.Failure("Unauthorized.");

        var user = await unitOfWork.Users.GetWithRolesAndPermissionsAsync(currentUserService.Email!, cancellationToken);
        if (user == null) return Result<CurrentUserDto>.Failure("User not found.");

        var dto = new CurrentUserDto(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.UserRoles.Select(r => r.Role.Name).ToList(),
            user.UserRoles.SelectMany(r => r.Role.RolePermissions).Select(p => p.Permission.Code).Distinct().ToList()
        );

        return Result<CurrentUserDto>.Success(dto);
    }
}
