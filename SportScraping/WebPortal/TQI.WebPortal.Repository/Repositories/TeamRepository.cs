using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class TeamRepository : BaseRepository<Team>, ITeamRepository
    {
        public TeamRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        public async Task<IEnumerable<Team>> GetTeams(string sportCode, int? timeoutSeconds = null)
        {
            const string sql = @"
SELECT
    *
FROM
    `sports_scraping`.`team` AS t
    LEFT JOIN `sports_scraping`.`player` AS p ON t.`id` = p.`team_id`
WHERE `sport_code` = @SportCode;
";
            var param = new
            {
                SportCode = sportCode
            };

            IEnumerable<Team> result;
            using (var connection = GetConnection())
            {
                connection.Open();
                var lookup = new Dictionary<int, Team>();
                result = await connection.QueryAsync<Team, Player, Team>(sql, (team, player) =>
                {
                    if (!lookup.TryGetValue(team.Id, out var teamValue))
                    {
                        team.Players = new List<Player>();
                        lookup.Add(team.Id, teamValue = team);
                    }

                    if (player != null)
                    {
                        teamValue.Players.Add(player);
                    }

                    return teamValue;
                }, param, commandTimeout: timeoutSeconds);
                connection.Close();
            }

            return result;
        }
    }
}
