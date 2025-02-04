using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int curItemCount;

    public Heap(int maxHeapSize) {
        items = new T[maxHeapSize];
    }

    public void Add(T item) {
        item.HeapIndex = curItemCount;
        // adds new item to end of heap array
        items[curItemCount] = item;
        // sorts the heap to maintain the order
        SortUp(item);
        curItemCount++;
    }

    public T RemoveFirst() {
        // saves first item
        T firtItem = items[0];
        curItemCount--;
        // places the last item in the head 
        // at the front and updates its heapIndex
        items[0] = items[curItemCount];
        items[0].HeapIndex = 0;
        // sorts the new root down to maintain the heap order
        SortDown(items[0]);
        return firtItem;
    }
    
    public void UpdateItem(T item) {
        // updates item's order if priority is increased
        SortUp(item);
    }
    public int Count {
        get {
            return curItemCount;
        }
    }
    public bool Contains(T item) {
        // checks if item is in items
        return Equals(items[item.HeapIndex], item);
    }
    
    private void SortUp(T item) {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true) {
            T parentItem = items[parentIndex];
            // if item has a higher priority than its parent
            if (item.CompareTo(parentItem) > 0) {
                // swaps if needed
                Swap(item, parentItem);
            }
            // if the new item is in its correct place in the heap
            else {
                break;
            }
            // gets parent index of where the item is after being swapped
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void SortDown(T item) {
        while (true) {
            int leftChildIndex = item.HeapIndex * 2 + 1;
            int rightChildIndex = item.HeapIndex * 2 + 2;
            int swapIndex = 0;
            // tries to swap the item with its highest priority child
            // frist checks if left child exists
            if (leftChildIndex < curItemCount) {
                // saves index to swap
                swapIndex = leftChildIndex;
                // checks if right child exists
                if (rightChildIndex < curItemCount) {
                    // if right child has a higher priority
                    // then it saves the index to swap
                    if (items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0) {
                        swapIndex = rightChildIndex;
                    }
                }
                if (item.CompareTo(items[swapIndex]) < 0) {
                    // item swaps with its highest priority child
                    Swap(item, items[swapIndex]);
                }
                // if item is higher priority than both of its children
                // then its in the correct spot
                else {
                    return;
                }
            }
            // if item has no children then its in its correct spot
            else {
                return;
            }
        }
    }

    private void Swap(T itemA, T itemB) {
        // swaps contents in items
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        // swpas indexes
        int tempItemA = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = tempItemA;
    }
}
public interface IHeapItem<T> : IComparable<T>{
    public int HeapIndex {
        get;
        set;
    }
}
