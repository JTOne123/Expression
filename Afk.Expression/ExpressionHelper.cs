﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afk.Expression
{
    /// <summary>
    /// Provides method to analyze an <see cref="IExpression"/>
    /// </summary>
    public class ExpressionHelper
    {
        /// <summary>
        /// Builds an <see cref="System.Linq.Expressions.Expression"/> from <see cref="IExpression"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <param name="expressionFunc"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static System.Linq.Expressions.Expression<Func<T, bool>> BuildLambda<T>(IExpression expr, 
            Func<System.Linq.Expressions.Expression, string, System.Linq.Expressions.Expression> expressionFunc,
            bool ignoreCase)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            object parse = (expr is ExpressionEval) ? ((ExpressionEval)expr).Parse() : expr;

            ParameterExpression argParam = System.Linq.Expressions.Expression.Parameter(typeof(T), "s");
            System.Linq.Expressions.Expression exp = BuildLambda(parse, argParam, expressionFunc);

            System.Linq.Expressions.Expression<Func<T, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(exp, argParam);

            return lambda;
        }

        private static System.Linq.Expressions.Expression BuildLambda(object o, 
            System.Linq.Expressions.Expression parameter, 
            Func<System.Linq.Expressions.Expression, string, System.Linq.Expressions.Expression> expressionFunc)
        {
            System.Linq.Expressions.Expression result = null;

            if (o is UserExpression)
            {
                result = expressionFunc(parameter, ((UserExpression)o).Expression);
            }
            else if (o is UnaryNode)
            {
                #region UnaryNode
                UnaryNode node = (UnaryNode)o;
                System.Linq.Expressions.Expression e1 = BuildLambda(node.Operand, parameter, expressionFunc);

                switch (node.Op.Op)
                {
                    case "+":
                        result = e1;
                        break;
                    case"-":
                        result = System.Linq.Expressions.Expression.Negate(e1);
                        break;
                    case"!":
                        result = System.Linq.Expressions.Expression.Not(e1);
                        break;
                    case"~":
                        result = System.Linq.Expressions.Expression.Not(e1);
                        break;
                }
                #endregion
            }
            else if (o is BinaryNode)
            {
                #region BinaryNode
                BinaryNode node = (BinaryNode)o;
                System.Linq.Expressions.Expression e1 = BuildLambda(node.Operand1, parameter, expressionFunc);
                System.Linq.Expressions.Expression e2 = BuildLambda(node.Operand2, parameter, expressionFunc);

                switch (node.Op.Op.ToLower())
                {
                    case "*":
                        result = System.Linq.Expressions.Expression.Multiply(e1, e2);
                        break;
                    case "/":
                        result = System.Linq.Expressions.Expression.Divide(e1, e2);
                        break;
                    case "%": 
                        result = System.Linq.Expressions.Expression.Modulo(e1, e2);
                        break;
                    case "+":
                        {
                            if (e1.Type == typeof(string) || e2.Type == typeof(string))
                            {
#if NETSTANDARD
                                var concatMethod = typeof(string).GetRuntimeMethod("Concat", new[] { typeof(object), typeof(object) });
                                result = System.Linq.Expressions.Expression.Add(e1, e2, concatMethod);
#else
                                var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) });
                                result = System.Linq.Expressions.Expression.Add(e1, e2, concatMethod);
#endif
                            }
                            else
                            {
                                result = System.Linq.Expressions.Expression.Add(e1, e2);
                            }
                        }
                        break;
                    case "-": 
                        result = System.Linq.Expressions.Expression.Subtract(e1, e2);
                        break;
                    case ">>":
                        result = System.Linq.Expressions.Expression.RightShift(e1, e2);
                        break;
                    case "<<": 
                        result = System.Linq.Expressions.Expression.LeftShift(e1, e2);
                        break;
                    case "<":
                        result = System.Linq.Expressions.Expression.LessThan(e1, e2);
                        break;
                    case "<=":
                        result = System.Linq.Expressions.Expression.LessThanOrEqual(e1, e2);
                        break;
                    case ">":
                        result = System.Linq.Expressions.Expression.GreaterThan(e1, e2);
                        break;
                    case ">=": 
                        result = System.Linq.Expressions.Expression.GreaterThanOrEqual(e1, e2);
                        break;
                    case "like":
                        if (e1.Type == typeof(string) && e2.Type == typeof(string))
                        {
#if NETSTANDARD
                            MethodInfo mi = typeof(string).GetRuntimeMethod("Contains", new Type[] { typeof(string) });
                            result = System.Linq.Expressions.Expression.Call(e1, mi, e2);
#else
                            MethodInfo mi = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
                            result = System.Linq.Expressions.Expression.Call(e1, mi, e2);
#endif
                        }
                        else
                        {
                            throw new InvalidOperationException("like operator not supported");
                        }
                        break;
                    case "==":
                    case "=":
                        result = System.Linq.Expressions.Expression.Equal(e1, e2);
                        break;
                    case "<>":
                    case "!=":
                        result = System.Linq.Expressions.Expression.NotEqual(e1, e2);
                        break;
                    case "&": 
                        result = System.Linq.Expressions.Expression.And(e1, e2);
                        break;
                    case "^":
                        throw new NotImplementedException();
                        //result = System.Linq.Expressions.Expression.Not(e1, e2);
                        //break;
                    case "|": 
                        result = System.Linq.Expressions.Expression.Or(e1, e2);
                        break;
                    case "and":
                    case "&&":
                        result = System.Linq.Expressions.Expression.AndAlso(e1, e2);
                        break;
                    case "or":
                    case "||":
                        result = System.Linq.Expressions.Expression.OrElse(e1, e2);
                        break;
                }
#endregion
            }
            else
            {
                result = System.Linq.Expressions.Expression.Constant(o);
            }
            return result;
        }
    }
    
}