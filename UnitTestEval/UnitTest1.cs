﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Afk.Expression;

namespace UnitTestEval
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestArithmetic()
        {
            ExpressionEval eval = new ExpressionEval("5 * x + x");
            eval.AddVariable("x"); 
            eval.UserExpressionEventHandler += OnUserExpression;
            var result = eval.Evaluate();
            Assert.AreEqual(result, 48d);
        }

        [TestMethod]
        public void TestConstants()
        {
            ExpressionEval eval = new ExpressionEval("5 * x + y - z");
            eval.AddConstant("x", 8);
            eval.AddConstant("y", 2);
            eval.AddConstant("z", 3);
            var result = eval.Evaluate();
            Assert.AreEqual(result, 39d);
        }

        [TestMethod]
        public void TestConstantsNotSensitive()
        {
            ExpressionEval eval = new ExpressionEval("5 * x + Y - Z", CaseSensitivity.None);
            eval.AddConstant("x", 8);
            eval.AddConstant("y", 2);
            eval.AddConstant("z", 3);
            var result = eval.Evaluate();
            Assert.AreEqual(result, 39d);
        }

        [TestMethod]
        public void TestConstantsSensitive()
        {
            ExpressionEval eval = new ExpressionEval("5 * xx + xX - Xx", CaseSensitivity.UserConstants);
            eval.AddConstant("xx", 8);
            eval.AddConstant("xX", 2);
            eval.AddConstant("Xx", 3);
            
            var result = eval.Evaluate();
            Assert.AreEqual(result, 39d);
        }

        [TestMethod]
        public void TestUserExpression()
        {
            ExpressionEval eval = new ExpressionEval("var1 + var2");
            eval.AddVariable("var1");
            eval.AddVariable("var2");
            eval.UserExpressionEventHandler += OnUserExpression;
            var result = eval.Evaluate();
            Assert.AreEqual(result, 9d);
        }

        [TestMethod]
        public void TestBracket()
        {
            ExpressionEval eval = new ExpressionEval("[ var1, 15, 'un '] + [var2, (25+9), 'deux']");
            eval.AddVariable("var1");
            eval.AddVariable("var2");
            eval.UserExpressionEventHandler += OnUserExpression;
            var result = eval.Evaluate();
            Assert.IsInstanceOfType(result, typeof(object[]));
            object[] values = result as object[];
            Assert.AreEqual(values[0], 9d);
            Assert.AreEqual(values[1], 49d);
            Assert.AreEqual(values[2], "un deux");
        }

        [TestMethod]
        public void TestFncConcat()
        {
            ExpressionEval eval = new ExpressionEval("'quand la caravane ' + Concat(  Concat(  Upper (verbe), ' les '), 'chiens ', 'aboient') + '.'");
            eval.AddVariable("var1");
            eval.AddVariable("var2");
            eval.AddVariable("verbe");

            eval.AddFunctions("Concat");
            eval.AddFunctions("Upper");

            eval.UserExpressionEventHandler += OnUserExpression;
            eval.UserFunctionEventHandler += OnFunctionHandler;
            var result = eval.Evaluate();

            Assert.AreEqual(result, "quand la caravane PASSE les chiens aboient.");
        }

        private void OnFunctionHandler(object sender, UserFunctionEventArgs e)
        {
            if (e.Name == "Concat")
            {
                e.Result = string.Join("", e.Parameters);
            }
            else if (e.Name == "Upper")
            {
                e.Result = e.Parameters[0].ToString().ToUpper();
            }
        }

        private void OnUserExpression(object sender, UserExpressionEventArgs e)
        {
            e.Result= 0;
            if (e.Name == "x") e.Result = 8d;
            if (e.Name == "var1") e.Result= 4;
            if (e.Name == "var2") e.Result= 5;
            if (e.Name == "verbe") e.Result = "passe";
        }
    }
}