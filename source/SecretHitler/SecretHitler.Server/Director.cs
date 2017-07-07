using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;
using SecretHitler.Game.Entities;

namespace SecretHitler.Server
{
    public class Director : IPlayerDirector
    {
        private Director(IHubContext connectionContext)
        {
            Context = connectionContext;
        }

        // Singleton instance
        private readonly static Lazy<Director> _instance = new Lazy<Director>(() =>
        {
            return new Director(GlobalHost.ConnectionManager.GetHubContext<ServerHub>());
        });

        private IHubContext Context { get; set; }

        public static Director Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public void Broadcast(string message)
        {
            Console.WriteLine(message);
            Context.Clients.All.MessageReceived(message);
        }

        public void SendMessage(Guid player, string message)
        {
            GetUser(player).MessageReceived(message);
        }

        public void UpdateGameData(Guid player, GameData gameData)
        {
            GetUser(player).UpdateGameData(gameData);
        }

        public void SelectPlayer(Guid chooser, GameState gameState, IEnumerable<Guid> candidates)
        {
            GetUser(chooser).PlayerSelectionRequested(gameState, candidates);
        }

        public void GetVotes(IEnumerable<Guid> voters)
        {
            foreach (var voter in voters)
                GetUser(voter).PlayerVoteRequested();
        }

        public void GetPresidentialPolicies(Guid president, IEnumerable<PolicyType> drawnPolicies)
        {
            throw new NotImplementedException();
        }

        public void GetEnactedPolicy(Guid chancellor, IEnumerable<PolicyType> drawnPolicies)
        {
            throw new NotImplementedException();
        }

        public void PolicyPeek(Guid president, IList<PolicyType> deckTopThree)
        {
            throw new NotImplementedException();
        }

        public void Reveal(Guid president, PlayerRole role)
        {
            throw new NotImplementedException();
        }

        public void ApproveVeto(Guid president)
        {
            throw new NotImplementedException();
        }

        private dynamic GetUser(Guid guid)
        {
            return Context.Clients.Group(guid.ToString());
        }
    }
}
