using AutoMapper;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Workspaces;
using SyncVerse.Application.Interfaces;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; 
using System.Linq;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public WorkspaceService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    // CREATE
    public async Task<Result<WorkspaceResponseDto>> CreateAsync(CreateWorkspaceDto dto, string managerId)
    {
        var user = await _userManager.FindByIdAsync(managerId);
        if (user == null)
            return Result<WorkspaceResponseDto>.Failure("User not found.");

        var exists = await _unitOfWork.Repository<Workspace>()
            .Query()
            .AnyAsync(w => w.Name == dto.Name && w.CreatedByUserId == managerId);

        if (exists)
            return Result<WorkspaceResponseDto>.Failure("Workspace name already exists");

        var workspace = new Workspace
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedByUserId = managerId,
            OrgCode = Guid.NewGuid().ToString("N").Substring(0, 8)
        };

        await _unitOfWork.Repository<Workspace>().AddAsync(workspace);
        await _unitOfWork.SaveChangesAsync();


        var userWorkspace = new UserWorkspace
        {
            UserId = managerId,
            WorkspaceId = workspace.Id,
            JoinedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<UserWorkspace>().AddAsync(userWorkspace);


        user.CurrentWorkspaceId = workspace.Id;
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<WorkspaceResponseDto>.Success(
            _mapper.Map<WorkspaceResponseDto>(workspace),
            "Workspace created successfully"
        );
    }

    // UPDATE
    public async Task<Result<WorkspaceResponseDto>> UpdateAsync(string workspaceId, UpdateWorkspaceDto dto, string managerId)
    {
        var workspace = await _unitOfWork.Repository<Workspace>()
            .Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return Result<WorkspaceResponseDto>.Failure("Workspace not found");

        if (workspace.CreatedByUserId != managerId)
            return Result<WorkspaceResponseDto>.Failure("Unauthorized");

        workspace.Name = dto.Name;
        workspace.Description = dto.Description;

        _unitOfWork.Repository<Workspace>().Update(workspace);
        await _unitOfWork.SaveChangesAsync();

        return Result<WorkspaceResponseDto>.Success(
            _mapper.Map<WorkspaceResponseDto>(workspace),
            "Workspace updated successfully"
        );
    }

    // GET BY ID
    public async Task<Result<WorkspaceResponseDto>> GetByIdAsync(string workspaceId, string managerId)
    {
        var workspace = await _unitOfWork.Repository<Workspace>()
            .Query()
            .Include(w => w.CreatedByUser)
            .FirstOrDefaultAsync(w => w.Id == workspaceId && w.CreatedByUserId == managerId);

        if (workspace == null)
            return Result<WorkspaceResponseDto>.Failure("Workspace not found");

        return Result<WorkspaceResponseDto>.Success(_mapper.Map<WorkspaceResponseDto>(workspace));
    }

    // GET ALL
    public async Task<Result<List<WorkspaceResponseDto>>> GetAllAsync(string managerId)
    {
        var workspaces = await _unitOfWork.Repository<Workspace>()
            .Query()
            .Where(w => w.CreatedByUserId == managerId)
            .ToListAsync();

        return Result<List<WorkspaceResponseDto>>.Success(
            _mapper.Map<List<WorkspaceResponseDto>>(workspaces)
        );
    }

    // DELETE
    public async Task<Result<bool>> DeleteAsync(string workspaceId, string managerId)
    {
        var workspace = await _unitOfWork.Repository<Workspace>()
            .Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return Result<bool>.Failure("Workspace not found");

        if (workspace.CreatedByUserId != managerId)
            return Result<bool>.Failure("Unauthorized");

        workspace.IsDeleted = true;

        _unitOfWork.Repository<Workspace>().Update(workspace);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Workspace deleted successfully");
    }

    // RESTORE
    public async Task<Result<bool>> RestoreAsync(string workspaceId, string managerId)
    {
        var workspace = await _unitOfWork.Repository<Workspace>()
            .Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return Result<bool>.Failure("Workspace not found");

        if (workspace.CreatedByUserId != managerId)
            return Result<bool>.Failure("Unauthorized");

        workspace.IsDeleted = false;

        _unitOfWork.Repository<Workspace>().Update(workspace);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Workspace restored successfully");
    }
}
