using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyBF.BFI
{
    public class Interpreter
    {
        public void Run(Instruction[] instructions)
        {
            byte[] memory = new byte[30000];

            int iptr = 0;
            int memptr = 0;
            Instruction instruction;

            using (var reader = new BinaryReader(Console.OpenStandardInput()))
            using (var writer = new BinaryWriter(Console.OpenStandardOutput()))
            {
                while (iptr < instructions.Length)
                {
                    instruction = instructions[iptr];

                    switch (instruction.Operation)
                    {
                        case Operation.Zero:
                            memory[memptr] = 0;
                            iptr++;
                            break;

                        case Operation.Add:
                            memory[memptr] = (byte)(memory[memptr] + instruction.Operand);
                            iptr++;
                            break;

                        case Operation.AddScale:
                            int offsetAddress = memptr + instruction.Operand;
                            CheckCapacity(ref memory, offsetAddress);

                            if (offsetAddress >= 0)
                                memory[offsetAddress] = (byte)(memory[offsetAddress] + memory[memptr] * instructions[iptr].Factor);

                            iptr++;
                            break;

                        case Operation.Shift:
                            memptr += instruction.Operand;
                            CheckCapacity(ref memory, memptr);
                            iptr++;
                            break;

                        case Operation.Print:
                            writer.Write(memory[memptr]);
                            iptr++;
                            break;

                        case Operation.Read:

                            byte input;

                            try {
                                input = reader.ReadByte(); }
                            catch (EndOfStreamException) {
                                input = memory[memptr]; }

                            memory[memptr] = input;
                            iptr++;
                            break;

                        case Operation.JumpIfZero:
                            iptr = (memory[memptr] == 0 ? instruction.Operand : iptr + 1);
                            break;

                        case Operation.JumpIf:
                            iptr = (memory[memptr] != 0 ? instruction.Operand : iptr + 1);
                            break;

                        default:
                            throw new BFProgramError("Unrecognized program instruction: " + instruction.Operation.ToString());
                    }
                }
            }
        }

        private void CheckCapacity(ref byte[] array, int address)
        {
            if (array.Length <= address)
            {
                byte[] resized = new byte[(address + 1) * 2];
                array.CopyTo(resized, 0);
                array = resized;
            }
        }
    }
}
