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
    public class PlayerHeadToHeadRepository : BaseRepository<PlayerHeadToHead>, IPlayerHeadToHeadRepository
    {
        public PlayerHeadToHeadRepository(DbConnectionString dbConnectionString) : base(dbConnectionString)
        {
        }

        /// <summary>
        /// Override the base InsertAsync method
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="timeoutSeconds">timeout in seconds</param>
        /// <returns>Total inserted records</returns>
        public new async Task<int> InsertAsync(IEnumerable<PlayerHeadToHead> entities, int? timeoutSeconds = null)
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
            // Insert head to head sql
            const string headToHeadSql = @"
INSERT INTO
    `sports_scraping`.`player_head_to_head` (
        `player_a_id`
        , `player_b_id`
        , `player_a_price`
        , `player_b_price`
        , `is_tie_included`
        , `tie_price`
        , `metric_id`
    )
VALUES (
        @PlayerAId
        , @PlayerBId
        , @PlayerAPrice
        , @PlayerBPrice
        , @IsTieIncluded
        , @TiePrice
        , @Id
    );
";
            // Avoid possible multiple enumeration
            var playerHeadToHeads = entities.ToList();
            int insertResult;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // Insert first element to get LAST_INSERT_ID() as first insert id
                    insertResult = await connection.ExecuteAsync(metricSql, playerHeadToHeads[0], commandTimeout: timeoutSeconds);
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
                        playerHeadToHeads.GetRange(1, playerHeadToHeads.Count - 1),
                        commandTimeout: timeoutSeconds);

                    if (insertResult == playerHeadToHeads.Count - 1)
                    {
                        // get inserted ids
                        const string idsSql = "SELECT id FROM `sports_scraping`.`metric` WHERE id >= @FirstInsertId;";
                        var param = new { FirstInsertId = firstInsertId };

                        var ids = (await connection.QueryAsync<int>(idsSql, param, commandTimeout: timeoutSeconds))?.ToList()
                                  ?? new List<int>();
                        
                        if (ids.Count != playerHeadToHeads.Count)
                        {
                            throw new Exception("Last insert id list not equal to entities list");
                        }

                        for (var i = 0; i < ids.Count; i++)
                        {
                            playerHeadToHeads[i].Id = ids[i];
                        }
                    }
                    else
                    {
                        throw new Exception($"Inserted result must equal to {playerHeadToHeads.Count}: {insertResult}");
                    }

                    insertResult = await connection.ExecuteAsync(headToHeadSql, playerHeadToHeads, commandTimeout: timeoutSeconds);
                    transaction.Commit();
                }
                connection.Close();
            }

            return insertResult;
        }

        public async Task<IEnumerable<PlayerHeadToHead>> GetByCountryAndSportCode(string countryCode, string sportCode, DateTime dateTime, int? timeoutSeconds = null)
        {
            const string sql = @"
SELECT
    *
FROM
    `sports_scraping`.`player_head_to_head` AS phth
    JOIN `sports_scraping`.`player` AS player ON (player.`id` = phth.`player_a_id` OR player.`id` = phth.`player_b_id`)
    JOIN `sports_scraping`.`metric` AS metric ON phth.`metric_id` = metric.`id`
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
ORDER BY provider.`id`;
";
            var param = new
            {
                SportCode = sportCode,
                CountryCode = countryCode,
                ScrapeDate = dateTime
            };

            IEnumerable<PlayerHeadToHead> result;
            using (var connection = GetConnection())
            {
                var hashSet = new HashSet<int>();
                connection.Open();
                result = await connection
                    .QueryAsync<PlayerHeadToHead, Player, Metric, ScrapingInformation, Provider, Match, PlayerHeadToHead>(
                        sql, (playerHeadToHead, player, metric, scrapingInformation, provider, match) =>
                        {
                            playerHeadToHead.Id = metric.Id;
                            playerHeadToHead.MatchId = match.Id;
                            playerHeadToHead.Match = match;
                            playerHeadToHead.CreatedAt = metric.CreatedAt;
                            scrapingInformation.ProviderId = provider.Id;
                            scrapingInformation.Provider = provider;
                            playerHeadToHead.ScrapingInformationId = metric.ScrapingInformationId;
                            playerHeadToHead.ScrapingInformation = scrapingInformation;

                            if (!hashSet.Contains(metric.Id))
                            {
                                playerHeadToHead.PlayerAId = player.Id;
                                playerHeadToHead.PlayerA = player;
                                hashSet.Add(metric.Id);
                            }
                            else
                            {
                                playerHeadToHead.PlayerBId = player.Id;
                                playerHeadToHead.PlayerB = player;
                            }
                            
                            return playerHeadToHead;
                        }, param, commandTimeout: timeoutSeconds);
                connection.Close();
            }

            return result;
        }
    }
}
