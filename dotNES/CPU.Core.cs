﻿using System;

namespace dotNES
{
    sealed partial class CPU
    {
        public enum InterruptType
        {
            NMI, IRQ, RESET
        }

        private readonly uint[] _interruptHandlerOffsets = { 0xFFFA, 0xFFFE, 0xFFFC };
        private readonly bool[] _interrupts = new bool[2];

        public void Initialize()
        {
            A = 0;
            X = 0;
            Y = 0;
            SP = 0xFD;
            P = 0x24;

            PC = ReadWord(_interruptHandlerOffsets[(int)InterruptType.RESET]);
        }

        public void Reset()
        {
            SP -= 3;
            F.InterruptsDisabled = true;
        }

        public void TickFromPPU()
        {
            if (Cycle-- > 0) return;
            ExecuteSingleInstruction();
        }

        public void ExecuteSingleInstruction()
        {
            for (int i = 0; i < _interrupts.Length; i++)
            {
                if (_interrupts[i])
                {
                    PushWord(PC);
                    Push(P);
                    PC = ReadWord(_interruptHandlerOffsets[i]);
                    F.InterruptsDisabled = true;
                    _interrupts[i] = false;
                    return;
                }
            }

            _currentInstruction = NextByte();

            Cycle += _opcodeDefs[_currentInstruction].Cycles;

            ResetInstructionAddressingMode();


            Opcode op = _opcodes[_currentInstruction];
            if (op == null)
                throw new ArgumentException(_currentInstruction.ToString("X2"));
            op();
        }

        public void TriggerInterrupt(InterruptType type)
        {
            if (!F.InterruptsDisabled || type == InterruptType.NMI)
                _interrupts[(int)type] = true;
        }
    }
}
