﻿using System;
using System.Collections.Generic;
using Duality;
using Pathfindax.Algorithms;
using Pathfindax.Collections.Interfaces;
using Pathfindax.Graph;
using Pathfindax.Nodes;
using Pathfindax.Paths;
using Pathfindax.Threading;

namespace Pathfindax.PathfindEngine
{
	/// <summary>
	/// Provides multithreading functionality using a producer consumer queue
	/// </summary>
	/// <typeparam name="TDefinitionNodeNetwork"></typeparam>
	/// <typeparam name="TThreadNodeNetwork"></typeparam>
	/// <typeparam name="TPath"></typeparam>
	public class Pathfinder<TDefinitionNodeNetwork, TThreadNodeNetwork, TPath> : IPathfinder<TDefinitionNodeNetwork, TThreadNodeNetwork, TPath>
		where TDefinitionNodeNetwork : IDefinitionNodeNetwork
		where TThreadNodeNetwork : IPathfindNodeNetwork
		where TPath : class, IPath
	{
		public event Event<IPathfinder> Disposed;
		public IPathFindAlgorithm<TThreadNodeNetwork, TPath> PathFindAlgorithm { get; }
		IPathFindAlgorithm<TPath> IPathfinder<TPath>.PathFindAlgorithm => PathFindAlgorithm;
		IPathFindAlgorithm IPathfinder.PathFindAlgorithm => PathFindAlgorithm;

		private readonly List<TThreadNodeNetwork> _pathfindNodeNetworks = new List<TThreadNodeNetwork>();
		public IReadOnlyList<TThreadNodeNetwork> PathfindNodeNetworks => _pathfindNodeNetworks;
		IReadOnlyList<IPathfindNodeNetwork> IPathfinder.PathfindNodeNetworks => (IReadOnlyList<IPathfindNodeNetwork>)PathfindNodeNetworks;

		public TDefinitionNodeNetwork DefinitionNodeNetwork { get; }
		IDefinitionNodeNetwork IPathfinder.DefinitionNodeNetwork => DefinitionNodeNetwork;

		private readonly MultithreadedWorkerQueue<PathRequest<TPath>> _multithreadedWorkerQueue;
		public ISynchronizationContext SynchronizationContext { get; }

		private ICache<IPathRequest, TPath> _pathCache;

		/// <summary>
		/// Creates a new <see cref="Pathfinder{TSourceNodeNetwork,TThreadNodeNetwork, TPath}"/>
		/// </summary>
		/// <param name="synchronizationContext"></param>
		/// <param name="definitionNodeNetwork"></param>
		/// <param name="pathFindAlgorithm"></param>
		/// <param name="processerConstructor">Used to construct the processers for each thread</param>
		/// <param name="pathCache"></param>
		/// <param name="threads">The amount of threads that will be used</param>
		public Pathfinder(
			ISynchronizationContext synchronizationContext, 
			TDefinitionNodeNetwork definitionNodeNetwork, 
			IPathFindAlgorithm<TThreadNodeNetwork, TPath> pathFindAlgorithm, 
			Func<TDefinitionNodeNetwork, IPathFindAlgorithm<TThreadNodeNetwork, TPath>, ICache<IPathRequest, TPath>, PathRequestProcesser<TThreadNodeNetwork, TPath>> processerConstructor, 
			ICache<IPathRequest, TPath> pathCache, 
			int threads = 1)
		{
			if (threads < 1) throw new ArgumentException("There is a minimum of 1 thread");
			_pathCache = pathCache;
			PathFindAlgorithm = pathFindAlgorithm;
			DefinitionNodeNetwork = definitionNodeNetwork;
			_multithreadedWorkerQueue = new MultithreadedWorkerQueue<PathRequest<TPath>>(() =>
			{
				var processer = processerConstructor.Invoke(DefinitionNodeNetwork, PathFindAlgorithm, _pathCache);
				_pathfindNodeNetworks.Add(processer.NodeNetwork);
				return processer;
			}, threads);
			SynchronizationContext = synchronizationContext;
		}

		/// <summary>
		/// Starts giving the pathfinders paths to solve if there are any
		/// </summary>
		public void Start()
		{
			_multithreadedWorkerQueue.Start();
		}

		/// <summary>
		/// Stops giving the pathfinders paths to solve if there are any
		/// </summary>
		public void Stop()
		{
			_multithreadedWorkerQueue.Stop();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_multithreadedWorkerQueue.Dispose();
			Disposed?.Invoke(this);
		}

		public void RequestPath(PathRequest<TPath> pathRequest)
		{
			pathRequest.StartSolvePath(this);
			_multithreadedWorkerQueue.Enqueue(pathRequest);
		}

		public PathRequest<TPath> RequestPath(Vector3 start, Vector3 end, PathfindaxCollisionCategory collisionLayer = PathfindaxCollisionCategory.None, byte agentSize = 1)
		{
			return RequestPath(start.X, start.Y, end.X, end.Y, collisionLayer, agentSize);
		}

		public PathRequest<TPath> RequestPath(Vector2 start, Vector2 end, PathfindaxCollisionCategory collisionLayer = PathfindaxCollisionCategory.None, byte agentSize = 1)
		{

			return RequestPath(start.X, start.Y, end.X, end.Y, collisionLayer, agentSize);
		}

		public PathRequest<TPath> RequestPath(float x1, float y1, float x2, float y2, PathfindaxCollisionCategory collisionLayer = PathfindaxCollisionCategory.None, byte agentSize = 1)
		{
			var startNode = DefinitionNodeNetwork.GetNodeIndex(x1, y1);
			var endNode = DefinitionNodeNetwork.GetNodeIndex(x2, y2);
			return RequestPath(startNode, endNode, collisionLayer, agentSize);
		}

		public PathRequest<TPath> RequestPath(int start, int end, PathfindaxCollisionCategory collisionLayer = PathfindaxCollisionCategory.None, byte agentSize = 1)
		{
			return PathRequest.Create(this, start, end, collisionLayer, agentSize);
		}
	}
}
