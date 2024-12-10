using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaftConsensus
{
    /// <summary>
    /// Represents the current state of a Raft node
    /// </summary>
    public enum NodeState
    {
        Follower,
        Candidate,
        Leader
    }

    /// <summary>
    /// Represents a message in the Raft protocol
    /// </summary>
    [Serializable]
    public class RaftMessage
    {
        public MessageType Type { get; set; }
        public int Term { get; set; }
        public string SenderId { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// Types of messages in the Raft protocol
    /// </summary>
    public enum MessageType
    {
        RequestVote,
        VoteResponse,
        Heartbeat
    }

    /// <summary>
    /// Represents a node in the Raft consensus cluster
    /// </summary>
    public class RaftNode
    {
        private readonly string _nodeId;
        private readonly int _port;
        private readonly UdpClient _udpClient;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // Raft algorithm state
        private NodeState _currentState;
        private int _currentTerm;
        private string _votedFor;
        private DateTime _lastHeartbeatTime;

        // Cluster configuration
        private readonly List<IPEndPoint> _clusterNodes;
        private readonly Random _random;

        // Election and heartbeat timeouts
        private readonly int _electionTimeoutMin = 150;
        private readonly int _electionTimeoutMax = 300;
        private readonly int _heartbeatInterval = 50;

        public RaftNode(string nodeId, int port, List<IPEndPoint> clusterNodes)
        {
            _nodeId = nodeId;
            _port = port;
            _udpClient = new UdpClient(_port);
            _cancellationTokenSource = new CancellationTokenSource();
            _random = new Random();

            // Initial state
            _currentState = NodeState.Follower;
            _currentTerm = 0;
            _votedFor = null;
            _lastHeartbeatTime = DateTime.UtcNow;

            _clusterNodes = clusterNodes.Where(n => n.Port != _port).ToList();
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"Node {_nodeId} starting on port {_port}");

            // Start receiving messages
            var receiveTask = ReceiveMessagesAsync();

            // Start election and heartbeat logic
            var electionTask = RunElectionLogicAsync();

            await Task.WhenAll(receiveTask, electionTask);
        }

        private async Task ReceiveMessagesAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    var message = DeserializeMessage(result.Buffer);

                    switch (message.Type)
                    {
                        case MessageType.RequestVote:
                            HandleVoteRequest(message, result.RemoteEndPoint);
                            break;
                        case MessageType.VoteResponse:
                            HandleVoteResponse(message);
                            break;
                        case MessageType.Heartbeat:
                            HandleHeartbeat(message);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message on node {_nodeId}: {ex.Message}");
                }
            }
        }

        private async Task RunElectionLogicAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                switch (_currentState)
                {
                    case NodeState.Follower:
                        await RunFollowerLogic();
                        break;
                    case NodeState.Candidate:
                        await RunCandidateLogic();
                        break;
                    case NodeState.Leader:
                        await RunLeaderLogic();
                        break;
                }
            }
        }

        private async Task RunFollowerLogic()
        {
            int electionTimeout = _random.Next(_electionTimeoutMin, _electionTimeoutMax);
            await Task.Delay(electionTimeout);

            // Check if we've received a heartbeat recently
            if ((DateTime.UtcNow - _lastHeartbeatTime).TotalMilliseconds > electionTimeout)
            {
                // No heartbeat received, transition to candidate
                _currentState = NodeState.Candidate;
                Console.WriteLine($"Node {_nodeId} transitioning to Candidate");
            }
        }

        private async Task RunCandidateLogic()
        {
            // Increment current term
            _currentTerm++;
            _votedFor = _nodeId;

            int votesReceived = 1; // Vote for self
            int totalVotes = _clusterNodes.Count + 1;

            // Send vote requests to all other nodes
            foreach (var node in _clusterNodes)
            {
                var voteRequest = new RaftMessage
                {
                    Type = MessageType.RequestVote,
                    Term = _currentTerm,
                    SenderId = _nodeId
                };

                await SendMessageAsync(voteRequest, node);
            }

            // Wait for votes
            await Task.Delay(_random.Next(_electionTimeoutMin, _electionTimeoutMax));

            // Check if won election
            if (votesReceived > totalVotes / 2)
            {
                _currentState = NodeState.Leader;
                Console.WriteLine($"Node {_nodeId} elected as Leader in term {_currentTerm}");
            }
            else
            {
                // If not elected, go back to follower
                _currentState = NodeState.Follower;
                Console.WriteLine($"Node {_nodeId} failed to become Leader");
            }
        }

        private async Task RunLeaderLogic()
        {
            // Send heartbeats to all nodes
            foreach (var node in _clusterNodes)
            {
                var heartbeat = new RaftMessage
                {
                    Type = MessageType.Heartbeat,
                    Term = _currentTerm,
                    SenderId = _nodeId
                };

                await SendMessageAsync(heartbeat, node);
            }

            // Wait before sending next heartbeat
            await Task.Delay(_heartbeatInterval);
        }

        private void HandleVoteRequest(RaftMessage message, IPEndPoint sender)
        {
            bool voteGranted = false;

            // Vote request logic
            if (message.Term > _currentTerm)
            {
                _currentTerm = message.Term;
                _currentState = NodeState.Follower;
                _votedFor = null;
            }

            if (message.Term == _currentTerm &&
                (_votedFor == null || _votedFor == message.SenderId))
            {
                voteGranted = true;
                _votedFor = message.SenderId;
            }

            // Send vote response
            var response = new RaftMessage
            {
                Type = MessageType.VoteResponse,
                Term = _currentTerm,
                SenderId = _nodeId,
                Success = voteGranted
            };

            SendMessageAsync(response, sender).Wait();
        }

        private void HandleVoteResponse(RaftMessage message)
        {
            // In a real implementation, this would track votes
            if (message.Success)
            {
                Console.WriteLine($"Node {_nodeId} received vote from {message.SenderId}");
            }
        }

        private void HandleHeartbeat(RaftMessage message)
        {
            // Update last heartbeat time
            _lastHeartbeatTime = DateTime.UtcNow;

            // Step down if message is from a higher term
            if (message.Term > _currentTerm)
            {
                _currentTerm = message.Term;
                _currentState = NodeState.Follower;
                _votedFor = null;
            }

            Console.WriteLine($"Node {_nodeId} received heartbeat from {message.SenderId}");
        }

        private async Task SendMessageAsync(RaftMessage message, IPEndPoint recipient)
        {
            try
            {
                byte[] data = SerializeMessage(message);
                await _udpClient.SendAsync(data, data.Length, recipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message from node {_nodeId}: {ex.Message}");
            }
        }

        private byte[] SerializeMessage(RaftMessage message)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(message);
            return Encoding.UTF8.GetBytes(json);
        }

        private RaftMessage DeserializeMessage(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<RaftMessage>(json);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _udpClient.Close();
        }
    }

    /// <summary>
    /// Main program to demonstrate Raft Leader Election
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create cluster configuration
            var clusterNodes = new List<IPEndPoint>
            {
                new IPEndPoint(IPAddress.Loopback, 5001),
                new IPEndPoint(IPAddress.Loopback, 5002),
                new IPEndPoint(IPAddress.Loopback, 5003)
            };

            // Create and start nodes
            var nodes = clusterNodes.Select(endpoint =>
                new RaftNode($"Node-{endpoint.Port}", endpoint.Port, clusterNodes)
            ).ToList();

            // Start all nodes
            var startTasks = nodes.Select(node => node.StartAsync());

            // Run for a while to simulate election
            await Task.Delay(5000);

            // Stop nodes
            foreach (var node in nodes)
            {
                node.Stop();
            }
        }
    }
}