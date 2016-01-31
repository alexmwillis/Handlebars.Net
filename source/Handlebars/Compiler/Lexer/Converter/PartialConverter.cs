using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HandlebarsDotNet.Compiler.Lexer;

namespace HandlebarsDotNet.Compiler
{
    internal class PartialConverter : TokenConverter
    {
        public static IEnumerable<object> Convert(IEnumerable<object> sequence)
        {
            return new PartialConverter().ConvertTokens(sequence).ToList();
        }

        private PartialConverter()
        {
        }

        public override IEnumerable<object> ConvertTokens(IEnumerable<object> sequence)
        {
            string indent = string.Empty;
            var enumerator = sequence.GetEnumerator();
            var first = true;
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                var staticExpression = item as StaticExpression;
                var partialToken = item as PartialToken;
                if (partialToken != null)
                {
                    var partialName = partialToken.Value.Substring(1).Trim('"');
                    var arguments = AccumulateArguments(enumerator);
                    if (arguments.Count == 0)
                    {
                        yield return HandlebarsExpression.Partial(partialName, indent);
                    }
                    else if (arguments.Count == 1)
                    {
                        yield return HandlebarsExpression.Partial(partialName, indent, arguments[0]);
                    }
                    else
                    {
                        throw new HandlebarsCompilerException(string.Format("Partial {0} can only accept 0 or 1 arguments", partialName));
                    }
                    yield return enumerator.Current;
                }
                else if (staticExpression != null)
                {
                    indent = GetIndent(staticExpression.Value, first);
                    yield return item;
                }
                else
                {
                    if (!(item is StartExpressionToken))
                    {
                        indent = string.Empty;
                    }
                    yield return item;
                }
                first = false;
            }
        }

        private static string GetIndent(string value, bool first)
        {
            var index = value.LastIndexOf("\n") + 1;
            return index > 0 && string.IsNullOrWhiteSpace(value.Substring(index))
                ? value.Substring(index)
                : first && string.IsNullOrWhiteSpace(value)
                    ? value
                    : string.Empty;
        }

        private static List<Expression> AccumulateArguments(IEnumerator<object> enumerator)
        {
            var item = GetNext(enumerator);
            List<Expression> helperArguments = new List<Expression>();
            while ((item is EndExpressionToken) == false)
            {
                if ((item is Expression) == false)
                {
                    throw new HandlebarsCompilerException(string.Format("Token '{0}' could not be converted to an expression", item));
                }
                helperArguments.Add((Expression)item);
                item = GetNext(enumerator);
            }
            return helperArguments;
        }

        private static object GetNext(IEnumerator<object> enumerator)
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }
    }
}

