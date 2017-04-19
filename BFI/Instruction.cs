namespace CyBF.BFI
{
    public struct Instruction
    {
        public Operation Operation;
        public int Operand;
        public int Factor;

        public override string ToString()
        {
            return string.Join(" ", this.Operation, this.Operand, this.Factor);
        }

        public static Instruction Zero()
        {
            return new Instruction()
            {
                Operation = Operation.Zero
            };
        }

        public static Instruction Add(int amount)
        {
            return new Instruction()
            {
                Operation = Operation.Add,
                Operand = amount
            };
        }

        public static Instruction AddScale(int offset, int factor)
        {
            return new Instruction()
            {
                Operation = Operation.AddScale,
                Operand = offset,
                Factor = factor
            };
        }

        public static Instruction Shift(int amount)
        {
            return new Instruction()
            {
                Operation = Operation.Shift,
                Operand = amount
            };
        }

        public static Instruction Print()
        {
            return new Instruction()
            {
                Operation = Operation.Print
            };
        }

        public static Instruction Read()
        {
            return new Instruction()
            {
                Operation = Operation.Read
            };
        }

        public static Instruction JumpIfZero(int address)
        {
            return new Instruction()
            {
                Operation = Operation.JumpIfZero,
                Operand = address
            };
        }

        public static Instruction JumpIf(int address)
        {
            return new Instruction()
            {
                Operation = Operation.JumpIf,
                Operand = address
            };
        }
    }
}
