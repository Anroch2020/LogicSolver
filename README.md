# LogicSolver: Un Motor de Inferencia Lógica en C#
`LogicSolver` es un motor de inferencia lógica básico, desarrollado en C#, capaz de resolver acertijos y problemas de lógica simbólica. El motor utiliza técnicas de **forward chaining** y **backtracking** para encontrar soluciones que satisfagan un conjunto de hechos y reglas definidos en una base de conocimiento.
## Características
- **Sintaxis Declarativa**: Define hechos y reglas en un archivo de texto simple y legible.
- **Motor de Inferencia**:
    - **Forward Chaining**: Deduce automáticamente nuevos hechos a partir de los existentes.
    - **Backtracking**: Explora sistemáticamente el espacio de soluciones cuando la deducción directa no es suficiente.

- **Unificación Lógica**: Incluye un mecanismo de unificación para emparejar hechos con reglas que contienen variables.
- **Detección de Contradicciones**: Verifica la consistencia de la base de conocimiento para evitar estados lógicamente imposibles.
- **Extensible**: Diseñado con clases como `Fact`, `Rule` y `Term` para facilitar la adición de nueva funcionalidad.

## Cómo Funciona
El proceso para resolver un acertijo se divide en los siguientes pasos:
1. **Parseo**: El motor lee un archivo de texto (por ejemplo, ) que contiene los hechos iniciales y las reglas del problema. `puzzle.txt`
2. **Forward Chaining**: Se aplica un algoritmo de encadenamiento hacia adelante para inferir todos los hechos posibles. Este proceso se repite hasta que no se puedan deducir nuevos hechos.
3. **Verificación de la Solución**: Una vez que el conocimiento es estable, el motor comprueba si la base de conocimiento resultante:
    - Está **completa** (no contiene variables sin resolver).
    - Es **consistente** (no viola ninguna de las restricciones lógicas).

4. **Búsqueda con Backtracking (si es necesario)**: Si el forward chaining no es suficiente para encontrar una solución completa y consistente, el motor puede emplear un algoritmo de backtracking para explorar diferentes asignaciones de valores y encontrar una solución válida.

## Estructura del Proyecto
El proyecto está organizado en los siguientes componentes clave:
- `LogicSolver/`: El proyecto principal de la aplicación de consola.
    - : El punto de entrada que carga el archivo, inicia el motor y muestra la solución. `Program.cs`
    - : El núcleo del motor, que orquesta el forward chaining, la detección de contradicciones y el backtracking. `InferenceEngine.cs`
    - : Responsable de leer el archivo de texto y convertirlo en una `KnowledgeBase`. `PuzzleParser.cs`

- `LogicSolver.Core/`: Contiene las clases fundamentales del modelo lógico.
    - : Almacena el conjunto de hechos y reglas. `KnowledgeBase.cs`
    - : Representa un hecho lógico (ej: `vive_en(juan, casa_roja)`). `Fact.cs`
    - : Representa una regla de inferencia (ej: `A -> B`). `Rule.cs`
    - (y sus derivados `Constant`, `Variable`): Representan los componentes de un hecho. `Term.cs`

## Cómo Usar
1. **Define tu acertijo**: Crea un archivo de texto (p. ej., ) para describir tu problema. `puzzle.txt`
    - Usa `hecho:` para definir un hecho inicial.
    - Usa `regla:` para definir una regla de inferencia.

**Ejemplo: `puzzle.txt`**
``` 
    # Acertijo de las Casas - Versión Simplificada
    # Hay 3 casas de diferentes colores, cada una habitada por una persona de diferente nacionalidad

    # Hechos iniciales
    hecho: (casa, numero, 1)
    hecho: (casa, numero, 2)
    hecho: (casa, numero, 3)
    hecho: (color, disponible, rojo)
    hecho: (color, disponible, verde)
    hecho: (nacionalidad, disponible, ingles)
    hecho: (nacionalidad, disponible, español)
    hecho: (nacionalidad, disponible, noruego)

    # Reglas del acertijo
    regla: (vive_en, X, casa_roja) -> (es, X, ingles)
    regla: (es, X, español) -> (vive_en, X, casa_azul)
    regla: (vive_en, X, Y) AND (casa, posicion, Y, 1) -> (es, X, noruego)

    # Restricciones y asignaciones iniciales
    hecho: (vive_en, juan, casa_roja)
    hecho: (casa, color, casa_roja, rojo)
    hecho: (casa, color, casa_azul, azul)
    hecho: (casa, color, casa_verde, verde)
    hecho: (casa, posicion, casa_roja, 2)
    hecho: (casa, posicion, casa_azul, 3)
    hecho: (casa, posicion, casa_verde, 1)
```
1. **Ejecuta el programa**: Abre una terminal en el directorio del proyecto y ejecuta la aplicación.
``` bash
    # Compila y ejecuta el proyecto, usando el archivo puzzle.txt por defecto
    dotnet run
    
    # O especifica un archivo de acertijo diferente
    dotnet run -- mi_otro_acertijo.txt
```
1. **Revisa la solución**: El programa imprimirá los pasos de la inferencia y la solución final encontrada.
   **Salida de ejemplo:**
``` 
    === LogicSolver: Motor de Inferencia Lógica ===

    ✓ Archivo cargado: puzzle.txt
    ✓ Hechos iniciales: 16
    ✓ Reglas: 3

    🔍 Iniciando motor de inferencia...
    
    ⚡ Aplicando forward chaining para deducir hechos...
    ... (pasos de la inferencia) ...
    
    ✓ Forward chaining completado. Total de hechos generados: 17
    
    🔍 Verificación de la base de conocimiento:
      - Solución completa (sin variables): True
      - Solución consistente (sin contradicciones): True

    🎉 SOLUCIÓN ENCONTRADA:

      casa(color, casa_azul, azul)
      casa(color, casa_roja, rojo)
      ... (y el resto de hechos de la solución) ...
```
