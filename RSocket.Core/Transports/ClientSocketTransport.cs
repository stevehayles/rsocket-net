﻿using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Transports
{
    public class ClientSocketTransport : SocketTransport, IRSocketTransport
    {
        private Socket _socket;
        private IPEndPoint _endpoint;

        public Uri Url { get; private set; }

        public ClientSocketTransport(string url, PipeOptions outputoptions = null, PipeOptions inputoptions = null) 
            : this(new Uri(url), outputoptions, inputoptions, null)
        {
        }

        public ClientSocketTransport(Uri url, PipeOptions outputOptions = null, PipeOptions inputOptions = null, WebSocketOptions options = null) 
            :base(outputOptions, inputOptions)
        {
            Url = url;
            if (string.Compare(url.Scheme, "TCP", true) != 0)
                throw new ArgumentException("Only TCP connections are supported.", nameof(Url));

            if (url.Port == -1)
                throw new ArgumentException("TCP Port must be specified.", nameof(Url));
        }

        public override async Task StartAsync(CancellationToken cancel = default)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(Url.Host), Url.Port);
            _socket = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            
            await _socket.ConnectAsync(_endpoint);

            _ = ProcessSocketAsync(_socket);
        }
    }
}