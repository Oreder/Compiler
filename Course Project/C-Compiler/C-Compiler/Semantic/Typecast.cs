﻿using System;
using System.Diagnostics;

namespace AST
{
    public class TypeCast : Expr
    {
        public enum EnumTypeCast
        {
            NOP,
            INT8_TO_INT16,
            INT8_TO_INT32,

            INT16_TO_INT32,

            INT32_TO_FLOAT,
            INT32_TO_DOUBLE,

            PRESERVE_INT8,
            PRESERVE_INT16,

            UINT8_TO_UINT16,
            UINT8_TO_UINT32,

            UINT16_TO_UINT32,

            FLOAT_TO_INT32,
            FLOAT_TO_DOUBLE,

            DOUBLE_TO_INT32,
            DOUBLE_TO_FLOAT,
        }

        public readonly Expr expr;
        public readonly EnumTypeCast cast;

        public TypeCast(EnumTypeCast _cast, Expr _expr, ExprType _type)
            : base(_type)
        {
            expr = _expr;
            cast = _cast;
        }

        public static bool EqualType(ExprType t1, ExprType t2)
        {
            return t1.EqualType(t2);
        }

        // SignedIntegralToArith
        // =====================
        // input: expr, type
        // output: TypeCast
        // converts expr to type
        // 
        public static Expr SignedIntegralToArith(Expr expr, ExprType type)
        {
            ExprType.EnumExprType from = expr.type.expr_type;
            ExprType.EnumExprType to = type.expr_type;

            switch (from)
            {
                case ExprType.EnumExprType.CHAR:
                    switch (to)
                    {
                        case ExprType.EnumExprType.SHORT:
                        case ExprType.EnumExprType.USHORT:
                            return new TypeCast(EnumTypeCast.INT8_TO_INT16, expr, type);

                        case ExprType.EnumExprType.LONG:
                        case ExprType.EnumExprType.ULONG:
                            return new TypeCast(EnumTypeCast.INT8_TO_INT32, expr, type);

                        case ExprType.EnumExprType.UCHAR:
                            return new TypeCast(EnumTypeCast.NOP, expr, type);

                        case ExprType.EnumExprType.FLOAT:
                            // char -> long -> float
                            return new TypeCast(EnumTypeCast.INT32_TO_FLOAT, new TypeCast(EnumTypeCast.INT8_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.EnumExprType.DOUBLE:
                            // char -> long -> double
                            return new TypeCast(EnumTypeCast.INT32_TO_DOUBLE, new TypeCast(EnumTypeCast.INT8_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.EnumExprType.SHORT:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                        case ExprType.EnumExprType.UCHAR:
                            return new TypeCast(EnumTypeCast.PRESERVE_INT8, expr, type);

                        case ExprType.EnumExprType.USHORT:
                            return new TypeCast(EnumTypeCast.NOP, expr, type);

                        case ExprType.EnumExprType.LONG:
                        case ExprType.EnumExprType.ULONG:
                            return new TypeCast(EnumTypeCast.INT16_TO_INT32, expr, type);

                        case ExprType.EnumExprType.FLOAT:
                            // short -> long -> float
                            return new TypeCast(EnumTypeCast.INT32_TO_FLOAT, new TypeCast(EnumTypeCast.INT16_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.EnumExprType.DOUBLE:
                            // short -> long -> double
                            return new TypeCast(EnumTypeCast.INT32_TO_DOUBLE, new TypeCast(EnumTypeCast.INT16_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.EnumExprType.LONG:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(SByte)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT8, expr, type);
                            }

                        case ExprType.EnumExprType.UCHAR:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)(Byte)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT8, expr, type);
                            }

                        case ExprType.EnumExprType.SHORT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(Int16)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, expr, type);
                            }

                        case ExprType.EnumExprType.USHORT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)(UInt16)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, expr, type);
                            }

                        case ExprType.EnumExprType.ULONG:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.NOP, expr, type);
                            }

