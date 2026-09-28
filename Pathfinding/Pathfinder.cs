using System.Collections.Generic;
using UnityEngine;

namespace BellumCorpus.Pathfinding
{
    using GameManagement;
    using Collections;
    using Grid;
    /// <summary>
    /// Finds and constructs a path between two hexes using A* or Dijkstra's algorithm.
    /// </summary>
    public class Pathfinder
    {
        private HexGrid _Grid;
        private UnitManager _UnitManager;
        private PriorityQueue<PathNode> _Frontier = new PriorityQueue<PathNode>();
        private HashSet<Hex> _VisitedHexes = new HashSet<Hex>();
        private IList<PathNode> _ReachableNodes = new List<PathNode>();

        public Pathfinder(HexGrid grid)
        {
            _Grid = grid;
            _UnitManager = GameManager.Instance.LevelManager.UnitManager;
        }

        # region A*

        /// <summary>
        /// Constructs a path between two positions on the grid using A* algorithm.
        /// </summary>
        /// <param name="startPosition">The start position of the path.</param>
        /// <param name="endPosition">The end position of the path.</param>
        /// <param name="earlyExit">When true, the search will stop as soon as the end hex is reached.
        /// When false, the search will continue until all reachable hexes are visited.</param>
        /// <param name="movementRange">The range (in hexes) where the path should be looked for.</param>
        /// <param name="pathNodePool">The pool of PathNodes of the pathfinding caller.</param>
        /// <param name="getClosestHexInRange">Should pathfinding return a path to the closest hex towards target
        /// if end hex is outside movement range.</param>
        /// <param name="getPathToNeighbour">If endPosition is Non-Traversable, should pathfinding return a path to a
        /// neighbour hex of the endPosition.</param>
        /// <returns>A list of hexes representing the constructed path.</returns>
        public List<Hex> AStarPath(Vector3 startPosition, Vector3 endPosition, int movementRange,
            PathNodePool pathNodePool, bool earlyExit = true, bool getClosestHexInRange = false,
            bool getPathToNeighbour = false)
        {
            if (startPosition == endPosition)
            {
                Debug.LogWarning("Cannot find path between same positions!");
                return null;
            }

            CubicCoordinate startCoordinate = _Grid.WorldToCubic(startPosition);
            CubicCoordinate endCoordinate = _Grid.WorldToCubic(endPosition);
            Hex startHex = _Grid.GetHex(startCoordinate);
            Hex endHex = _Grid.GetHex(endCoordinate);

            if (startHex == null || endHex == null || startHex.Flags == HexFlags.NonTraverseable)
            {
                return null;
            }

            IList<Hex> hexesInMovementRange = _Grid.GetHexRange(startHex, movementRange, false, HexFlags.NonTraverseable);

            if (!getClosestHexInRange && !IsInMovementRange(startHex, endHex, hexesInMovementRange))
            {
                return null;
            }

            if (getPathToNeighbour
                && (endHex.Flags == HexFlags.NonTraverseable || _UnitManager.UnitPositions.ContainsKey(endHex.Index)))
            {
                int neighbourCount = _Grid.GetHexRange(endHex, 1, false, HexFlags.NonTraverseable).Length;
                endHex = endHex.Neighbors[Random.Range(0, neighbourCount)];
            }

            PathNode startNode = pathNodePool.Get();
            startNode.CorrespondingHex = startHex;
            startNode.ParentNode = null;
            startNode.GCost = 0;
            startNode.HCost = GetEstimatedCost(startHex, endHex);
            PathNode endNode = pathNodePool.Get();
            endNode.CorrespondingHex = endHex;

            _Frontier.Clear();
            _VisitedHexes.Clear();
            int currentHexIndex = 0;

            _Frontier.Enqueue(startNode);

            while (currentHexIndex < hexesInMovementRange.Count && _Frontier.Count > 0)
            {
                PathNode currentPathNode = _Frontier.Dequeue();
                _VisitedHexes.Add(currentPathNode.CorrespondingHex);

                if (earlyExit && currentPathNode.CorrespondingHex == endHex || currentPathNode.GCost >= movementRange)
                {
                    endNode = currentPathNode;
                    break;
                }

                HexNeighbors neighbourHexes = currentPathNode.CorrespondingHex.Neighbors;

                foreach (Hex neighbourHex in neighbourHexes)
                {
                    if (_VisitedHexes.Contains(neighbourHex)
                        || neighbourHex == null
                        || neighbourHex.Flags == HexFlags.NonTraverseable
                        || _UnitManager.UnitPositions.ContainsKey(neighbourHex.Coordinates.Index)
                        || Hex.GetEdgeType(currentPathNode.CorrespondingHex.EnvironmentData.Elevation,
                            neighbourHex.EnvironmentData.Elevation) == HexEdgeType.Cliff)
                    {
                        continue;
                    }

                    PathNode neighbourNode = pathNodePool.Get();
                    neighbourNode.CorrespondingHex = neighbourHex;

                    int costToNeighbour = GetCostToNeighbour(currentPathNode.CorrespondingHex, neighbourHex);

                    int costSoFar = currentPathNode.GCost + costToNeighbour;

                    if (costSoFar < neighbourNode.GCost || !_VisitedHexes.Contains(neighbourHex))
                    {
                        neighbourNode.GCost = costSoFar;
                        neighbourNode.HCost = GetEstimatedCost(neighbourHex, endHex);
                        neighbourNode.ParentNode = currentPathNode;

                        _Frontier.Enqueue(neighbourNode);
                    }
                }
                currentHexIndex++;
            }

            return RetracePath(startNode, endNode);
        }

