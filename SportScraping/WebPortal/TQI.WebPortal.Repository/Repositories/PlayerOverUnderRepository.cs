using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.BaseRepository;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Entity.Models.Metrics;
using TQI.WebPortal.Repository.IRepositories;

namespace TQI.WebPortal.Repository.Repositories
{
    public class PlayerOverUnderRepository : BaseRepository<PlayerOverUnder>, IPlayerOverUnderRepository
    {
        public PlayerOverUnderRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        /// <summary>
        /// Override the base InsertAsync method
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="timeoutSeconds">timeout in seconds</param>
        /// <returns>Total inserted records</returns>
        public new async Task<int> InsertAsync(IEnumerable<PlayerOverUnder> entities, int? timeoutSeconds = null)
        {
            // Insert metric sql
            const string metricSql = @"
INSERT INTO
    `sports_scraping`.`metric` (
        `match_id`
        , `scraping_information_id`
        , `created_at`
    )
VALUES (
        @MatchId
        , @ScrapingInformationId
        , @CreatedAt
    );
";
            // Insert over under sql
            const string overUnderSql = @"
INSERT INTO
    `sports_scraping`.`player_over_under` (
        `player_id`
        , `score_type`
        , `over`
        , `over_line`
        , `under`
        , `under_line`
        , `metric_id`
    )
VALUES (
        @PlayerId
        , @ScoreType
        , @Over
        , @OverLine
        , @Under
        , @UnderLine
        , @Id
    );
";
            // Avoid possible multiple enumeration
            var playerOverUnders = entities.ToList();
            int insertResult;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // Insert first element to get LAST_INSERT_ID() as first insert id
                    insertResult = await connection.ExecuteAsync(metricSql, playerOverUnders[0], commandTimeout: timeoutSeconds);
                    int? firstInsertId;
                    if (insertResult == 1)
                    {
                        const string getLastIdQuery = "SELECT LAST_INSERT_ID();";
                        firstInsertId = (await connection.QueryAsync<int>(getLastIdQuery, null, commandTimeout: timeoutSeconds))?.FirstOrDefault();
                        if (firstInsertId == null || firstInsertId == -1)
                        {
                            throw new Exception("Cannot get last insert id");
                        }
                    }
                    else
                    {
                        throw new Exception($"Inserted result must equal to 1: {insertResult}");
                    }

                    // Insert the rest
                    insertResult = await connection.ExecuteAsync(metricSql,
                        playerOverUnders.GetRange(1, playerOverUnders.Count - 1),
                        commandTimeout: timeoutSeconds);

                    if (insertResult == playerOverUnders.Count - 1)
                    {
                        // get inserted ids
                        const string idsSql = "SELECT id FROM `sports_scraping`.`metric` WHERE id >= @FirstInsertId;";
                        var param = new {FirstInsertId = firstInsertId};

                        var ids = (await connection.QueryAsync<int>(idsSql, param, commandTimeout: timeoutSeconds))?.ToList()
                                  ?? new List<int>();
                        if (ids.Count != playerOverUnders.Count)
                        {
                            throw new Exception("Last insert id list not equal to entities list");
                        }

                        for (var i = 0; i < ids.Count; i++)
                        {
                            playerOverUnders[i].Id = ids[i];
                        }
                    }
                    else
                    {
                        throw new Exception($"Inserted result must equal to {playerOverUnders.Count}: {insertResult}");
                    }

                    insertResult = await connection.ExecuteAsync(overUnderSql, playerOverUnders, commandTimeout: timeoutSeconds);
                    transaction.Commit();
                }
                connection.Close();
            }

            return insertResult;
        }

        public async Task<IEnumerable<PlayerOverUnder>> GetByCountryAndSportCode(string countryCode, string sportCode, DateTime dateTime, int? timeoutSeconds = null)
        {
            const string sql = @"
SELECT
    *
FROM
    `sports_scraping`.`player_over_under` AS pov
    JOIN `sports_scraping`.`player` AS player ON player.`id` = pov.`player_id`
    JOIN `sports_scraping`.`metric` AS metric ON pov.`metric_id` = metric.`id`
    JOIN `sports_scraping`.`scraping_information` AS scrapeInfo ON scrapeInfo.`id` = metric.`scraping_information_id`
    JOIN `sports_scraping`.`provider` AS provider ON provider.`id` = scrapeInfo.`provider_id`
    JOIN `sports_scraping`.`match` AS mt ON mt.`id` = metric.`match_id`
WHERE
    provider.`country_code` = @CountryCode
    AND provider.`sport_code` = @SportCode
    AND DATE(metric.`created_at`) = DATE(@ScrapeDate)
    AND scrapeInfo.`id` IN (
        SELECT
            MAX(si.`id`)
        FROM
            `sports_scraping`.`scraping_information` AS si
            JOIN `sports_scraping`.`provider` AS p ON si.`provider_id` = p.`id`
        WHERE
            p.`sport_code` = @SportCode
            AND p.`country_code` = @CountryCode
            AND DATE(si.`scrape_time`) = DATE(@ScrapeDate)
        GROUP BY p.`id`
    )
ORDER BY mt.`id`;
";
            var param = new
            {
                SportCode = sportCode,
                CountryCode = countryCode,
                ScrapeDate = dateTime
            };

            IEnumerable<PlayerOverUnder> result;
            using (var connection = GetConnection())
            {
                connection.Open();
                result = await connection
                    .QueryAsync<PlayerOverUnder, Player, Metric, ScrapingInformation, Provider, Match, PlayerOverUnder>(
                        sql, (playerOverUnder, player, metric, scrapingInformation, provider, match) =>
                        {
                            playerOverUnder.Id = metric.Id;
                            playerOverUnder.MatchId = match.Id;
                            playerOverUnder.Match = match;
                            playerOverUnder.CreatedAt = metric.CreatedAt;
                            scrapingInformation.ProviderId = provider.Id;
                            scrapingInformation.Provider = provider;
                            playerOverUnder.ScrapingInformationId = metric.ScrapingInformationId;
                            playerOverUnder.ScrapingInformation = scrapingInformation;
                            playerOverUnder.PlayerId = player.Id;
                            playerOverUnder.Player = player;
                            return playerOverUnder;
                        }, param, commandTimeout: timeoutSeconds);
                connection.Close();
            }

            return result;
        }
    }
}
