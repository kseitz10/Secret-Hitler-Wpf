using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SecretHitler.Game.Enums;
using SecretHitler.Game.Interfaces;

namespace SecretHitler.Server
{
    public class ClientProxy : Game.Interfaces.IClientProxy
    {
        private ClientProxy(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        // Singleton instance
        private readonly static Lazy<ClientProxy> _instance = new Lazy<ClientProxy>(() =>
        {
            var proxy = new ClientProxy(GlobalHost.ConnectionManager.GetHubContext<ServerHub>().Clients);
            return proxy;
        });

        private IHubConnectionContext<dynamic> Clients { get; set; }

        public static ClientProxy Instance
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

        public Task<bool> ApproveVeto(IPlayerInfo president)
        {
            throw new NotImplementedException();
        }

        public Task<PolicyType> GetEnactedPolicy(IPlayerInfo chancellor, IEnumerable<PolicyType> drawnPolicies)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PolicyType>> GetPresidentialPolicies(IPlayerInfo president, IEnumerable<PolicyType> drawnPolicies)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<bool>> GetVotes(IEnumerable<IPlayerInfo> voters)
        {
            throw new NotImplementedException();
        }

        public Task PolicyPeek(IPlayerInfo president, IList<PolicyType> deckTopThree)
        {
            throw new NotImplementedException();
        }

        public Task<IPlayerInfo> SelectPlayer(IPlayerInfo chooser, GameState gameState, IEnumerable<IPlayerInfo> candidates)
        {
            throw new NotImplementedException();
        }
    }
}
