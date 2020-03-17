using System;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Service.UnitOfWork
{
    public interface IWebPortalUnitOfWork : IDisposable
    {
        IMatchRepository MatchRepository { get; }
        ISportRepository SportRepository { get; }
        ITeamRepository TeamRepository { get; }
        IPlayerRepository PlayerRepository { get; }
        IScrapingInformationRepository ScrapingInformationRepository { get; }
        IProviderRepository ProviderRepository { get; }
        IPlayerOverUnderRepository PlayerOverUnderRepository { get; }
        IPlayerHeadToHeadRepository PlayerHeadToHeadRepository { get; }
        ITempTableToTestRepository TempTableToTestRepository { get; }
    }
}
