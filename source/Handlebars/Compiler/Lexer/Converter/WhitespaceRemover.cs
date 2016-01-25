using System;
using System.Collections.Generic;
using System.Linq;
using HandlebarsDotNet.Compiler.Lexer;
using System.Text.RegularExpressions;

namespace HandlebarsDotNet.Compiler
{
    internal class WhitespaceRemover : TokenConverter
    {
        private static readonly Regex StartsNewLineRegex = new Regex("^[^\\S\r\n]*\r?\n");
        private static readonly Regex EndsNewLineRegex = new Regex("\r?\n[^\\S\r\n]*$");
        private readonly HandlebarsConfiguration _configuration;

        public static IEnumerable<object> Remove(IEnumerable<object> sequence, HandlebarsConfiguration configuration)
        {
            return new WhitespaceRemover(configuration).ConvertTokens(sequence).ToList();
        }

        private WhitespaceRemover(HandlebarsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override IEnumerable<object> ConvertTokens(IEnumerable<object> sequence)
        {
            var list = sequence.ToList();
            FindStandaloneHelpers(list);

            for (int i = 0; i < list.Count; i++)
            {
                var prevEndToken = (i > 0 ? list[i - 1] : null) as EndExpressionToken;
                var nextStartToken = (i < list.Count - 1 ? list[i + 1] : null) as StartExpressionToken;
                var staticToken = list[i] as StaticToken;
                if (staticToken == null)
                {
                    yield return list[i];
                    continue;
                }
                if (prevEndToken != null)
                {
                    if (prevEndToken.TrimTrailingWhitespace)
                    {
                        staticToken = Token.Static(staticToken.Value.TrimStart());
                    }
                    if (prevEndToken.ExpressionIsStandalone)
                    {
                        staticToken = Token.Static(StartsNewLineRegex.Replace(staticToken.Value, ""));
                    }
                }
                if (nextStartToken != null)
                {
                    if (nextStartToken.TrimPreceedingWhitespace)
                    {
                        staticToken = Token.Static(staticToken.Value.TrimEnd());
                    }
                    if (nextStartToken.ExpressionIsStandalone)
                    {
                        staticToken =
                            Token.Static(EndsNewLineRegex.Replace(staticToken.Value, Environment.NewLine));
                    }
                }
                yield return staticToken;
            }
        }

        private void FindStandaloneHelpers(IEnumerable<object> sequence)
        {
            var staticTokenEndsOnNewLine = true;
            var enumerator = sequence.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                if (item is StartExpressionToken && staticTokenEndsOnNewLine)
                {
                    var startExpressionToken = (StartExpressionToken) item;
                    var isHelper = false;
                    while (enumerator.MoveNext() && !(enumerator.Current is EndExpressionToken))
                    {
                        var word = enumerator.Current as WordExpressionToken;
                        isHelper = isHelper || (word != null && IsRegisteredHelperName(word.Value));
                    }
                    var endExpressionToken = enumerator.Current as EndExpressionToken;
                    if (isHelper && endExpressionToken != null && enumerator.MoveNext())
                    {
                        var staticToken = enumerator.Current as StaticToken;
                        var isStandalone = (staticToken != null) &&
                                           StartsNewLineRegex.IsMatch(staticToken.Value);
                        startExpressionToken.ExpressionIsStandalone = isStandalone;
                        endExpressionToken.ExpressionIsStandalone = isStandalone;
                        staticTokenEndsOnNewLine = (staticToken != null) && EndsNewLineRegex.IsMatch(staticToken.Value);
                    }
                }
                else
                {
                    if (item is StaticToken)
                    {
                        staticTokenEndsOnNewLine = EndsNewLineRegex.IsMatch(((StaticToken) item).Value) ||
                                                   string.IsNullOrEmpty(((StaticToken) item).Value);
                    }
                    else
                    {
                        staticTokenEndsOnNewLine = false;
                    }
                }
            }
        }

        private bool IsRegisteredHelperName(string name)
        {
            name = name.Replace("#", "").Replace("/","");
            return _configuration.Helpers.ContainsKey(name)
            || _configuration.BlockHelpers.ContainsKey(name)
            || new []{ "else", "each" }.Contains(name);
        }
    }
}

