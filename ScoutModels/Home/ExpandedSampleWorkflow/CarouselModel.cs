using ScoutUtilities.Common;
using System.Collections.Generic;

namespace ScoutModels.ExpandedSampleWorkflow
{
    public class CarouselModel
    {
        #region Singleton

        private static CarouselModel _instance;
        public static CarouselModel Instance => _instance ?? (_instance = new CarouselModel(ApplicationConstants.CarouselSampleCount));

        private CarouselModel(int numPositions)
        {
            lock (_sampleWellLock)
            {
                SampleWells = new Dictionary<int, SampleWellModel>(numPositions);
                for (var i = ApplicationConstants.StartingIndexOfCarousel; i <= numPositions; i++)
                {
                    SampleWells.Add(i, new SampleWellModel(i));
                }
            }

            SetTopCarouselPosition(ApplicationConstants.StartingIndexOfCarousel);
        }

        #endregion

        #region Properties & Fields

        private object _sampleWellLock = new object();
        private object _topPositionLock = new object();

        public int TopCarouselPosition { get; private set; }
        public Dictionary<int, SampleWellModel> SampleWells { get; private set; }

        #endregion

        #region Methods

        public void Add(SampleModel sample)
        {
            lock (_sampleWellLock)
            {
                SampleWells[sample.CarouselPosition].Sample = sample;
            }
        }

        public void Remove(SampleModel sample)
        {
            Remove(sample.CarouselPosition);
        }

        public void Remove(int index)
        {
            lock (_sampleWellLock)
            {
                if (SampleWells.ContainsKey(index))
                {
                    SampleWells[index].Sample = null;
                }
            }
        }

        public void ClearAll()
        {
            lock (_sampleWellLock)
            {
                for (var i = ApplicationConstants.StartingIndexOfCarousel; i <= ApplicationConstants.CarouselSampleCount; i++)
                {
                    if (SampleWells.ContainsKey(i))
                    {
                        SampleWells[i].Sample = null;
                    }
                }
            }
        }

        public void IncrementTopCarouselPosition()
        {
            lock (_topPositionLock)
            {
                if (TopCarouselPosition == SampleWells.Count) TopCarouselPosition = ApplicationConstants.StartingIndexOfCarousel;
                else TopCarouselPosition++;
            }
        }

        public void SetTopCarouselPosition(int topPosition)
        {
            lock (_topPositionLock)
            {
                if (topPosition > SampleWells.Count)
                {
                    TopCarouselPosition = SampleWells.Count;
                }
                else if (topPosition < ApplicationConstants.StartingIndexOfCarousel)
                {
                    TopCarouselPosition = ApplicationConstants.StartingIndexOfCarousel;
                }
                else TopCarouselPosition = topPosition;
            }
        }

        #endregion
    }
}