﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
    public class EnvironmentTest
    {
        [TestMethod]
        public void TestEnv()
        {
            AST.Env env = new AST.Env();

            env = env.PushEntry(AST.Env.EntryLoc.GLOBAL, "global_var", new AST.TLong());

            env = env.InScope();
            List<Tuple<String, AST.ExprType>> args = new List<Tuple<string, AST.ExprType>>();
            args.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TFunction func = new AST.TFunction(new AST.TVoid(), args);
            AST.Env env2 = env.SetCurrentFunction(func);

            String log = env.Dump();
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestShadow()
        {
            AST.Env env = new AST.Env();
            env = env.PushEntry(AST.Env.EntryLoc.GLOBAL, "c", new AST.TChar());
            env = env.InScope();
            env = env.PushEntry(AST.Env.EntryLoc.STACK, "c", new AST.TLong());

            String log = env.Dump();
            System.Diagnostics.Debug.WriteLine(log);

            AST.Env.Entry entry = env.Find("c");

            System.Diagnostics.Debug.WriteLine("c : " + entry.entry_loc + " " + entry.entry_type);
        }
    }

    [TestClass]
    public class TypeTest
    {
        [TestMethod]
        public void TestDump()
        {
            AST.ExprType type = new AST.TDouble(true, true);
            type = type.GetQualifiedType(false, false);
        }

        [TestMethod]
        public void TestFunction()
        {
            List<Tuple<String, AST.ExprType>> args = new List<Tuple<string, AST.ExprType>>();
            args.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            args.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            args.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TFunction func = new AST.TFunction(new AST.TVoid(), args);
            String log = func.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestStruct()
        {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
            attribs.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TStruct struct_ = new AST.TStruct(attribs);
            String log = struct_.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestUnion()
        {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<string, AST.ExprType>>();
            attribs.Add(new Tuple<string, AST.ExprType>("some_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_char", new AST.TChar()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("another_double", new AST.TDouble()));
            attribs.Add(new Tuple<string, AST.ExprType>("some_int", new AST.TLong()));
            AST.TUnion union_ = new AST.TUnion(attribs);
            String log = union_.Dump(true);
            System.Diagnostics.Debug.WriteLine(log);
        }

        [TestMethod]
        public void TestDeclnSpecs()
        {
            String src = "int long unsigned";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.ExprType> t = decln_specs.GetExprType(env);
        }

        [TestMethod]
        public void TestDeclnSpecsStruct()
        {
            String src = "struct MyStruct { int a; int b; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.ExprType> t = decln_specs.GetExprType(env);
        }

        [TestMethod]
        public void TestDeclnSpecsUnion()
        {
            String src = "union MyUnion { int a; int b; }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.ExprType> t = decln_specs.GetExprType(env);
        }

        [TestMethod]
        public void TestEnum()
        {
            String src = "enum MyEnum { VAL1, VAL2, VAL3 }";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.ExprType> t = decln_specs.GetExprType(env);
            env = t.Item1;
            String log = t.Item1.Dump();
            System.Diagnostics.Debug.WriteLine(log);

            src = "enum MyEnum";
            tokens = Parser.GetTokensFromString(src);
            r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            t = decln_specs.GetExprType(env);

        }

        [TestMethod]
        // This test should not pass!
        public void TestEnum2()
        {
            String src = "enum MyEnum";
            List<Token> tokens = Parser.GetTokensFromString(src);
            DeclnSpecs decln_specs;
            int r = _declaration_specifiers.Parse(tokens, 0, out decln_specs);
            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.ExprType> t = decln_specs.GetExprType(env);
        }
    }

    [TestClass]
    public class DeclnTest
    {
        [TestMethod]
        public void TestInt()
        {
            string src = "int a, *b, c(int haha, int), d[];";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Decln decln;
            int r = _declaration.Parse(tokens, 0, out decln);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
            //AST.ExprType type = new AST.TDouble(true, true);
            //type = type.GetQualifiedType(false, false);
        }

        [TestMethod]
        public void TestStruct()
        {
            string src = "struct MyStruct { int a; int b; } my_struct;";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Decln decln;
            int r = _declaration.Parse(tokens, 0, out decln);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
        }

        [TestMethod]
        public void TestUnion()
        {
            string src = "union MyUnion { int a; int b; } my_union;";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Decln decln;
            int r = _declaration.Parse(tokens, 0, out decln);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_decln = decln.GetDeclns(env);
        }
    }

    [TestClass]
    public class ExprTest
    {
        [TestMethod]
        public void TestMult()
        {
            string src = "3.0 * 5.0f;";
            List<Token> tokens = Parser.GetTokensFromString(src);
            Expression expr;
            int r = _multiplicative_expression.Parse(tokens, 0, out expr);

            AST.Env env = new AST.Env();
            Tuple<AST.Env, AST.Expr> r_expr = expr.GetExpr(env);

        }
    }

[TestClass]
public class StmtTest
{
    [TestMethod]
    public void TestCompountStmt()
    {
        string src = "{ int a; int b; 3.0f; a % a; }";
        List<Token> tokens = Parser.GetTokensFromString(src);
        CompoundStatement stmt;
        int r = _compound_statement.Parse(tokens, 0, out stmt);

        AST.Env env = new AST.Env();
        Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);

    }

    [TestMethod]
    public void TestVariable()
    {
        string src = "{int *a; a; }";
        List<Token> tokens = Parser.GetTokensFromString(src);
        CompoundStatement stmt;
        int r = _compound_statement.Parse(tokens, 0, out stmt);
        AST.Env env = new AST.Env();
        Tuple<AST.Env, AST.Stmt> r_stmt = stmt.GetStmt(env);
    }
}

[TestClass]
public class FullTest
{
    [TestMethod]
    public void TestFunctionDef()
    {
        string src = "int main(int argc, char **argv) { 0; 1; 3.0f; }";
        List<Token> tokens = Parser.GetTokensFromString(src);
        TranslationUnit unit;
        int r = _translation_unit.Parse(tokens, 0, out unit);

        Tuple<AST.Env, AST.TranslationUnit> r_unit = unit.GetTranslationUnit();
    }
}