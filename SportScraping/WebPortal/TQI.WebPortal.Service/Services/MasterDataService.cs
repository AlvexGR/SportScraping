using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using TQI.Infrastructure.Entity.Models;
using TQI.Infrastructure.Utility;
using TQI.WebPortal.Service.IServices;
using TQI.WebPortal.Service.UnitOfWork;

namespace TQI.WebPortal.Service.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ILogger _logger;
        private readonly IWebPortalUnitOfWork _unitOfWork;

        public MasterDataService(ILogger logger, IWebPortalUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> InsertUpdateMatches(List<Match> matches)
        {
            if (matches == null || matches.Count == 0)
            {
                throw new ArgumentException("List of matches empty");
            }

            _logger.Information("Check for existing matches");
            var existingMatches =
                (await _unitOfWork.MatchRepository
                    .GetMatchesByGameCodes(matches.Select(x => x.GameCode)))?.ToList()
                ?? new List<Match>();

            var toInsert = new List<Match>();
            var toUpdate = new List<Match>();
            foreach (var match in matches)
            {
                if (match.HomeTeamId == match.AwayTeamId)
                {
                    _logger.Warning($"Home team id ({match.HomeTeamId}) is equal to Away team id ({match.AwayTeamId})");
                }
                var existing = existingMatches.FirstOrDefault(x => x.GameCode == match.GameCode);

                if (existing == null)
                {
                    match.CreatedAt = DateTime.Now;
                    toInsert.Add(match);
                    continue;
                }

                match.Id = existing.Id;
                match.CreatedAt = existing.CreatedAt;
                match.UpdatedAt = DateTime.Now;
                toUpdate.Add(match);
            }

            var insertNewTask = _unitOfWork.MatchRepository.InsertAsync(toInsert);
            var updateExistingTask = _unitOfWork.MatchRepository.UpdateAsync(toUpdate);
            await Task.WhenAll(insertNewTask, updateExistingTask);
            var insertResult = await insertNewTask;
            var updateResult = await updateExistingTask;

            _logger.Information($"Insert {insertResult}/{toInsert.Count}");
            _logger.Information($"Update {updateResult}/{toUpdate.Count}");

            var result = insertResult == toInsert.Count && updateResult == toUpdate.Count;
            _logger.Information($"Insert update result: {result}");
            return result;
        }

        public async Task<List<Team>> GetTeams(string sportCode)
        {
            if (string.IsNullOrEmpty(sportCode))
            {
                throw new ArgumentException("SportCode is null or empty");
            }
            var teams = (await _unitOfWork.TeamRepository.GetTeams(sportCode))?.ToList()
                        ?? new List<Team>();
            if (teams.Count == 0)
            {
                _logger.Warning($"Cannot find any teams with this SportCode {sportCode}");
            }
            return teams;
        }

        public async Task<bool> InsertUpdatePlayers(List<Player> players)
        {
            if (players == null || players.Count == 0)
            {
                throw new ArgumentException("List of players is null or empty");
            }

            _logger.Information("Check for existing players");
            var existingPlayers =
                (await _unitOfWork.PlayerRepository
                    .GetPlayersBySourceId(players.Select(x => x.SourceId)))?.ToList()
                ?? new List<Player>();

            var toInsert = new List<Player>();
            var toUpdate = new List<Player>();
            var toDelete = new List<Player>();
            foreach (var player in players)
            {
                var existing = existingPlayers.FirstOrDefault(x => x.SourceId == player.SourceId);

                if (existing == null)
                {
                    player.CreatedAt = DateTime.Now;
                    toInsert.Add(player);
                    continue;
                }

                // Player changes team
                if (existing.TeamId != player.TeamId
                    // Ignore if that player appears twice in input players
                    && players.Count(x => x.SourceId == existing.SourceId) == 1)
                {
                    toDelete.Add(existing);

                    player.CreatedAt = DateTime.Now;
                    toInsert.Add(player);
                    continue;
                }

                player.Id = existing.Id;
                player.CreatedAt = existing.CreatedAt;
                player.UpdatedAt = DateTime.Now;
                toUpdate.Add(player);
            }

            var insertNewTask = _unitOfWork.PlayerRepository.InsertAsync(toInsert);
            var updateExistingTask = _unitOfWork.PlayerRepository.UpdateAsync(toUpdate);
            var deleteExistingTask = _unitOfWork.PlayerRepository.DeleteAsync(toDelete);
            await Task.WhenAll(insertNewTask, updateExistingTask, deleteExistingTask);
            var insertResult = await insertNewTask;
            var updateResult = await updateExistingTask;
            var deleteResult = await deleteExistingTask;

            _logger.Information($"Insert {insertResult}/{toInsert.Count}");
            _logger.Information($"Update {updateResult}/{toUpdate.Count}");
            _logger.Information($"Delete {deleteResult}/{toDelete.Count}");

            var result = insertResult == toInsert.Count && updateResult == toUpdate.Count && deleteResult == toDelete.Count;
            _logger.Information($"Insert update result: {result}");
            return result;
        }

        public async Task<List<Match>> GetMatches(string sportCode, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(sportCode))
            {
                throw new ArgumentException("SportCode is empty");
            }
            fromDate = Helper.ToMinTime(fromDate);
            toDate = Helper.ToMaxTime(toDate);

            var result = (await _unitOfWork.MatchRepository.GetMatches(sportCode, fromDate, toDate))?.ToList();
            return result;
        }

        public async Task<List<Match>> GetFullMatches(string sportCode, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(sportCode))
            {
                throw new ArgumentException("SportCode is empty");
            }
            fromDate = Helper.ToMinTime(fromDate);
            toDate = Helper.ToMaxTime(toDate);

            var matches = (await _unitOfWork.MatchRepository.GetMatches(sportCode, fromDate, toDate))?.ToList();
            if (matches == null || matches.Count == 0)
            {
                throw new ArgumentException("Matches is empty");
            }

            var teams = (await _unitOfWork.TeamRepository.GetTeams(sportCode))?.ToList();
            if (teams == null || teams.Count == 0)
            {
                throw new ArgumentException("Teams is empty");
            }

            foreach (var match in matches)
            {
                var homeTeam = teams.FirstOrDefault(x => x.Id == match.HomeTeamId);
                var awayTeam = teams.FirstOrDefault(x => x.Id == match.AwayTeamId);
                match.HomeTeam = homeTeam ?? throw new ArgumentException($"Home team is not found: {match.HomeTeamId}");
                match.AwayTeam = awayTeam ?? throw new ArgumentException($"Away team is not found: {match.AwayTeamId}");
            }

            return matches;
        }
    }
}
