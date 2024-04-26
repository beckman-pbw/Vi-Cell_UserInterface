using System;
using System.Collections.Generic;

namespace ScoutUtilities.Helper
{
    public class WrapAroundIndexer
    {
        public int StartingIndex { get; set; }
        public int MaxIndex { get; set; }

        public WrapAroundIndexer(int maxNumberOfElements, bool startingIndexIsZero)
        {
            MaxIndex = maxNumberOfElements;
            StartingIndex = startingIndexIsZero ? 0 : 1;
        }

        public int GetNextIndex(int index)
        {
            if (index + 1 > MaxIndex) return StartingIndex;
            return index + 1;
        }

        public int GetNextIndex(int currentIndex, int numberToTraverse)
        {
            var indexToReturn = currentIndex;

            for (var i = 0; i < numberToTraverse; i++)
            {
                indexToReturn = GetNextIndex(indexToReturn);
            }

            return indexToReturn;
        }

        public List<int> GetNextIndexes(int currentIndex, int numberToTraverse)
        {
            var tempIndex = currentIndex;
            var list = new List<int>();

            for (var i = 0; i < numberToTraverse; i++)
            {
                tempIndex = GetNextIndex(tempIndex);
                list.Add(tempIndex);
            }

            return list;
        }

        public int GetPreviousIndex(int index)
        {
            if (index - 1 < StartingIndex) return MaxIndex;
            return index - 1;
        }

        public int GetPreviousIndex(int currentIndex, int numberToTraverse)
        {
            var indexToReturn = currentIndex;

            for (var i = 0; i < numberToTraverse; i++)
            {
                indexToReturn = GetPreviousIndex(indexToReturn);
            }

            return indexToReturn;
        }

        public List<int> GetPreviousIndexes(int currentIndex, int numberToTraverse)
        {
            var tempIndex = currentIndex;
            var list = new List<int>();

            for (var i = 0; i < numberToTraverse; i++)
            {
                tempIndex = GetPreviousIndex(tempIndex);
                list.Add(tempIndex);
            }

            return list;
        }

        public List<int> GetSurroundingIndexes(int currentIndex, int numberOfSurroundingToCollect)
        {
            var toTheLeft = (int)Math.Floor((double)numberOfSurroundingToCollect / 2);
            var toTheRight = numberOfSurroundingToCollect - toTheLeft - 1; // -1 because the current index will be included

            var list = new List<int> { currentIndex };
            list.AddRange(GetPreviousIndexes(currentIndex, toTheRight));
            list.AddRange(GetNextIndexes(currentIndex, toTheLeft));
            return list;
        }
    }
}