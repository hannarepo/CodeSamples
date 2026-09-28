using System;
using System.Collections.Generic;

namespace BellumCorpus.Collections
{
    public abstract class ListPool<T> : IDisposable
        where T : class
    {
        protected List<(T, bool, int)> _Items;
        private bool _CanGrow = false;

        /// <summary>
        /// Creates a new List Pool with the given items.
        /// </summary>
        /// <param name="itemsToAdd">The list of items to add to the pool.</param>
        /// <param name="canGrow">Can the pool grow when full.</param>
        /// <exception cref="NullReferenceException">If the list of items to add is empty.</exception>
        protected ListPool(List<T> itemsToAdd, bool canGrow = false)
        {
            if (itemsToAdd.Count <= 0)
            {
                throw new NullReferenceException("No items in list of items to add!");
            }

            _Items = new List<(T, bool, int)>(itemsToAdd.Count);
            _CanGrow = canGrow;

            for (int i = 0; i < itemsToAdd.Count; i++)
            {
                Add(itemsToAdd[i], true, i);
            }
        }

        #region Public Interface

        /// <summary>
        /// Gets the first available item from the pool. Changes its state to not in pool.
        /// If the pool is empty and can't grow, returns null.
        /// If the item is not in contained in the pool and the pool can grow, add the new item
        /// to the pool and then return it.
        /// </summary>
        /// <returns>The first available item from the pool.</returns>
        /// <exception cref="ArgumentNullException">If the item is null.</exception>
        /// <exception cref="Exception">If the item is not in the pool.</exception>
        public virtual T Get()
        {
            (T, bool, int) item = (null, true, 0);

            for (int i = 0; i < _Items.Count; i++)
            {
                (T, bool, int) currentItem = _Items[i];
                if (currentItem.Item1 == null)
                {
                    throw new ArgumentNullException("Item in the pool was null!");
                }

                if (IsInPool(currentItem))
                {
                    currentItem.Item2 = false;
                    currentItem.Item3 = i;
                    item = currentItem;
                    _Items[i] = currentItem;
                    break;
                }
            }

            if (_Items.Contains(item))
            {
                item.Item2 = false;
            }
            else if (_CanGrow)
            {
                item = AddAndGetItem();
            }

            return item.Item1;
        }

        /// <summary>
        /// Returns an item back to the pool. Changes its state to in pool.
        /// If the item is null or not from this pool, returns false.
        /// </summary>
        /// <returns>The returned item.
        public virtual (T, bool, int) Return((T, bool, int) item)
        {
            if (item.Item1 == null || !_Items.Contains(item))
            {
                return (null, false, 0);
            }

            item.Item2 = true;
            _Items[item.Item3] = item;
            return item;
        }

        /// <summary>
        /// Returns all items back to the pool.
        /// </summary>
        public void ReturnAll()
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                (T, bool, int) item = _Items[i];
                item = Return(item);
            }
        }

        /// <summary>
        /// Disposes the pool.
        /// </summary>
        public virtual void Dispose()
        {
            _Items = null;
        }

        #endregion

        private void Add(T item, bool isInPool, int index)
        {
            _Items.Add((item, isInPool, index));
        }

        protected abstract bool IsInPool((T, bool, int) item);
        protected abstract (T, bool, int) AddAndGetItem();
    }
}