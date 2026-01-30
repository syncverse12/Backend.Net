using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Workspaces;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Application.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkspaceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // CREATE
        public async Task<Result<WorkspaceResponseDto>> CreateAsync(CreateWorkspaceDto dto, string managerId)
        {
            var workspace = new Workspace
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedByUserId = managerId
            };

            await _unitOfWork.Repository<Workspace>().AddAsync(workspace);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkspaceResponseDto>.Success(_mapper.Map<WorkspaceResponseDto>(workspace), "Workspace created successfully");
        }

        // UPDATE
        public async Task<Result<WorkspaceResponseDto>> UpdateAsync(string workspaceId, UpdateWorkspaceDto dto, string managerId)
        {
            var workspace = await _unitOfWork.Repository<Workspace>().GetByIdAsync(workspaceId);

            if (workspace == null)
                return Result<WorkspaceResponseDto>.Failure("Workspace not found");

            if (workspace.CreatedByUserId != managerId)
                return Result<WorkspaceResponseDto>.Failure("Unauthorized");

            workspace.Name = dto.Name;
            workspace.Description = dto.Description;

            _unitOfWork.Repository<Workspace>().Update(workspace);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkspaceResponseDto>.Success(_mapper.Map<WorkspaceResponseDto>(workspace), "Workspace updated successfully");
        }

        // GET DETAILS
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
    }
}
