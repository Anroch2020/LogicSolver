namespace LogicSolver.Core;

/// <summary>
/// Representa un término en lógica: puede ser una constante, variable o término compuesto
/// </summary>
public abstract record Term
{
    public abstract bool IsVariable { get; }
    public abstract bool IsGround { get; } // No contiene variables
}

/// <summary>
/// Término constante (ej: "casa", "rojo", "1")
/// </summary>
public record Constant(string Value) : Term
{
    public override bool IsVariable => false;
    public override bool IsGround => true;

    public override string ToString() => Value;
}

/// <summary>
/// Variable lógica (ej: "X", "Y")
/// </summary>
public record Variable(string Name) : Term
{
    public override bool IsVariable => true;
    public override bool IsGround => false;

    public override string ToString() => Name;
}

/// <summary>
/// Término compuesto (ej: casa(color, rojo))
/// </summary>
public record CompoundTerm(string Functor, params Term[] Arguments) : Term
{
    public override bool IsVariable => false;
    public override bool IsGround => Arguments.All(arg => arg.IsGround);

    public override string ToString()
    {
        if (Arguments.Length == 0)
            return Functor;

        return $"{Functor}({string.Join(", ", Arguments.Select(a => a.ToString()))})";
    }
}