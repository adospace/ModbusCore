﻿namespace ModbusCore
{
    public interface IModbusCoilsTable
    {
        bool this[int index] { get; set; }

        int Length { get; }
    }
}