﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afk.Expression
{
    /// <summary>
    /// Represents an array of expressions whch can contains value or <see cref="IExpression"/>
    /// </summary>
    class ArrayExpression : IExpression
    {
        /// <summary>
        /// Represents the method that handles the user expression event
        /// </summary>
        public event UserExpressionEventHandler UserExpressionEventHandler;

        /// <summary>
        /// Represents the method that handles the user function event
        /// </summary>
        public event UserFunctionEventHandler UserFunctionEventHandler;

        private bool parsed;
        private CaseSensitivity caseSensitivity;
        private object[] parameters;

        private ExpressionArguments arguments;

        /// <summary>
        /// Initialize a new instance of <see cref="ArrayExpression"/>
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="arguments"></param>
        /// <param name="caseSensitivity"></param>
        public ArrayExpression(string expression, ExpressionArguments arguments, CaseSensitivity caseSensitivity)
        {
            this.Expression = expression;
            this.arguments = arguments;
            this.caseSensitivity = caseSensitivity;
        }

        /// <summary>
        /// Gets the expression of array
        /// </summary>
        public string Expression
        {
            get;
            private set;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <returns></returns>
        public object Evaluate()
        {
            return this.Evaluate(null);
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <returns></returns>
        public object Evaluate(int? correlationId)
        {
            if (!this.parsed)
            {
                this.parameters = GetParameters(this.Expression, correlationId);
                this.parsed = true;
            }
            return this.parameters;
        }

        /// <summary>
        /// Reduce the expression
        /// </summary>
        /// <returns></returns>
        public object Reduce()
        {
            return this;
        }

        /// <summary>
        /// Return the values of array
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        private object[] GetParameters(string expression, int? correlationId)
        {
            string expr = "";
            int nIdx = 0;
            int nBrackets = 0;
            int nParenthesis = 0;
            int nLast = 0;
            bool bInQuotes = false;
            List<object> ret = new List<object>();

            if (expression == null || expression == string.Empty)
                return new object[] { };

            expr = expression;

            try
            {
                for (nIdx = 0; nIdx < expr.Length; nIdx++)
                {
                    if (!bInQuotes && expr[nIdx] == ']')
                        nBrackets--;
                    if (!bInQuotes && expr[nIdx] == '[')
                        nBrackets++;
                    if (!bInQuotes && expr[nIdx] == ')')
                        nParenthesis--;
                    if (!bInQuotes && expr[nIdx] == '(')
                        nParenthesis++;

                    if (expr[nIdx] == DefinedRegex.QuoteCharacter)
                        bInQuotes = !bInQuotes;

                    if (!bInQuotes && nBrackets == 0 && nParenthesis == 0 && expr[nIdx] == ',')
                    {
                        ret.Add(EvaluateParameter(expr.Substring(nLast, nIdx - nLast), correlationId));
                        nLast = nIdx + 1;
                    }
                }
                ret.Add(EvaluateParameter(expr.Substring(nLast, nIdx - nLast), correlationId));
            }
            catch (ExpressionException e)
            {
                throw new ExpressionException(e.Message, nLast, e.Length);
            }

            return ret.ToArray();
        }

        /// <summary>
        /// Evalue un paramètre
        /// </summary>
        /// <param name="expr">Expression à évaluer</param>
        /// <param name="correlationId"></param>
        /// <returns>Objet évalué</returns>
        private object EvaluateParameter(string expr, int? correlationId)
        {
            ExpressionParser eval = new ExpressionParser(expr, this.arguments, this.caseSensitivity);

            if (this.UserExpressionEventHandler != null)
            {
                eval.UserExpressionEventHandler += this.UserExpressionEventHandler;
            }
            if (this.UserFunctionEventHandler != null)
            {
                eval.UserFunctionEventHandler += this.UserFunctionEventHandler;
            }

            return eval.Evaluate(correlationId);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public string ToPolishString()
        {
            return "[ArrayExpr]";
        }
    }
}