        #endregion

        #region Dijkstra

        /// <summary>
        /// Finds all possible paths within movement range using Dijkstra's algorithm.
        /// </summary>
        /// <param name="startPosition">The start position of search.</param>
        /// <param name="movementRange">The range of the search.</param>
        /// <param name="pathNodePool">The pool of PathNodes of the pathfinding caller.</param>
        /// <returns>A list of path nodes that can be reached from start position.</returns>
        public IList<PathNode> DijkstraPaths(Vector3 startPosition, int movementRange, PathNodePool pathNodePool)
        {
            CubicCoordinate startCoordinate = _Grid.WorldToCubic(startPosition);
            Hex startHex = _Grid.GetHex(startCoordinate);

            if (startHex == null)
            {
                return null;
            }

            PathNode startNode = pathNodePool.Get();
            startNode.CorrespondingHex = startHex;
            startNode.ParentNode = null;

            _Frontier.Clear();
            _VisitedHexes.Clear();
            _ReachableNodes.Clear();

            _Frontier.Enqueue(startNode);

            while (_Frontier.Count > 0)
            {
                PathNode currentPathNode = _Frontier.Dequeue();

                if (_VisitedHexes.Contains(currentPathNode.CorrespondingHex)) continue;
                _VisitedHexes.Add(currentPathNode.CorrespondingHex);
                _ReachableNodes.Add(currentPathNode);

                HexNeighbors neighbourHexes = currentPathNode.CorrespondingHex.Neighbors;

                foreach (Hex neighbourHex in neighbourHexes)
                {
                    if (_VisitedHexes.Contains(neighbourHex)
                        || neighbourHex == null
                        || neighbourHex.Flags == HexFlags.NonTraverseable
                        || _UnitManager.UnitPositions.ContainsKey(neighbourHex.Coordinates.Index)
                        || Hex.GetEdgeType(currentPathNode.CorrespondingHex.EnvironmentData.Elevation,
                            neighbourHex.EnvironmentData.Elevation) == HexEdgeType.Cliff)
                    {
                        continue;
                    }

                    PathNode neighbourNode = pathNodePool.Get();
                    neighbourNode.CorrespondingHex = neighbourHex;

                    int costToNeighbour = GetCostToNeighbour(currentPathNode.CorrespondingHex, neighbourHex);

                    int costSoFar = currentPathNode.GCost + costToNeighbour;

                    if (costSoFar < neighbourNode.GCost || !_VisitedHexes.Contains(neighbourHex))
                    {
                        neighbourNode.GCost = costSoFar;
                        neighbourNode.HCost = 0;
                        neighbourNode.ParentNode = currentPathNode;

                        if (neighbourNode.GCost <= movementRange)
                        {
                            _Frontier.Enqueue(neighbourNode);
                        }
                    }
                }
            }

            return _ReachableNodes;
        }

        /// <summary>
		/// Finds all possible paths within movement range using Dijkstra's algorithm and returns a hashset of all reachable hexes.
		/// </summary>
		/// <param name="startPosition">The hex that will be used as a start for the search.</param>
		/// <param name="movementRange">The range of the search.</param>
		/// <param name="pathNodePool">The pool of pathnodes to use for the search.</param>
		/// <returns>A hashset of hexes that are reachable from the start position.</returns>
        public HashSet<Hex> DijkstraPaths(Hex startPosition, int movementRange, PathNodePool pathNodePool)
        {
            DijkstraPaths(startPosition.Coordinates.World, movementRange, pathNodePool);
            return CopyVisitedHexes(_VisitedHexes);
        }

