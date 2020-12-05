using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore.Tests
{
    public class MockedMemoryStream : MemoryStream
    {
        private StreamOperation _lastOperation; 
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_lastOperation != StreamOperation.Write)
            {
                Seek(0, SeekOrigin.Begin);
                _lastOperation = StreamOperation.Write;
            }

            base.Write(buffer, offset, count);            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_lastOperation != StreamOperation.Read)
            {
                Seek(0, SeekOrigin.Begin);
                _lastOperation = StreamOperation.Read;
            }

            return base.Read(buffer, offset, count);
        }

    }

    internal enum StreamOperation
    {
        Unknown,

        Write,

        Read
    }
}
