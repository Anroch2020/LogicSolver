namespace LogicSolver.Core;

/// <summary>
/// Representa una solución completa al acertijo
/// </summary>
public record Solution(HashSet<Fact> Facts, int InferenceSteps);