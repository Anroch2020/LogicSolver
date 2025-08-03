
using LogicSolver.Core;

namespace LogicSolver.Inference;

/// <summary>
/// Motor de inferencia que utiliza forward chaining y backtracking
/// </summary>
public class InferenceEngine
{
    private readonly KnowledgeBase _knowledgeBase;

    public InferenceEngine(KnowledgeBase knowledgeBase)
    {
        _knowledgeBase = knowledgeBase;
    }

    /// <summary>
    /// Resuelve el acertijo aplicando inferencia hasta encontrar una solución completa
    /// </summary>
    public Solution? Solve()
    {
        Console.WriteLine("🔍 Iniciando motor de inferencia...");

        var workingKb = _knowledgeBase.Clone();
        int inferenceSteps = 0;
        const int MAX_STEPS = 10;

        Console.WriteLine("\n⚡ Aplicando forward chaining para deducir hechos...");

        // Bucle de Forward Chaining mejorado para detectar estabilidad
        while (true)
        {
            int factsBefore = workingKb.Facts.Count;

            // Se asume que ForwardChain intenta añadir todos los hechos deducibles en una pasada
            workingKb.ForwardChain();

            int factsAfter = workingKb.Facts.Count;
            bool newFactsWereAdded = factsAfter > factsBefore;

            if (!newFactsWereAdded)
            {
                Console.WriteLine("  No se dedujeron nuevos hechos. El conocimiento es estable.");
                break; // Salir del bucle si no hay cambios
            }

            inferenceSteps++;
            Console.WriteLine(
                $"  Paso {inferenceSteps}: Se agregaron {factsAfter - factsBefore} hechos nuevos. Total: {factsAfter}");

            if (inferenceSteps >= MAX_STEPS)
            {
                Console.WriteLine($"  ⚠️ LÍMITE DE PASOS ALCANZADO ({MAX_STEPS}). Deteniendo forward chaining.");
                break;
            }
        }

        Console.WriteLine($"\n✓ Forward chaining completado. Total de hechos generados: {workingKb.Facts.Count}");

        Console.WriteLine("\nHechos finales en la base de conocimiento:");
        foreach (var fact in workingKb.Facts.OrderBy(f => f.ToString()))
        {
            Console.WriteLine($"  - {fact}");
        }

        // Verificar si la solución es completa y consistente
        bool isComplete = IsSolutionComplete(workingKb);
        bool isConsistent = IsSolutionConsistent(workingKb);

        Console.WriteLine($"\n🔍 Verificación de la base de conocimiento:");
        Console.WriteLine($"  - Solución completa (sin variables): {isComplete}");
        Console.WriteLine($"  - Solución consistente (sin contradicciones): {isConsistent}");

        if (isComplete && isConsistent)
        {
            Console.WriteLine("\n🎉 ¡Solución encontrada directamente con forward chaining!");
            return new Solution(workingKb.Facts, inferenceSteps);
        }

        if (!isConsistent)
        {
            Console.WriteLine(
                "\n❌ Se ha detectado una contradicción en la base de conocimiento. No se puede continuar.");
            return null;
        }

        Console.WriteLine("\n🔄 Forward chaining insuficiente. Intentando backtracking...");

        // Si no hay solución directa, intentar backtracking con asunciones
        var solution = SolveWithBacktracking(workingKb, inferenceSteps);

        if (solution == null)
        {
            Console.WriteLine(
                "\n❌ No se encontró solución que satisfaga todas las restricciones tras el backtracking.");
        }

        return solution;
    }

    /// <summary>
    /// Resuelve usando backtracking cuando forward chaining no es suficiente
    /// </summary>
    private Solution? SolveWithBacktracking(KnowledgeBase kb, int initialSteps)
    {
        Console.WriteLine("🧩 Identificando variables libres...");

        var freeVariables = IdentifyFreeVariables(kb);

        Console.WriteLine($"  Variables libres encontradas: {freeVariables.Count}");
        foreach (var (variable, possibleValues) in freeVariables)
        {
            Console.WriteLine($"    {variable.Name}: {possibleValues.Count} valores posibles");
        }

        if (freeVariables.Count == 0)
        {
            Console.WriteLine("⚠️ No hay variables libres para asignar. La solución depende solo de la consistencia.");
            return IsSolutionConsistent(kb) ? new Solution(kb.Facts, initialSteps) : null;
        }

        if (freeVariables.Count > 3 || freeVariables.Any(fv => fv.Item2.Count > 5))
        {
            Console.WriteLine("⚠️ Espacio de búsqueda demasiado grande. Limitando variables...");
            freeVariables = freeVariables.Take(2).Select(fv => (fv.Item1, fv.Item2.Take(3).ToList())).ToList();
        }

        Console.WriteLine("🔍 Iniciando backtracking...");

        return BacktrackSolve(kb, freeVariables, 0, initialSteps);
    }

