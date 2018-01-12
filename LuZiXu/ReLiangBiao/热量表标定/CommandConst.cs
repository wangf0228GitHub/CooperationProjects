using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 热量表标定
{
    public class CommandConst
    {
        public const byte CP68_UserCommand_ReadAD = 0x21;
        public const byte CP68_UserCommand_WriteTCalc = 0x22;
        public const byte CP68_UserCommand_ReadPress = 0x23;
        public const byte CP68_UserCommand_WriteQCalc = 0x24;
        public const byte CP68_UserCommand_WriteDeviceParams = 0x25; 
    }
}
