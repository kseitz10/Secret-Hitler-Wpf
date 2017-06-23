using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Server
{
    public class Director : IPlayerDirector
    {
        private Director(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        // Singleton instance
        private readonly static Lazy<Director> _instance = new Lazy<Director>(() =>
        {
            var proxy = new Director(GlobalHost.ConnectionManager.GetHubContext<ServerHub>().Clients);
            return proxy;
        });

        private IHubConnectionContext<dynamic> Clients { get; set; }

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
            Clients.All.broadcastMessage(message);
        }

        public void UpdatePlayerStates(IEnumerable<IPlayerInfo> playerData)
        {
            Clients.All.UpdatePlayerStates(playerData);
        }

        public void SelectPlayer(Guid chooser, GameState gameState, IEnumerable<Guid> candidates)
        {
            throw new NotImplementedException();
        }

        public void GetVotes(IEnumerable<Guid> voters)
        {
            throw new NotImplementedException();
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
    }
}