    private Solution? BacktrackSolve(KnowledgeBase kb, List<(Variable, List<Constant>)> assignments, int depth,
        int steps)
    {
        if (depth >= assignments.Count)
        {
            Console.WriteLine("    Evaluando combinación completa...");

            var testKb = kb.Clone();
            int localSteps = steps;

            // Re-run forward chaining con las nuevas asunciones
            while (true)
            {
                int factsBefore = testKb.Facts.Count;
                testKb.ForwardChain();
                int factsAfter = testKb.Facts.Count;

                if (factsAfter > factsBefore)
                {
                    localSteps++;
                }
                else
                {
                    break;
                }
            }

            bool isComplete = IsSolutionComplete(testKb);
            bool isConsistent = IsSolutionConsistent(testKb);

            Console.WriteLine($"    --> Resultado: Completa: {isComplete}, Consistente: {isConsistent}");

            if (isComplete && isConsistent)
            {
                Console.WriteLine("    ✓ Solución válida encontrada en esta rama.");
                return new Solution(testKb.Facts, localSteps);
            }

            return null;
        }

        var (variable, possibleValues) = assignments[depth];
        Console.WriteLine(
            $"  Profundidad {depth}: Probando variable '{variable.Name}' con {possibleValues.Count} valores...");

        foreach (var value in possibleValues)
        {
            Console.WriteLine($"    - Asignando {variable.Name} = {value.Value}");

            var testKb = kb.Clone();

            ApplyVariableAssignment(testKb, variable, value);

            var result = BacktrackSolve(testKb, assignments, depth + 1, steps);
            if (result != null)
                return result;
        }

        Console.WriteLine($"  Profundidad {depth}: No se encontró solución en esta rama.");
        return null;
    }

    private void ApplyVariableAssignment(KnowledgeBase kb, Variable variable, Constant value)
    {
        var substitution = new Dictionary<Variable, Term> { [variable] = value };

        var factsToUpdate = kb.Facts.Where(f => ContainsVariable(f, variable)).ToList();

        foreach (var fact in factsToUpdate)
        {
            kb.Facts.Remove(fact);
            var newFact = fact.ApplySubstitution(substitution);
            kb.Facts.Add(newFact);
        }
    }

    private bool ContainsVariable(Fact fact, Variable variable)
    {
        return fact.Arguments.Any(arg => ContainsVariable(arg, variable));
    }

    private bool ContainsVariable(Term term, Variable variable)
    {
        return term switch
        {
            Variable v => v.Name == variable.Name,
            CompoundTerm ct => ct.Arguments.Any(arg => ContainsVariable(arg, variable)),
            _ => false
        };
    }

    private List<(Variable, List<Constant>)> IdentifyFreeVariables(KnowledgeBase kb)
    {
        var freeVariables = new Dictionary<string, Variable>();
        var constantsByType = new Dictionary<string, HashSet<string>>();

        foreach (var fact in kb.Facts) ExtractVariablesAndConstants(fact, freeVariables, constantsByType);
        foreach (var rule in kb.Rules)
        {
            foreach (var antecedent in rule.Antecedents)
                ExtractVariablesAndConstants(antecedent, freeVariables, constantsByType);
            ExtractVariablesAndConstants(rule.Consequent, freeVariables, constantsByType);
        }

        var result = new List<(Variable, List<Constant>)>();
        foreach (var variable in freeVariables.Values)
        {
            var possibleValues = GeneratePossibleValues(variable, constantsByType);
            if (possibleValues.Any()) result.Add((variable, possibleValues));
        }

        return result;
    }

    private void ExtractVariablesAndConstants(Fact fact, Dictionary<string, Variable> variables,
        Dictionary<string, HashSet<string>> constants)
    {
        foreach (var arg in fact.Arguments) ExtractFromTerm(arg, variables, constants);
    }

    private void ExtractFromTerm(Term term, Dictionary<string, Variable> variables,
        Dictionary<string, HashSet<string>> constants)
    {
        switch (term)
        {
            case Variable v:
                if (!v.IsGround) variables[v.Name] = v;
                break;
            case Constant c:
                var type = InferConstantType(c.Value);
                if (!constants.ContainsKey(type)) constants[type] = new HashSet<string>();
                constants[type].Add(c.Value);
                break;
            case CompoundTerm ct:
                foreach (var arg in ct.Arguments) ExtractFromTerm(arg, variables, constants);
                break;
        }
    }

