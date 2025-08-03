using LogicSolver.Core;
using System.Text.RegularExpressions;

namespace LogicSolver.Parser;

/// <summary>
/// Parser para la sintaxis personalizada del acertijo
/// Sintaxis:
/// - Hecho: hecho: (predicado, arg1, arg2, ...)
/// - Regla: regla: (antecedente1, arg1, ...) AND (antecedente2, arg1, ...) -> (consecuente, arg1, ...)
/// - Variables empiezan con mayúscula
/// - Constantes empiezan con minúscula o son números
/// </summary>
public class PuzzleParser
{
    private static readonly Regex FactRegex = new(@"^\s*hecho:\s*(.+)\s*$", RegexOptions.IgnoreCase);
    private static readonly Regex RuleRegex = new(@"^\s*regla:\s*(.+?)\s*->\s*(.+)\s*$", RegexOptions.IgnoreCase);
    private static readonly Regex TermRegex = new(@"\(\s*([^,\(\)]+)(?:\s*,\s*([^,\(\)]+))*\s*\)");

    public KnowledgeBase Parse(string content)
    {
        var knowledgeBase = new KnowledgeBase();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;

            if (FactRegex.IsMatch(trimmedLine))
            {
                var fact = ParseFact(trimmedLine);
                if (fact != null)
                    knowledgeBase.AddFact(fact);
            }
            else if (RuleRegex.IsMatch(trimmedLine))
            {
                var rule = ParseRule(trimmedLine);
                if (rule != null)
                    knowledgeBase.AddRule(rule);
            }
        }

        return knowledgeBase;
    }

    private Fact? ParseFact(string line)
    {
        var match = FactRegex.Match(line);
        if (!match.Success) return null;

        var factText = match.Groups[1].Value.Trim();
        return ParseFactFromText(factText);
    }

    private Rule? ParseRule(string line)
    {
        var match = RuleRegex.Match(line);
        if (!match.Success) return null;

        var antecedentText = match.Groups[1].Value.Trim();
        var consequentText = match.Groups[2].Value.Trim();

        var antecedents = new List<Fact>();

        // Dividir antecedentes por AND
        var antecedentParts = antecedentText.Split(" AND ", StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in antecedentParts)
        {
            var antecedent = ParseFactFromText(part.Trim());
            if (antecedent != null)
                antecedents.Add(antecedent);
        }

        var consequent = ParseFactFromText(consequentText);

        if (antecedents.Count > 0 && consequent != null)
            return new Rule(antecedents, consequent);

        return null;
    }

    private Fact? ParseFactFromText(string text)
    {
        text = text.Trim();

        if (!text.StartsWith('(') || !text.EndsWith(')'))
            return null;

        // Remover paréntesis externos
        text = text.Substring(1, text.Length - 2);

        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(p => p.Trim())
                       .ToArray();

        if (parts.Length == 0) return null;

        var predicate = parts[0];
        var arguments = parts.Skip(1).Select(ParseTerm).Where(t => t != null).ToArray()!;

        return new Fact(predicate, arguments);
    }

    private Term? ParseTerm(string text)
    {
        text = text.Trim();

        if (string.IsNullOrEmpty(text))
            return null;

        // Variable (empieza con mayúscula)
        if (char.IsUpper(text[0]) && text.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return new Variable(text);
        }

        // Constante (empieza con minúscula o es número)
        if (char.IsLower(text[0]) || char.IsDigit(text[0]) || text.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return new Constant(text);
        }

        return new Constant(text); // Por defecto, tratar como constante
    }
}