using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;

namespace TQI.WebPortal.Service.IServices
{
    public interface IMasterDataService
    {
        /// <summary>
        /// Insert and update matches depend on data by check the current data in database
        /// </summary>
        /// <param name="matches">Matches to insert</param>
        /// <returns>True if can insert or update ALL of data otherwise false</returns>
        Task<bool> InsertUpdateMatches(List<Match> matches);

        /// <summary>
        /// Insert and update players depend on data by check the current data in database
        /// </summary>
        /// <param name="players">Players to insert</param>
        /// <returns>True if can insert or update ALL of data otherwise false</returns>
        Task<bool> InsertUpdatePlayers(List<Player> players);
        
        /// <summary>
        /// Get all teams by sport code
        /// </summary>
        /// <param name="sportCode">Sport code to query</param>
        /// <returns>List of all teams by sport code</returns>
        Task<List<Team>> GetTeams(string sportCode);

        /// <summary>
        /// Get matches [from] [to] by SportCode
        /// </summary>
        /// <param name="sportCode">SportCode to query</param>
        /// <param name="fromDate">From date</param>
        /// <param name="toDate">To date</param>
        /// <returns>List of matches</returns>
        Task<List<Match>> GetMatches(string sportCode, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get full matches model with teams and players
        /// </summary>
        /// <param name="sportCode">SportCode to query</param>
        /// <param name="fromDate">From date</param>
        /// <param name="toDate">To date</param>
        /// <returns>List of matches</returns>
        Task<List<Match>> GetFullMatches(string sportCode, DateTime fromDate, DateTime toDate);
    }
}
