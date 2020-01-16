using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusCore
{
    public interface IModbusSlave
    {
        void HandleAnyRequest();
    }
}
