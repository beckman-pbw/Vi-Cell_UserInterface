namespace ScoutDomains
{
    public class AcupCalibrationConcentrationListDomain : CalibrationConcentrationListDomain
    {
        /// <summary>
        /// Used to keep track of which sample number this is (0 -> 17)
        /// </summary>
        public int Index
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool IsComplete
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}