using Doner.Features.ReelsFeature.Repository;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using FluentValidation;
using LanguageExt;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Features.ReelsFeature.Services;

public class ReelService : IReelService
{
    private readonly IReelRepository _reelRepository;
    private readonly IValidator<Reel> _reelValidator;
    private readonly IWorkspaceService _workspaceService;

    public ReelService
    (
        IReelRepository reelRepository,
        IValidator<Reel> reelValidator,
        IWorkspaceService workspaceService
    )
    {
        _reelRepository = reelRepository;
        _reelValidator = reelValidator;
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
        var workspaceResult = await _workspaceService.GetAsync(workspaceId);

       _ = workspaceResult.Match(
           // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
           w =>
            {
                if (w.OwnerId != userId)
                {
                    throw new UnauthorizedAccessException();
                }
                return Unit.Default;
            },
            e => throw e
        );


        var reels = await _reelRepository.GetByWorkspaceAsync(workspaceId, cancellationToken);
        return reels.Where(reel => reel.OwnerId == userId);
    }

    public Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption,
        CancellationToken cancellationToken = default)
    {
        return _reelRepository.SearchByNameAsync(name, searchOption, cancellationToken);
    }
}