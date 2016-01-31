using System;
using HandlebarsDotNet.Compiler;
using System.Linq.Expressions;

namespace HandlebarsDotNet.Compiler
{
    internal class PartialExpression : HandlebarsExpression
    {
        private readonly string _partialName;
        private readonly Expression _argument;
        private readonly string _indent;

        public PartialExpression(string partialName, string indent, Expression argument)
        {
            _partialName = partialName;
            _argument = argument;
            _indent = indent;
        }

        public override ExpressionType NodeType
        {
            get { return (ExpressionType)HandlebarsExpressionType.PartialExpression; }
        }

        public override Type Type
        {
            get { return typeof(void); }
        }

        public string PartialName
        {
            get { return _partialName; }
        }

        public Expression Argument
        {
            get { return _argument; }
        }

        public string Indent {
            get { return _indent; }
        }
    }
}

