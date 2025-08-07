using LogicSolver.Core;

namespace LogicSolver.Inference
{
    /// <summary>
    /// Motor de inferencia que resuelve un acertijo lógico.
    /// </summary>
    public class InferenceEngine
    {
        private readonly KnowledgeBase _initialKnowledgeBase;

        public InferenceEngine(KnowledgeBase knowledgeBase)
        {
            _initialKnowledgeBase = knowledgeBase;
        }

        /// <summary>
        /// Resuelve el acertijo aplicando repetidamente las reglas (forward-chaining)
        /// hasta que no se puedan deducir más hechos nuevos.
        /// </summary>
        public Solution? Solve()
        {
            // Clonamos la base de conocimiento para no modificar el estado original.
            var workingKb = _initialKnowledgeBase.Clone();
            int initialFactCount = workingKb.Facts.Count;

            // Este es el bucle principal de inferencia.
            // Continuará ejecutándose mientras el método ForwardChain() siga añadiendo nuevos hechos.
            while (workingKb.ForwardChain())
            {
                // El bucle se detiene cuando una pasada completa de ForwardChain() no añade nada nuevo.
            }

            int finalFactCount = workingKb.Facts.Count;
            int inferredCount = finalFactCount - initialFactCount;

            // Devolvemos la base de conocimiento final como la solución.
            return new Solution(workingKb.Facts, inferredCount);
        }
    }
}