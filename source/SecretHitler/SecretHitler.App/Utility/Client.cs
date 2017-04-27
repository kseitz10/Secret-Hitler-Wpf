﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SecretHitler.App.Interfaces;

namespace SecretHitler.App.Utility
{
    /// <summary>
    /// The SignalR client.
    /// </summary>
    public class Client : IClient
    {
        private string _serverUrl;
        private string _nickname;
        private Guid _clientGuid;
        private HubConnection _connection;
        private IHubProxy _hubProxy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverUrl">The URL of the server to connect to.</param>
        /// <param name="nickname">Nickname of the client to display to other clients.</param>
        /// <param name="clientGuid">Guid for the client. Used for reconnections under the same identity, regardless of name.</param>
        public Client(string serverUrl, string nickname, Guid? clientGuid = null)
        {
            _serverUrl = serverUrl;
            _nickname = nickname;
            _clientGuid = clientGuid ?? Guid.NewGuid();
        }

        /// <summary>
        /// Creates and connects the hub connection and hub proxy.
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            _connection = new HubConnection(_serverUrl, new Dictionary<string, string>
            {
                { "nickname", _nickname },
                { "guid", _clientGuid.ToString() }
            });

            // TODO
            //// _connection.Closed += Connection_Closed;

            _hubProxy = _connection.CreateHubProxy("ServerHub");
            _hubProxy.On<string>("broadcastMessage", _ => MessageReceived?.Invoke(_));

            try
            {
                await _connection.Start();
                return true;
            }
            catch (HttpRequestException ex)
            {
                // TODO
                throw;
                return false;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            _hubProxy = null;
            _connection?.Dispose();
            _connection = null;
        }

        /// <summary>
        /// Send a message to other players.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(string message)
        {
            _hubProxy.Invoke("broadcastMessage", message);
        }

        public delegate void MessageReceivedDelegate(string message);
        public event MessageReceivedDelegate MessageReceived;
    }
}