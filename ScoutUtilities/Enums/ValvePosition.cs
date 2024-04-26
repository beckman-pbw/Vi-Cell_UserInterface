namespace ScoutUtilities.Enums
{
    public enum ValvePosition
    {
        ValveA = 1,
        ValveB,
        ValveC,
        ValveD,
        ValveE,
        ValveF,
        ValveG,
        ValveH
    }

    public class ValvePositionMap
    {
        /// <summary>
        /// Convert a name such as ValveE to a single character 'E'. If the provided string
        /// does not provide a character in the range of 'A'-'H', then return 'A'.
        /// </summary>
        /// <param name="valveName">A valveName name in the format "Valve[A-H]".</param>
        /// <returns>A single character 'A'-'H'.</returns>
        public static ValvePosition ValveNameToValvePosition(string valveName)
        {
            var valveLetter = valveName?.Length == 6 && valveName.StartsWith("Valve") ? valveName[5] : 'A';
            if (valveLetter < 'A' || valveLetter > 'H')
            {
                valveLetter = 'A';
            }

            return ValvePositionMap.CharToValvePosition(valveLetter);
        }

        public static char ValvePositionToChar(ValvePosition valve)
        {
            switch (valve)
            {
                case ValvePosition.ValveA:
                    return 'A';
                case ValvePosition.ValveB:
                    return 'B';
                case ValvePosition.ValveC:
                    return 'C';
                case ValvePosition.ValveD:
                    return 'D';
                case ValvePosition.ValveE:
                    return 'E';
                case ValvePosition.ValveF:
                    return 'F';
                case ValvePosition.ValveG:
                    return 'G';
                case ValvePosition.ValveH:
                    return 'H';
                default:
                    return 'A';
            }
        }

        public static ValvePosition BackendToValvePosition(ushort valvePositionFromBackend)
        {
            switch (valvePositionFromBackend)
            {
                default:
                case 1:
                    return ValvePosition.ValveF;
                case 2:
                    return ValvePosition.ValveG;
                case 3:
                    return ValvePosition.ValveH;
                case 4:
                    return ValvePosition.ValveA;
                case 5:
                    return ValvePosition.ValveB;
                case 6:
                    return ValvePosition.ValveC;
                case 7:
                    return ValvePosition.ValveD;
                case 8:
                    return ValvePosition.ValveE;
            }
        }

        public static ValvePosition CharToValvePosition(char valvePosition)
        {
            switch (valvePosition)
            {
                case 'A':
                    return ValvePosition.ValveA;
                case 'B':
                    return ValvePosition.ValveB;
                case 'C':
                    return ValvePosition.ValveC;
                case 'D':
                    return ValvePosition.ValveD;
                case 'E':
                    return ValvePosition.ValveE;
                case 'F':
                    return ValvePosition.ValveF;
                case 'G':
                    return ValvePosition.ValveG;
                case 'H':
                    return ValvePosition.ValveH;
                default:
                    return ValvePosition.ValveA;
            }
        }
    }
}
