using LogicSolver.Core;
using LogicSolver.Parser;
using LogicSolver.Inference;

namespace LogicSolver;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== LogicSolver: Motor de Inferencia Lógica ===");
        Console.WriteLine();

        try
        {
            string puzzleFile = args.Length > 0 ? args[0] : "puzzle.txt";

            if (!File.Exists(puzzleFile))
            {
                Console.WriteLine($"Archivo no encontrado: {puzzleFile}");
                Console.WriteLine("Uso: LogicSolver [archivo_acertijo.txt]");
                return;
            }

            // Cargar y parsear el archivo del acertijo
            string content = await File.ReadAllTextAsync(puzzleFile);
            var parser = new PuzzleParser();
            var knowledgeBase = parser.Parse(content);

            Console.WriteLine($"✓ Archivo cargado: {puzzleFile}");
            Console.WriteLine($"✓ Hechos iniciales: {knowledgeBase.Facts.Count}");
            Console.WriteLine($"✓ Reglas: {knowledgeBase.Rules.Count}");
            Console.WriteLine();

            Console.WriteLine("📊 INFORMACIÓN DE DEBUG:");
            Console.WriteLine("Hechos cargados:");
            foreach (var fact in knowledgeBase.Facts.OrderBy(f => f.ToString()))
            {
                Console.WriteLine($"  - {fact}");
            }

            Console.WriteLine("\nReglas cargadas:");
            foreach (var rule in knowledgeBase.Rules)
            {
                Console.WriteLine($"  - {rule}");
            }
            Console.WriteLine();

            // Guardamos una copia de los hechos iniciales para comparar después
            var initialFacts = new HashSet<Fact>(knowledgeBase.Facts);

            // Ejecutar el motor de inferencia
            var inferenceEngine = new InferenceEngine(knowledgeBase);
            var solution = inferenceEngine.Solve();

            if (solution != null)
            {
                Console.WriteLine("🎉 SOLUCIÓN ENCONTRADA:");
                Console.WriteLine();
                // Pasamos los hechos iniciales para una mejor visualización
                DisplaySolution(solution, initialFacts);
            }
            else
            {
                Console.WriteLine("❌ No se encontró solución que satisfaga todas las restricciones.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Muestra la solución, distinguiendo entre hechos inferidos y hechos totales.
    /// </summary>
    private static void DisplaySolution(Solution solution, HashSet<Fact> initialFacts)
    {
        // Calculamos los hechos que son nuevos
        var inferredFacts = solution.Facts.Except(initialFacts).ToList();

        Console.WriteLine("--- Hechos Inferidos ---");
        if (inferredFacts.Any())
        {
            foreach (var fact in inferredFacts.OrderBy(f => f.ToString()))
            {
                Console.WriteLine($"  ✓ {fact}");
            }
        }
        else
        {
            Console.WriteLine("  (Ninguno)");
        }

        Console.WriteLine();
        Console.WriteLine("--- Conjunto Completo de Hechos Finales ---");
        foreach (var fact in solution.Facts.OrderBy(f => f.ToString()))
        {
            Console.WriteLine($"  - {fact}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total de nuevos hechos deducidos: {solution.InferenceSteps}");
    }
}