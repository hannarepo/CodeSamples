using System;
using BellumCorpus.Grid;

namespace BellumCorpus.Pathfinding
{
    /// <summary>
    /// A data class for storing path data.
    /// </summary>
    public class PathNode : IComparable
    {
        public PathNode(int gCost, int hCost)
        {
            GCost = gCost;
            HCost = hCost;
        }

        /// <summary>
        /// Cost of the path so far (until this node).
        /// </summary>
        public int GCost { get; set; }

        /// <summary>
        /// The estimated cost from this node to the end node.
        /// </summary>
        public int HCost { get; set; }
        
        /// <summary>
        /// The total (estimated) cost of the path at this node.
        /// </summary>
        public int FCost => GCost + HCost;

        /// <summary>
        /// Reference to the node where this node can be reached from.
        /// </summary>
        public PathNode ParentNode { get; set; }

        /// <summary>
        /// The hex where this node is located.
        /// </summary>
        public Hex CorrespondingHex { get; set; }

        /// <summary>
        /// Compares the FCosts of two nodes. This nodes FCost is subrtacted from the compared nodes
        /// FCost so that the smallest FCost is the first priority.
        /// </summary>
        /// <param name="other">The node this node is compared to.</param>
        /// <returns>The subrtaction of the FCosts of the compared nodes. Negative other is null.</returns>
        public int CompareTo(Object other)
        {
            if (other == null) return -1;

            PathNode otherPathNode = other as PathNode;
            return otherPathNode.FCost - FCost;
        }

        /// <summary>
        /// Reset this nodes values.
        /// </summary>
        public void Reset()
        {
            GCost = 0;
            HCost = 0;
            ParentNode = null;
            CorrespondingHex = null;
        }
    }
}