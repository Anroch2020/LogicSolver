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

            // *** AGREGAR ESTAS LÍNEAS DE DEBUG AQUÍ ***
            Console.WriteLine("📊 INFORMACIÓN DE DEBUG:");
            Console.WriteLine("Hechos cargados:");
            foreach (var fact in knowledgeBase.Facts)
            {
                Console.WriteLine($"  - {fact}");
            }

            Console.WriteLine("\nReglas cargadas:");
            foreach (var rule in knowledgeBase.Rules)
            {
                Console.WriteLine($"  - {rule}");
            }
            Console.WriteLine();
            // *** FIN DE LAS LÍNEAS DE DEBUG ***

            // Ejecutar el motor de inferencia
            var inferenceEngine = new InferenceEngine(knowledgeBase);
            var solution = inferenceEngine.Solve();

            if (solution != null)
            {
                Console.WriteLine("🎉 SOLUCIÓN ENCONTRADA:");
                Console.WriteLine();
                DisplaySolution(solution);
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

    private static void DisplaySolution(Solution solution)
    {
        foreach (var fact in solution.Facts.OrderBy(f => f.ToString()))
        {
            Console.WriteLine($"  {fact}");
        }

        Console.WriteLine();
        Console.WriteLine($"Hechos totales en la solución: {solution.Facts.Count}");
        Console.WriteLine($"Pasos de inferencia: {solution.InferenceSteps}");
    }
}
