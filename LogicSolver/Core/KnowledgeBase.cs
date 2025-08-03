using System.Data;

namespace LogicSolver.Core;

/// <summary>
/// Base de conocimiento que almacena hechos y reglas
/// </summary>
public class KnowledgeBase
{
    public HashSet<Fact> Facts { get; private set; } = new();
    public List<Rule> Rules { get; private set; } = new();

    public void AddFact(Fact fact)
    {
        Facts.Add(fact);
    }

    public void AddRule(Rule rule)
    {
        Rules.Add(rule);
    }

    public void AddFacts(IEnumerable<Fact> facts)
    {
        foreach (var fact in facts)
        {
            Facts.Add(fact);
        }
    }

    /// <summary>
    /// Aplica forward chaining: deduce nuevos hechos aplicando las reglas
    /// </summary>
    public bool ForwardChain()
    {
        bool newFactsAdded = false;

        Console.WriteLine($"    Forward chaining: {Facts.Count} hechos iniciales");

        // Usamos una lista para los nuevos hechos y la verificamos con .Contains()
        var newFacts = new List<Fact>();

        foreach (var rule in Rules)
        {
            Console.WriteLine($"    Aplicando regla: {rule}");

            var generatedFacts = rule.Apply(Facts);
            int addedCount = 0;

            foreach (var newFact in generatedFacts)
            {
                // Ahora que Fact.Equals funciona, podemos usar .Contains() en ambas colecciones.
                if (!Facts.Contains(newFact) && !newFacts.Contains(newFact))
                {
                    newFacts.Add(newFact);
                    addedCount++;
                    Console.WriteLine($"      ✓ Nuevo hecho: {newFact}");
                }
                else
                {
                    Console.WriteLine($"      - Hecho duplicado ignorado: {newFact}");
                }
            }

            if (addedCount == 0)
            {
                Console.WriteLine($"      - No se generaron nuevos hechos");
            }
        }

        if (newFacts.Any())
        {
            foreach (var fact in newFacts)
            {
                Facts.Add(fact);
            }
            newFactsAdded = true;
        }

        Console.WriteLine($"    Resultado: {newFacts.Count} hechos nuevos agregados");

        if (Facts.Count > 100)
        {
            Console.WriteLine($"    ⚠️ LÍMITE ALCANZADO: {Facts.Count} hechos. Deteniendo forward chaining.");
            return false;
        }

        return newFactsAdded;
    }

    public KnowledgeBase Clone()
    {
        var clone = new KnowledgeBase();
        clone.Facts = new HashSet<Fact>(Facts);
        clone.Rules = new List<Rule>(Rules);
        return clone;
    }
}