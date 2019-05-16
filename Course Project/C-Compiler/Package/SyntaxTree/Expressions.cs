﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree
{
    // 3.2.1.5
    /* First, if either operand has type long double, the other operand is converted to long double.
     * Otherwise, if either operand has type double, the other operand is converted to double.
     * Otherwise, if either operand has type float, the other operand is converted to float.
     * Otherwise, the integral promotions are performed on both operands.
     * Then the following rules are applied:
     * If either operand has type unsigned long Int32, the other operand is converted to unsigned long Int32.
     * Otherwise, if one operand has type long Int32 and the other has type unsigned Int32, if a long Int32 can represent all values of an unsigned Int32, the operand of type unsigned Int32 is converted to long Int32;
     * if a long Int32 cannot represent all the values of an unsigned Int32, both operands are converted to unsigned long Int32. Otherwise, if either operand has type long Int32, the other operand is converted to long Int32.
     * Otherwise, if either operand has type unsigned Int32, the other operand is converted to unsigned Int32.
     * Otherwise, both operands have type Int32.*/

    // My simplification:
    // I let long = int, long double = double

    public abstract class Expr : PTNode
    {
        public abstract AST.Expr GetExpr(AST.Env env);
    }

    /// <summary>
    /// An empty expression
    /// used in [], and empty initialization
    /// </summary>
    public class EmptyExpr : Expr
    {
        public EmptyExpr() { }
        public override AST.Expr GetExpr(AST.Env env)
        {
            return new AST.EmptyExpr();
        }
    }

    /// <summary>
    /// Only a name
    /// </summary>
    public class Variable : Expr
    {
        public Variable(String name)
        {
            this.name = name;
        }
        public readonly String name;

        public override AST.Expr GetExpr(AST.Env env)
        {
            AST.Env.Entry entry = env.Find(name);

            switch (entry.kind)
            {
                case AST.Env.EntryKind.NOT_FOUND:
                    throw new InvalidOperationException($"Cannot find variable '{name}'");
                case AST.Env.EntryKind.TYPEDEF:
                    throw new InvalidOperationException($"Expected a variable '{name}', not a typedef.");
                case AST.Env.EntryKind.ENUM:
                    return new AST.ConstLong(entry.offset);
                case AST.Env.EntryKind.FRAME:
                case AST.Env.EntryKind.GLOBAL:
                case AST.Env.EntryKind.STACK:
                    return new AST.Variable(entry.type, name);
                default:
                    throw new InvalidOperationException($"Cannot find variable '{name}'");
            }
        }
    }

    /// <summary>
    /// A list of assignment expressions.
    /// e.g.
    ///   a = 3, b = 4;
    /// </summary>
	public class AssignmentList : Expr
    {
        public AssignmentList(List<Expr> _exprs)
        {
            assign_exprs = _exprs;
        }
        public List<Expr> assign_exprs;

        public override AST.Expr GetExpr(AST.Env env)
        {
            List<AST.Expr> exprs = assign_exprs.ConvertAll(expr => expr.GetExpr(env));
            return new AST.AssignmentList(exprs, exprs.FindLast(_ => true).type);
        }
    }

    /// <summary>
    /// Conditional Expression
    /// 
    /// cond ? true_expr : false_expr
    /// 
    /// cond must be of scalar type
    /// 
    /// 1. if both true_expr and false_expr have arithmetic types
    ///    perform usual arithmetic conversion
    /// 2. 
    /// </summary>
    // TODO : What if const???
    public class ConditionalExpression : Expr
    {
        public ConditionalExpression(Expr _cond, Expr _true_expr, Expr _false_expr)
        {
            cond_cond = _cond;
            cond_true_expr = _true_expr;
            cond_false_expr = _false_expr;
        }
        public readonly Expr cond_cond;
        public readonly Expr cond_true_expr;
        public readonly Expr cond_false_expr;

        public override AST.Expr GetExpr(AST.Env env)
        {
            AST.Expr cond = cond_cond.GetExpr(env);

            if (!cond.type.IsScalar())
            {
                throw new InvalidOperationException("Expected a scalar condition in conditional expression.");
            }

            AST.Expr true_expr = cond_true_expr.GetExpr(env);
            AST.Expr false_expr = cond_false_expr.GetExpr(env);

            // 1. if both true_expr and false_Expr have arithmetic types:
            //    perform usual arithmetic conversion
            if (true_expr.type.IsArith() && false_expr.type.IsArith())
            {
                var r_cast = AST.TypeCast.UsualArithmeticConversion(true_expr, false_expr);
                true_expr = r_cast.Item1;
                false_expr = r_cast.Item2;
                return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);
            }

            if (true_expr.type.kind != false_expr.type.kind)
            {
                throw new InvalidOperationException("Operand types not match in conditional expression.");
            }

            switch (true_expr.type.kind)
            {
                // 2. if both true_expr and false_expr have struct or union type
                //    make sure they are compatible
                case AST.ExprType.Kind.STRUCT_OR_UNION:
                    if (!true_expr.type.EqualType(false_expr.type))
                    {
                        throw new InvalidOperationException("Expected compatible types in conditional expression.");
                    }
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);

                // 3. if both true_expr and false_expr have void type
                //    return void
                case AST.ExprType.Kind.VOID:
                    return new AST.ConditionalExpr(cond, true_expr, false_expr, true_expr.type);

                // 4. if both true_expr and false_expr have pointer type
                case AST.ExprType.Kind.POINTER:

                    // if either points to void, convert to void *
                    if (((AST.TPointer)true_expr.type).ref_t.kind == AST.ExprType.Kind.VOID
                        || ((AST.TPointer)false_expr.type).ref_t.kind == AST.ExprType.Kind.VOID)
                    {
                        return new AST.ConditionalExpr(cond, true_expr, false_expr, new AST.TPointer(new AST.TVoid()));
                    }

                    throw new NotImplementedException("More comparisons here.");

                default:
                    throw new InvalidOperationException("Expected compatible types in conditional expression.");
            }
        }
    }

    public class FunctionCall : Expr
    {
        public FunctionCall(Expr func, IEnumerable<Expr> args)
        {
            this.func = func;
            this.args = args;
        }
        public readonly Expr func;
        public readonly IEnumerable<Expr> args;

        public override AST.Expr GetExpr(AST.Env env)
        {
            AST.Expr func = this.func.GetExpr(env);

            if (func.type.kind != AST.ExprType.Kind.FUNCTION)
            {
                throw new InvalidOperationException("Expected a function in function call.");
            }

            AST.TFunction func_type = (AST.TFunction)(func.type);

            var args = this.args.Select(_ => _.GetExpr(env)).ToList();

            if (args.Count() != func_type.args.Count)
            {
                throw new InvalidOperationException("Number of arguments mismatch.");
            }

            // make implicit cast
            args = Enumerable.Zip(args, func_type.args, (arg, entry) => AST.TypeCast.MakeCast(arg, entry.type)).ToList();

            return new AST.FunctionCall(func, func_type, args, func_type.ret_type);
        }
    }

    /// <summary>
    /// expr.attrib: get an attribute from a struct or union
    /// </summary>
    public class Attribute : Expr
    {
        public Attribute(Expr expr, Variable attrib)
        {
            this.expr = expr;
            this.attrib = attrib;
        }
        public readonly Expr expr;
        public readonly Variable attrib;

        public override AST.Expr GetExpr(AST.Env env)
        {
            AST.Expr expr = this.expr.GetExpr(env);
            String name = this.attrib.name;

            if (expr.type.kind != AST.ExprType.Kind.STRUCT_OR_UNION)
            {
                throw new InvalidOperationException("Must get the attribute from a struct or union.");
            }

            AST.Utils.StoreEntry entry = ((AST.TStructOrUnion)expr.type).Attribs.First(_ => _.name == name);
            AST.ExprType type = entry.type;

            return new AST.Attribute(expr, name, type);
        }
    }
}