    private string InferConstantType(string value)
    {
        if (int.TryParse(value, out _)) return "number";
        if (value.Contains("casa")) return "house";
        if (new[] { "rojo", "azul", "verde", "amarillo", "blanco" }.Contains(value)) return "color";
        if (new[] { "ingles", "español", "noruego", "ucraniano", "japones" }.Contains(value)) return "nationality";
        return "general";
    }

    private List<Constant> GeneratePossibleValues(Variable variable,
        Dictionary<string, HashSet<string>> constantsByType)
    {
        // Simple strategy: Infer type from variable name (e.g., 'X' is person, 'C' is color)
        // This part can be significantly improved for a more general solver.
        var key = "general";
        if (variable.Name.StartsWith("C") && constantsByType.ContainsKey("color")) key = "color";
        if (variable.Name.StartsWith("N") && constantsByType.ContainsKey("nationality")) key = "nationality";

        if (constantsByType.TryGetValue(key, out var values))
        {
            return values.Select(v => new Constant(v)).ToList();
        }

        return constantsByType.Values.SelectMany(set => set).Select(v => new Constant(v)).Take(5).ToList();
    }

    private bool IsSolutionComplete(KnowledgeBase kb)
    {
        return kb.Facts.All(fact => fact.IsGround);
    }

    private bool IsSolutionConsistent(KnowledgeBase kb)
    {
        // Define "functional properties": attributes that should be unique for an entity or value.
        var uniqueProperties = new Dictionary<string, Fact>();

        foreach (var fact in kb.Facts.Where(f => f.IsGround))
        {
            string key;
            Fact conflictingFact;

            switch (fact.Predicate)
            {
                // A person can only have one nationality.
                case "es" when fact.Arity == 2:
                    key = $"nationality_of_person_{fact.Arguments[0]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[1].Equals(fact.Arguments[1]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: {fact.Arguments[0]} no puede ser '{conflictingFact.Arguments[1]}' y '{fact.Arguments[1]}' al mismo tiempo.");
                        return false;
                    }

                    uniqueProperties[key] = fact;
                    break;

                // A person lives in one house, and a house is occupied by one person.
                case "vive_en" when fact.Arity == 2:
                    // Check if person already lives somewhere else.
                    key = $"person_lives_in_{fact.Arguments[0]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[1].Equals(fact.Arguments[1]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: {fact.Arguments[0]} no puede vivir en '{conflictingFact.Arguments[1]}' y '{fact.Arguments[1]}'.");
                        return false;
                    }

                    uniqueProperties[key] = fact;

                    // Check if house is already occupied.
                    key = $"house_is_occupied_by_{fact.Arguments[1]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[0].Equals(fact.Arguments[0]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: La casa {fact.Arguments[1]} no puede estar ocupada por '{conflictingFact.Arguments[0]}' y '{fact.Arguments[0]}'.");
                        return false;
                    }

                    uniqueProperties[key] = fact;
                    break;

                // A house has one color, and a color is used for one house.
                case "casa" when fact.Arguments.Length == 3 && fact.Arguments[0].ToString() == "color":
                    // Check if house already has a color.
                    key = $"color_of_house_{fact.Arguments[1]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[2].Equals(fact.Arguments[2]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: La casa {fact.Arguments[1]} no puede ser '{conflictingFact.Arguments[2]}' y '{fact.Arguments[2]}'.");
                        return false;
                    }

                    uniqueProperties[key] = fact;

                    // Check if color is already used.
                    key = $"color_is_used_by_house_{fact.Arguments[2]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[1].Equals(fact.Arguments[1]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: El color {fact.Arguments[2]} no puede ser usado por la casa '{conflictingFact.Arguments[1]}' y '{fact.Arguments[1]}'.");
                        return false;
                    }

                    uniqueProperties[key] = fact;
                    break;

                // A house has one position, and a position is used for one house.
                case "casa" when fact.Arguments.Length == 3 && fact.Arguments[0].ToString() == "posicion":
                    // Check if house already has a position.
                    key = $"position_of_house_{fact.Arguments[1]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[2].Equals(fact.Arguments[2]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: La casa {fact.Arguments[1]} no puede tener la posición '{conflictingFact.Arguments[2]}' y '{fact.Arguments[2]}'.");
                        return false;
                    }

                    uniqueProperties[key] = fact;

                    // Check if position is already used.
                    key = $"position_is_used_by_house_{fact.Arguments[2]}";
                    if (uniqueProperties.TryGetValue(key, out conflictingFact) &&
                        !conflictingFact.Arguments[1].Equals(fact.Arguments[1]))
                    {
                        Console.WriteLine(
                            $"  !! Contradicción: La posición {fact.Arguments[2]} no puede ser para la casa '{conflictingFact.Arguments[1]}' y '{fact.Arguments[1]}'.");
                        return false;
                    }

                    uniqueProperties[key] = fact;
                    break;
            }
        }

        return true;
    }
}
