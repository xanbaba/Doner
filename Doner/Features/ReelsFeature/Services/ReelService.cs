using Doner.Features.ReelsFeature.Elements;
using Doner.Features.ReelsFeature.Repository;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using FluentValidation;
using LanguageExt;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Features.ReelsFeature.Services;

public class ReelService : IReelService
{
    private readonly IReelRepository _reelRepository;
    private readonly IValidator<Reel> _reelValidator;
    private readonly IValidator<ReelElement> _reelElementValidator;
    private readonly IWorkspaceService _workspaceService;

    public ReelService
    (
        IReelRepository reelRepository,
        IValidator<Reel> reelValidator,
        IValidator<ReelElement> reelElementValidator,
        IWorkspaceService workspaceService
    )
    {
        _reelRepository = reelRepository;
        _reelValidator = reelValidator;
        _reelElementValidator = reelElementValidator;
        _workspaceService = workspaceService;
    }

    public async Task AddAsync(Reel reel, CancellationToken cancellationToken = default)
    {
        await _reelValidator.ValidateAndThrowAsync(reel, cancellationToken);
        await _reelRepository.AddAsync(reel, cancellationToken);
    }

    public async Task<Reel?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (reel is null)
        {
            return null;
        }

        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        return reel;
    }

    public async Task<bool> UpdateAsync(Reel reel, CancellationToken cancellationToken = default)
    {
        var existingReel = await _reelRepository.GetByIdAsync(reel.Id, cancellationToken);
        if (existingReel is null)
        {
            return false;
        }

        if (reel.OwnerId != existingReel.OwnerId)
        {
            throw new UnauthorizedAccessException();
        }

        reel.WorkspaceId = existingReel.WorkspaceId;

        await _reelValidator.ValidateAndThrowAsync(reel, cancellationToken);


        return await _reelRepository.UpdateAsync(reel, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(id, cancellationToken);
        if (reel is null)
        {
            return false;
        }

        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        return await _reelRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var workspaceResult = await _workspaceService.GetAsync(workspaceId, userId);

       _ = workspaceResult.Match(
           // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
           _ => Unit.Default,
           e =>
           {
               if (e is PermissionDeniedException)
               {
                   throw new UnauthorizedAccessException();
               }

               throw e;
           }
        );


        var reels = await _reelRepository.GetByWorkspaceAsync(workspaceId, cancellationToken);
        return reels.Where(reel => reel.OwnerId == userId);
    }

    public Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption,
        CancellationToken cancellationToken = default)
    {
        return _reelRepository.SearchByNameAsync(name, searchOption, cancellationToken);
    }

    public async Task<ReelElement?> GetReelElementAsync(Guid reelId, Guid elementId, Guid userId, CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return null;
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        
        return reel.ReelElements.FirstOrDefault(x => x.Id == elementId);
    }

    public async Task<IEnumerable<ReelElement>> GetReelElementsAsync(Guid reelId, Guid userId, CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return [];
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        return reel.ReelElements;
    }

    public async Task<ReelElement?> AppendReelElementAsync(Guid reelId, ReelElement reelElement, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return null;
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        await _reelElementValidator.ValidateAndThrowAsync(reelElement, cancellationToken);
        return await _reelRepository.AppendReelElementAsync(reelId, reelElement, cancellationToken);
    }

    public async Task<ReelElement?> PrependReelElementAsync(Guid reelId, ReelElement reelElement, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return null;
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        await _reelElementValidator.ValidateAndThrowAsync(reelElement, cancellationToken);
        return await _reelRepository.PrependReelElementAsync(reelId, reelElement, cancellationToken);
    }

    public async Task<ReelElement?> InsertReelElementAsync(Guid reelId, Guid insertAfterElementId, ReelElement reelElement, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return null;
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        await _reelElementValidator.ValidateAndThrowAsync(reelElement, cancellationToken);
        return await _reelRepository.InsertReelElementAsync(reelId, insertAfterElementId, reelElement, cancellationToken);
    }

    public async Task<ReelElement?> UpdateReelElementAsync(Guid reelId, ReelElement reelElement, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return null;
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        await _reelElementValidator.ValidateAndThrowAsync(reelElement, cancellationToken);
        return await _reelRepository.UpdateReelElementAsync(reelId, reelElement, cancellationToken);
    }

    public async Task<bool> DeleteReelElementAsync(Guid reelId, Guid elementId, Guid userId, CancellationToken cancellationToken = default)
    {
        var reel = await _reelRepository.GetByIdAsync(reelId, cancellationToken);
        if (reel is null)
        {
            return false;
        }
        if (reel.OwnerId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        await _reelRepository.DeleteReelElementAsync(reelId, elementId, cancellationToken);
        return true;
    }
}