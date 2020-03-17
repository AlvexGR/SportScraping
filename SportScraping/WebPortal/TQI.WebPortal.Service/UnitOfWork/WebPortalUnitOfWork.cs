using TQI.Infrastructure.Entity.Database;
using TQI.WebPortal.Repository.IRepositories;
using TQI.WebPortal.Repository.Repositories;

namespace TQI.WebPortal.Service.UnitOfWork
{
    public class WebPortalUnitOfWork : IWebPortalUnitOfWork
    {
        private readonly DbConnectionString _webPortalConnectionString;

        public IMatchRepository MatchRepository { get; }
        public ISportRepository SportRepository { get; }
        public ITeamRepository TeamRepository { get; }
        public IPlayerRepository PlayerRepository { get; }
        public IScrapingInformationRepository ScrapingInformationRepository { get; }
        public IProviderRepository ProviderRepository { get; }
        public IPlayerOverUnderRepository PlayerOverUnderRepository { get; }
        public IPlayerHeadToHeadRepository PlayerHeadToHeadRepository { get; }
        public ITempTableToTestRepository TempTableToTestRepository { get; }

        public WebPortalUnitOfWork(DbConnectionString webPortalConnectionString)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            _webPortalConnectionString = webPortalConnectionString;
            MatchRepository = new MatchRepository(_webPortalConnectionString);
            SportRepository = new SportRepository(_webPortalConnectionString);
            TeamRepository = new TeamRepository(_webPortalConnectionString);
            PlayerRepository = new PlayerRepository(_webPortalConnectionString);
            ScrapingInformationRepository = new ScrapingInformationRepository(_webPortalConnectionString);
            ProviderRepository = new ProviderRepository(_webPortalConnectionString);
            PlayerOverUnderRepository = new PlayerOverUnderRepository(_webPortalConnectionString);
            PlayerHeadToHeadRepository = new PlayerHeadToHeadRepository(_webPortalConnectionString);
            TempTableToTestRepository = new TempTableToTestRepository(_webPortalConnectionString);
        }

        public void Dispose()
        {
            _webPortalConnectionString.Dispose();
        }
    }
}
