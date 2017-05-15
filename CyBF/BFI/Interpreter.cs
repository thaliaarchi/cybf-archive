using System.IO;

namespace CyBF.BFI
{
    public class Interpreter
    {
        public int InitialMemoryCapacity { get; set; }

        public Interpreter()
        {
            this.InitialMemoryCapacity = 30000;
        }

        public void Run(Instruction[] instructions, Stream input, Stream output)
        {
            byte[] memory = new byte[this.InitialMemoryCapacity];

            int iptr = 0;
            int memptr = 0;
            Instruction instruction;

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

                        if (memory[memptr] > 0)
                        {
                            int offsetAddress = memptr + instruction.Operand;
                            CheckAddress(ref memory, offsetAddress);
                            memory[offsetAddress] = (byte)(memory[offsetAddress] + memory[memptr] * instructions[iptr].Factor);
                        }

                        iptr++;
                        break;

                    case Operation.Shift:
                        memptr += instruction.Operand;
                        CheckAddress(ref memory, memptr);
                        iptr++;
                        break;

                    case Operation.Print:
                        output.Write(memory, memptr, 1);
                        iptr++;
                        break;

                    case Operation.Read:
                        input.Read(memory, memptr, 1);
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

        private void CheckAddress(ref byte[] array, int address)
        {
            if (address < 0)
                throw new BFProgramError("Negative memory address referenced.");

            if (array.Length <= address)
            {
                byte[] resized = new byte[(address + 1) * 2];
                array.CopyTo(resized, 0);
                array = resized;
            }
        }
    }
}
