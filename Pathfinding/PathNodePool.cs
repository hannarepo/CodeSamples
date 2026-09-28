using System;
using System.Collections.Generic;
using BellumCorpus.Collections;

namespace BellumCorpus.Pathfinding
{
    public class PathNodePool : ListPool<PathNode>
    {
        public PathNodePool(List<PathNode> item, bool canGrow = false)
            : base(item, canGrow)
        {
        }

        protected override bool IsInPool((PathNode, bool, int) item)
        {
            return item.Item2;
        }

        protected override (PathNode, bool, int) AddAndGetItem()
        {
            (PathNode, bool, int) item = (new PathNode(0, 0), false, _Items.Count);
            _Items.Add(item);
            return item;
        }

        /// <summary>
        /// Reset path node values and return it to pool.
        /// </summary>
        /// <param name="item">Path node to be returned and it's in pool status.</param>
        /// <returns>Path node and its in pool status.</returns>
        public override (PathNode, bool, int) Return((PathNode, bool, int) item)
        {
            if (item.Item1 != null)
            {
                item.Item1.Reset();
            }
            return base.Return(item);
        }
    }
}