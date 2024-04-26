namespace ScoutUtilities.EventDomain
{
    public class MotorFocusPosition
    {
        public MotorFocusPosition(int definedFocusPosition)
        {
            FocusPosition = definedFocusPosition;
        }

        public int FocusPosition { get; }
    }
}
