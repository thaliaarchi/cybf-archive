namespace CyBF.BFI
{
    public enum Operation
    {
        Zero,               // Zero
        Add,                // Add <Amount>
        AddScale,           // AddScale <Offset> <Factor>
        Shift,              // Shift <Amount>
        Print,              // Print
        Read,               // Read
        JumpIfZero,         // JumpIfZero <Address>
        JumpIf              // JumpIf <Address>
    }
}