        /// <summary>
        /// Construct a path using the path nodes created in the Dijksta search.
        /// </summary>
        /// <param name="startPosition">The start position of the path.</param>
        /// <param name="endPosition">The end position of the path.</param>
        /// <returns>A list of PathNodes representing the constructed path.</returns>
        public List<Hex> ConstructDijkstraPath(Vector3 startPosition, Vector3 endPosition)
        {
            Hex startHex = _Grid.GetHexWorld(startPosition);
            Hex endHex = _Grid.GetHexWorld(endPosition);
            PathNode startNode = null;
            PathNode endNode = null;

            foreach (PathNode pathNode in _ReachableNodes)
            {
                if (pathNode.CorrespondingHex == startHex)
                {
                    startNode = pathNode;
                }
                if (pathNode.CorrespondingHex == endHex)
                {
                    endNode = pathNode;
                }
            }

            if (startNode == null || endNode == null)
            {
                return null;
            }

            return RetracePath(startNode, endNode);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if destination hex is in movement range from source hex.
        /// </summary>
        /// <param name="source">The source hex.</param>
        /// <param name="destination">The destination hex.</param>
        /// <param name="hexesInRange">A list of hexes in movement range.</param>
        /// <returns>True if the destination is in movement range, false if not.</returns>
        private bool IsInMovementRange(Hex source, Hex destination, IList<Hex> hexesInRange)
        {
            foreach (Hex hex in hexesInRange)
            {
                if (hex == destination) return true;
            }
            return false;
        }


        /// <summary>
        /// Gets the movement cost to move from the source hex to the destination hex.
        /// </summary>
        /// <param name="source">The source hex.</param>
        /// <param name="destination">The destination hex.</param>
        /// <returns>The movement cost to the destination hex, or -1 if not a neighbour.</returns>
        private int GetCostToNeighbour(Hex source, Hex destination)
        {
            if (source == destination)
            {
                return 0;
            }

            int qDistance = Mathf.Abs(source.Coordinates.Cubic.Q - destination.Coordinates.Cubic.Q);
            int rDistance = Mathf.Abs(source.Coordinates.Cubic.R - destination.Coordinates.Cubic.R);
            int sDistance = Mathf.Abs(source.Coordinates.Cubic.S - destination.Coordinates.Cubic.S);

            // Check that the destination is source's neighbour.
            if (qDistance > 1 || rDistance > 1 || sDistance > 1)
            {
                return -1;
            }

            int movementCost = destination.MovementCost;

            if (Hex.GetEdgeType(source.EnvironmentData.Elevation, destination.EnvironmentData.Elevation) == HexEdgeType.Slope
                && source.EnvironmentData.Elevation < destination.EnvironmentData.Elevation)
            {
                movementCost += GameManager.Instance.GameConfig.MovementConfig.AddedMovementCostFromSlope;
            }

            return movementCost;
        }

        /// <summary>
        /// Estimates the cost to reach the destination hex from the source hex using the
        /// cubic distance heuristic.
        /// </summary>
        /// <param name="source">The source hex.</param>
        /// <param name="destination">The destination hex.</param>
        /// <returns>The estimated cost to the destination hex.</returns>
        private int GetEstimatedCost(Hex source, Hex destination)
        {
            int absQ = Mathf.Abs(source.Coordinates.Cubic.Q - destination.Coordinates.Cubic.Q);
            int absR = Mathf.Abs(source.Coordinates.Cubic.R - destination.Coordinates.Cubic.R);
            int absS = Mathf.Abs(source.Coordinates.Cubic.S - destination.Coordinates.Cubic.S);

            return Mathf.Max(absQ, absR, absS);
        }

        /// <summary>
        /// Retraces the path from the end node to the start node using the ParentNode references.
        /// The path is then reversed so that it starts from the start node and ends at the end node.
        /// </summary>
        /// <param name="startNode">The start node of the path.</param>
        /// <param name="endNode">The end node of the path.</param>
        /// <returns>A list of PathNodes representing the reversed path.</returns>
        private List<Hex> RetracePath(PathNode startNode, PathNode endNode)
        {
            List<Hex> path = new List<Hex>();

            PathNode currentNode = endNode;
            bool isValid = true;

            while (currentNode != startNode && (isValid = currentNode != null))
            {
                path.Add(currentNode.CorrespondingHex);
                currentNode = currentNode.ParentNode;
            }

            if (!isValid)
            {
                return null;
            }

            path.Reverse();

            return path;
        }

        private HashSet<Hex> CopyVisitedHexes(HashSet<Hex> hashSet)
        {
            HashSet<Hex> newHashSet = new HashSet<Hex>();
            foreach (Hex hex in hashSet)
            {
                newHashSet.Add(hex);
            }

            return newHashSet;
        }

        #endregion
    }
}