                        case ExprType.EnumExprType.FLOAT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstFloat((Single)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.INT32_TO_FLOAT, expr, type);
                            }

                        case ExprType.EnumExprType.DOUBLE:
                            if (expr.IsConstExpr())
                            {
                                return new ConstDouble((Double)((ConstLong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.INT32_TO_DOUBLE, expr, type);
                            }

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                default:
                    Debug.Assert(false);
                    return null;
            }
        }

        // UnsignedIntegralToArith
        // =======================
        // input: expr, type
        // output: TypeCast
        // converts expr to type
        // 
        // Note: according to MSDN "Conversions from Unsigned Integral Types",
        //       unsigned long converts directly to double.
        //       however, I just treat unsigned long as long.
        // 
        public static Expr UnsignedIntegralToArith(Expr expr, ExprType type)
        {
            ExprType.EnumExprType from = expr.type.expr_type;
            ExprType.EnumExprType to = type.expr_type;

            switch (from)
            {
                case ExprType.EnumExprType.UCHAR:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                            return new TypeCast(EnumTypeCast.NOP, expr, type);

                        case ExprType.EnumExprType.SHORT:
                        case ExprType.EnumExprType.USHORT:
                            return new TypeCast(EnumTypeCast.UINT8_TO_UINT16, expr, type);

                        case ExprType.EnumExprType.LONG:
                        case ExprType.EnumExprType.ULONG:
                            return new TypeCast(EnumTypeCast.UINT8_TO_UINT32, expr, type);

                        case ExprType.EnumExprType.FLOAT:
                            // uchar -> ulong -> long -> float
                            return new TypeCast(EnumTypeCast.INT32_TO_FLOAT, new TypeCast(EnumTypeCast.UINT8_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.EnumExprType.DOUBLE:
                            // uchar -> ulong -> long -> double
                            return new TypeCast(EnumTypeCast.INT32_TO_DOUBLE, new TypeCast(EnumTypeCast.UINT8_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.EnumExprType.USHORT:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                        case ExprType.EnumExprType.UCHAR:
                            return new TypeCast(EnumTypeCast.PRESERVE_INT8, expr, type);

                        case ExprType.EnumExprType.USHORT:
                            return new TypeCast(EnumTypeCast.NOP, expr, type);

                        case ExprType.EnumExprType.LONG:
                        case ExprType.EnumExprType.ULONG:
                            return new TypeCast(EnumTypeCast.UINT16_TO_UINT32, expr, type);

                        case ExprType.EnumExprType.FLOAT:
                            // ushort -> ulong -> long -> float
                            return new TypeCast(EnumTypeCast.INT32_TO_FLOAT, new TypeCast(EnumTypeCast.UINT16_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.EnumExprType.DOUBLE:
                            // ushort -> ulong -> long -> double
                            return new TypeCast(EnumTypeCast.INT32_TO_DOUBLE, new TypeCast(EnumTypeCast.UINT16_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.EnumExprType.ULONG:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(SByte)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT8, expr, type);
                            }

                        case ExprType.EnumExprType.UCHAR:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)(Byte)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT8, expr, type);
                            }

                        case ExprType.EnumExprType.SHORT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(Int16)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, expr, type);
                            }

                        case ExprType.EnumExprType.USHORT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)(UInt16)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, expr, type);
                            }

                        case ExprType.EnumExprType.LONG:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.NOP, expr, type);
                            }

                        case ExprType.EnumExprType.FLOAT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstFloat((Single)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.INT32_TO_FLOAT, expr, type);
                            }

                        case ExprType.EnumExprType.DOUBLE:
                            if (expr.IsConstExpr())
                            {
                                return new ConstDouble((Double)((ConstULong)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.INT32_TO_DOUBLE, expr, type);
                            }

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                default:
                    Debug.Assert(false);
                    return null;
            }
        }

        // FloatToArith
        // ============
        // input: expr, type
        // output: TypeCast
        // converts expr to type
        // 
        // Note: according to MSDN "Conversions from Floating-Point Types",
        //       float cannot convert to unsigned char.
        //       I don't know why, but I follow it.
        // 
        public static Expr FloatToArith(Expr expr, ExprType type)
        {
            ExprType.EnumExprType from = expr.type.expr_type;
            ExprType.EnumExprType to = type.expr_type;

            switch (from)
            {
                case ExprType.EnumExprType.FLOAT:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(SByte)((ConstFloat)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT8, new TypeCast(EnumTypeCast.FLOAT_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);
                            }

                        case ExprType.EnumExprType.SHORT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(Int16)((ConstFloat)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, new TypeCast(EnumTypeCast.FLOAT_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);
                            }

                        case ExprType.EnumExprType.USHORT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)(UInt16)((ConstFloat)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, new TypeCast(EnumTypeCast.FLOAT_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);
                            }

                        case ExprType.EnumExprType.LONG:
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)((ConstFloat)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.FLOAT_TO_INT32, expr, type);
                            }

                        case ExprType.EnumExprType.ULONG:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)((ConstFloat)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.FLOAT_TO_INT32, expr, type);
                            }

                        case ExprType.EnumExprType.DOUBLE:
                            if (expr.IsConstExpr())
                            {
                                return new ConstDouble((Double)((ConstFloat)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.FLOAT_TO_DOUBLE, expr, type);
                            }

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.EnumExprType.DOUBLE:
                    switch (to)
                    {
                        case ExprType.EnumExprType.CHAR:
                            // double -> float -> char
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(SByte)((ConstDouble)expr).value);
                            }
                            else
                            {
                                return FloatToArith(FloatToArith(expr, new TFloat(type.is_const, type.is_volatile)), new TChar(type.is_const, type.is_volatile));
                            }

                        case ExprType.EnumExprType.SHORT:
                            // double -> float -> short
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)(Int16)((ConstDouble)expr).value);
                            }
                            else
                            {
                                return FloatToArith(FloatToArith(expr, new TFloat(type.is_const, type.is_volatile)), new TShort(type.is_const, type.is_volatile));
                            }

                        case ExprType.EnumExprType.LONG:
                            // double -> float -> short
                            if (expr.IsConstExpr())
                            {
                                return new ConstLong((Int32)((ConstDouble)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.DOUBLE_TO_INT32, expr, type);
                            }

                        case ExprType.EnumExprType.ULONG:
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)((ConstDouble)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.DOUBLE_TO_INT32, expr, type);
                            }

                        case ExprType.EnumExprType.USHORT:
                            // double -> long -> ushort
                            if (expr.IsConstExpr())
                            {
                                return new ConstULong((UInt32)(UInt16)((ConstDouble)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.PRESERVE_INT16, new TypeCast(EnumTypeCast.DOUBLE_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);
                            }

                        case ExprType.EnumExprType.FLOAT:
                            if (expr.IsConstExpr())
                            {
                                return new ConstFloat((Single)((ConstDouble)expr).value);
                            }
                            else
                            {
                                return new TypeCast(EnumTypeCast.DOUBLE_TO_FLOAT, expr, type);
                            }

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                default:
                    Debug.Assert(false);
                    return null;
            }
        }

        // FromPointer
        // ===========
        // input: expr, type
        // output: TypeCast
        // converts expr to type
        // 
        // Note: a pointer behaves like a ulong.
        //       it can be converted to 1) another pointer 2) an integral
        //       if else, assert.
        // 
        public static Expr FromPointer(Expr expr, ExprType type)
        {
            ExprType.EnumExprType from = expr.type.expr_type;
            ExprType.EnumExprType to = type.expr_type;

            if (from != ExprType.EnumExprType.POINTER)
            {
                throw new Exception("Error: expected a pointer.");
            }

            // if we are casting to another pointer, do a nop
            if (to == ExprType.EnumExprType.POINTER)
            {
                if (expr.IsConstExpr())
                {
                    return new ConstPtr(((ConstPtr)expr).value, type);
                }
                else
                {
                    return new TypeCast(EnumTypeCast.NOP, expr, type);
                }
            }

            // if we are casting to an integral
            if (type.IsIntegral())
            {
                // pointer -> ulong -> whatever integral
                if (expr.IsConstExpr())
                {
                    expr = new ConstULong(((ConstPtr)expr).value);
                }
                else
                {
                    expr = new TypeCast(EnumTypeCast.NOP, expr, new TULong(type.is_const, type.is_volatile));
                }
                return UnsignedIntegralToArith(expr, type);
            }

            throw new Exception("Error: Casting from a pointer to an unsupported type.");
        }

        // ToPointer
        // =========
        // input: expr, type
        // output: TypeCast
        // converts expr to type
        // 
        // Note: a pointer behaves like a ulong.
        //       it can be converted from 1) another pointer 2) an integral
        //       if else, assert.
        // 
        public static Expr ToPointer(Expr expr, ExprType type)
        {
            ExprType.EnumExprType from = expr.type.expr_type;
            ExprType.EnumExprType to = type.expr_type;

            if (to != ExprType.EnumExprType.POINTER)
            {
                throw new Exception("Error: expected casting to pointer.");
            }

            if (from == ExprType.EnumExprType.POINTER)
            {
                if (expr.IsConstExpr())
                {
                    return new ConstPtr(((ConstPtr)expr).value, type);
                }
                else
                {
                    return new TypeCast(EnumTypeCast.NOP, expr, type);
                }
            }

            if (expr.type.IsIntegral())
            {
                // if we are casting from an integral

                // whatever integral -> ulong
                switch (expr.type.expr_type)
                {
                    case ExprType.EnumExprType.CHAR:
                    case ExprType.EnumExprType.SHORT:
                    case ExprType.EnumExprType.LONG:
                        expr = SignedIntegralToArith(expr, new TULong(type.is_const, type.is_volatile));
                        break;
                    case ExprType.EnumExprType.UCHAR:
                    case ExprType.EnumExprType.USHORT:
                    case ExprType.EnumExprType.ULONG:
                        expr = expr = UnsignedIntegralToArith(expr, new TULong(type.is_const, type.is_volatile));
                        break;
                    default:
                        break;
                }

                // ulong -> pointer
                if (expr.IsConstExpr())
                {
                    return new ConstPtr(((ConstULong)expr).value, type);
                }
                else
                {
                    return new TypeCast(EnumTypeCast.NOP, expr, type);
                }

            }

            throw new Exception("Error: casting from an unsupported type to pointer.");
        }

        // MakeCast
        // ========
        // input: expr, type
        // output: TypeCast
        // converts expr to type
        // 
        public static Expr MakeCast(Expr expr, ExprType type)
        {

            // if two types are equal, return expr
            if (EqualType(expr.type, type))
            {
                return expr;
            }

            // from pointer
            if (expr.type.expr_type == ExprType.EnumExprType.POINTER)
            {
                return FromPointer(expr, type);
            }

            // to pointer
            if (type.expr_type == ExprType.EnumExprType.POINTER)
            {
                return ToPointer(expr, type);
            }

            switch (expr.type.expr_type)
            {
                // from signed integral
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.LONG:
                    return SignedIntegralToArith(expr, type);

                // from unsigned integral
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.ULONG:
                    return UnsignedIntegralToArith(expr, type);

                // from float
                case ExprType.EnumExprType.FLOAT:
                case ExprType.EnumExprType.DOUBLE:
                    return FloatToArith(expr, type);

                default:
                    throw new Exception("Error: expression type not supported for casting from.");
            }

        }

        // UsualArithmeticConversion
        // =========================
        // input: e1, e2
        // output: tuple<e1', e2', enumexprtype>
        // performs the usual arithmetic conversion on e1 & e2
        // 
        // possible return type: double, float, ulong, long
        // 
        public static Tuple<Expr, Expr, ExprType.EnumExprType> UsualArithmeticConversion(Expr e1, Expr e2)
        {
            ExprType t1 = e1.type;
            ExprType t2 = e2.type;

            bool c1 = t1.is_const;
            bool v1 = t1.is_volatile;
            bool c2 = t2.is_const;
            bool v2 = t2.is_volatile;
            // 1. if either expr is double: both are converted to double
            if (t1.expr_type == ExprType.EnumExprType.DOUBLE || t2.expr_type == ExprType.EnumExprType.DOUBLE)
            {
                return new Tuple<Expr, Expr, ExprType.EnumExprType>(MakeCast(e1, new TDouble(c1, v1)), MakeCast(e2, new TDouble(c2, v2)), ExprType.EnumExprType.DOUBLE);
            }

            // 2. if either expr is float: both are converted to float
            if (t1.expr_type == ExprType.EnumExprType.FLOAT || t2.expr_type == ExprType.EnumExprType.FLOAT)
            {
                return new Tuple<Expr, Expr, ExprType.EnumExprType>(MakeCast(e1, new TFloat(c1, v1)), MakeCast(e2, new TFloat(c2, v2)), ExprType.EnumExprType.FLOAT);
            }

            // 3. if either expr is unsigned long: both are converted to unsigned long
            if (t1.expr_type == ExprType.EnumExprType.ULONG || t2.expr_type == ExprType.EnumExprType.ULONG)
            {
                return new Tuple<Expr, Expr, ExprType.EnumExprType>(MakeCast(e1, new TULong(c1, v1)), MakeCast(e2, new TULong(c2, v2)), ExprType.EnumExprType.ULONG);
            }

            // 4. both are converted to long
            return new Tuple<Expr, Expr, ExprType.EnumExprType>(MakeCast(e1, new TLong(c1, v1)), MakeCast(e2, new TLong(c2, v2)), ExprType.EnumExprType.LONG);

        }

        // UsualScalarConversion
        // =====================
        // input: e1, e2
        // output: tuple<e1', e2', enumexprtype>
        // first, convert pointers to ulongs, then do usual arithmetic conversion
        // 
        // possible return type: double, float, ulong, long
        // 
        public static Tuple<Expr, Expr, ExprType.EnumExprType> UsualScalarConversion(Expr e1, Expr e2)
        {
            if (e1.type.expr_type == ExprType.EnumExprType.POINTER)
            {
                e1 = FromPointer(e1, new TULong(e1.type.is_const, e1.type.is_volatile));
            }
            if (e2.type.expr_type == ExprType.EnumExprType.POINTER)
            {
                e2 = FromPointer(e2, new TULong(e2.type.is_const, e2.type.is_volatile));
            }
            return UsualArithmeticConversion(e1, e2);
        }

    }
}