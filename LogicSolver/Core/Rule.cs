namespace LogicSolver.Core;

/// <summary>
/// Representa una regla lógica: Antecedente -> Consecuente
/// Ejemplo: (vive_en, X, casa_roja) -> (es, X, ingles)
/// </summary>
public record Rule(List<Fact> Antecedents, Fact Consequent)
{
    public Rule(Fact antecedent, Fact consequent) : this([antecedent], consequent) { }
    
    public override string ToString()
    {
        var antecedentStr = string.Join(" AND ", Antecedents.Select(a => a.ToString()));
        return $"{antecedentStr} -> {Consequent}";
    }
    
    /// <summary>
    /// Intenta aplicar esta regla dado un conjunto de hechos conocidos
    /// </summary>
    public IEnumerable<Fact> Apply(HashSet<Fact> knownFacts)
    {
        var results = new List<Fact>();
        
        // Buscar todas las combinaciones de hechos conocidos que unifiquen con los antecedentes
        var matchingSets = FindMatchingFactSets(knownFacts, 0, new Dictionary<Variable, Term>());
        
        foreach (var substitution in matchingSets)
        {
            var newFact = Consequent.ApplySubstitution(substitution);
            if (newFact.IsGround)
            {
                results.Add(newFact);
            }
        }
        
        return results;
    }
    
    private IEnumerable<Dictionary<Variable, Term>> FindMatchingFactSets(
        HashSet<Fact> knownFacts, 
        int antecedentIndex, 
        Dictionary<Variable, Term> currentSubstitution)
    {
        if (antecedentIndex >= Antecedents.Count)
        {
            yield return new Dictionary<Variable, Term>(currentSubstitution);
            yield break;
        }
        
        var currentAntecedent = Antecedents[antecedentIndex];
        
        foreach (var fact in knownFacts)
        {
            if (currentAntecedent.Unifies(fact, out var newSubstitution))
            {
                // Combinar sustituciones
                var combinedSubstitution = new Dictionary<Variable, Term>(currentSubstitution);
                bool compatible = true;
                
                foreach (var (variable, term) in newSubstitution)
                {
                    if (combinedSubstitution.ContainsKey(variable))
                    {
                        // Verificar compatibilidad
                        if (!TermsEqual(combinedSubstitution[variable], term))
                        {
                            compatible = false;
                            break;
                        }
                    }
                    else
                    {
                        combinedSubstitution[variable] = term;
                    }
                }
                
                if (compatible)
                {
                    // Recursivamente buscar coincidencias para el siguiente antecedente
                    foreach (var result in FindMatchingFactSets(knownFacts, antecedentIndex + 1, combinedSubstitution))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
    
    private static bool TermsEqual(Term term1, Term term2)
    {
        return (term1, term2) switch
        {
            (Constant c1, Constant c2) => c1.Value == c2.Value,
            (Variable v1, Variable v2) => v1.Name == v2.Name,
            (CompoundTerm ct1, CompoundTerm ct2) => ct1.Functor == ct2.Functor && 
                ct1.Arguments.Length == ct2.Arguments.Length &&
                ct1.Arguments.Zip(ct2.Arguments).All(pair => TermsEqual(pair.First, pair.Second)),
            _ => false
        };
    }
}