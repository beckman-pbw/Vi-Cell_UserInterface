namespace ScoutUtilities.Enums
{
    public enum RowPosition : uint
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        Y = 98, // used in Automation Cup
        Z = 99, // used in Carousel
    }

    public static class RowPositionHelper
    {
        public static char GetRowChar(RowPosition position)
        {
            switch (position)
            {
                case RowPosition.A: return 'A';
                case RowPosition.B: return 'B';
                case RowPosition.C: return 'C';
                case RowPosition.D: return 'D';
                case RowPosition.E: return 'E';
                case RowPosition.F: return 'F';
                case RowPosition.G: return 'G';
                case RowPosition.H: return 'H';
                case RowPosition.Y: return 'Y';
                default: return 'Z';
            }
        }

        public static RowPosition GetRowPosition(char row)
        {
            switch (row)
            {
                case 'A': return RowPosition.A;
                case 'B': return RowPosition.B;
                case 'C': return RowPosition.C;
                case 'D': return RowPosition.D;
                case 'E': return RowPosition.E;
                case 'F': return RowPosition.F;
                case 'G': return RowPosition.G;
                case 'H': return RowPosition.H;
                case 'Y': return RowPosition.Y;
                default: return RowPosition.Z;
            }
        }
    }
}