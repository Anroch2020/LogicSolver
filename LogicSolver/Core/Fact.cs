using System.Linq;

namespace LogicSolver.Core;

/// <summary>
/// Representa un hecho lógico
/// Ejemplo: (casa, color, rojo, 1) o (vive_en, juan, casa_azul)
/// </summary>
public record Fact(string Predicate, params Term[] Arguments)
{
    public bool IsGround => Arguments.All(arg => arg.IsGround);
    
    public int Arity => Arguments.Length;
    
    public override string ToString()
    {
        if (Arguments.Length == 0)
            return Predicate;
        
        return $"{Predicate}({string.Join(", ", Arguments.Select(a => a.ToString()))})";
    }

    /// <summary>
    /// Anulación del método Equals para comparar los hechos por valor y no por referencia.
    /// </summary>
    public virtual bool Equals(Fact? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        // Compara el predicado y la secuencia de argumentos.
        return Predicate == other.Predicate && Arguments.SequenceEqual(other.Arguments);
    }

    /// <summary>
    /// Anulación del GetHashCode para que coincida con la lógica de Equals.
    /// </summary>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Predicate);
        foreach (var term in Arguments)
        {
            hashCode.Add(term);
        }
        return hashCode.ToHashCode();
    }
    
    /// <summary>
    /// Verifica si este hecho unifica con otro
    /// </summary>
    public bool Unifies(Fact other, out Dictionary<Variable, Term> substitution)
    {
        substitution = new Dictionary<Variable, Term>();
        
        if (Predicate != other.Predicate || Arity != other.Arity)
            return false;
        
        for (int i = 0; i < Arguments.Length; i++)
        {
            if (!UnifyTerms(Arguments[i], other.Arguments[i], substitution))
                return false;
        }
        
        return true;
    }
    
    private static bool UnifyTerms(Term term1, Term term2, Dictionary<Variable, Term> substitution)
    {
        // Aplicar sustituciones existentes
        term1 = ApplySubstitution(term1, substitution);
        term2 = ApplySubstitution(term2, substitution);
        
        return (term1, term2) switch
        {
            (Variable var1, Term t2) => AddSubstitution(var1, t2, substitution),
            (Term t1, Variable var2) => AddSubstitution(var2, t1, substitution),
            (Constant c1, Constant c2) => c1.Value == c2.Value,
            (CompoundTerm ct1, CompoundTerm ct2) when ct1.Functor == ct2.Functor && ct1.Arguments.Length == ct2.Arguments.Length =>
                ct1.Arguments.Zip(ct2.Arguments).All(pair => UnifyTerms(pair.First, pair.Second, substitution)),
            _ => false
        };
    }
    
    private static bool AddSubstitution(Variable variable, Term term, Dictionary<Variable, Term> substitution)
    {
        if (substitution.ContainsKey(variable))
            return UnifyTerms(substitution[variable], term, substitution);
        
        if (OccursCheck(variable, term))
            return false;
        
        substitution[variable] = term;
        return true;
    }
    
    private static bool OccursCheck(Variable variable, Term term)
    {
        return term switch
        {
            Variable v => v.Name == variable.Name,
            CompoundTerm ct => ct.Arguments.Any(arg => OccursCheck(variable, arg)),
            _ => false
        };
    }
    
    private static Term ApplySubstitution(Term term, Dictionary<Variable, Term> substitution)
    {
        return term switch
        {
            Variable var when substitution.ContainsKey(var) => ApplySubstitution(substitution[var], substitution),
            CompoundTerm ct => new CompoundTerm(ct.Functor, ct.Arguments.Select(arg => ApplySubstitution(arg, substitution)).ToArray()),
            _ => term
        };
    }
    
    /// <summary>
    /// Aplica una sustitución a este hecho
    /// </summary>
    public Fact ApplySubstitution(Dictionary<Variable, Term> substitution)
    {
        var newArgs = Arguments.Select(arg => ApplySubstitution(arg, substitution)).ToArray();
        return new Fact(Predicate, newArgs);
    }
}