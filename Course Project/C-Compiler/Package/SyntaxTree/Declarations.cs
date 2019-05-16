﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxTree
{
    // the declaration of an object
    public class Decln : ExternalDeclaration
    {
        public Decln(DeclnSpecs decln_specs, IEnumerable<InitDeclr> init_declrs)
        {
            this.decln_specs = decln_specs;
            this.init_declrs = init_declrs;
        }

        public readonly DeclnSpecs decln_specs;
        public readonly IEnumerable<InitDeclr> init_declrs;

        public Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> GetDeclns(AST.Env env)
        {

            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = decln_specs.GetSCSType(env);
            env = r_specs.Item1;
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType base_type = r_specs.Item3;

            List<Tuple<AST.Env, AST.Decln>> declns = new List<Tuple<AST.Env, AST.Decln>>();

            foreach (InitDeclr init_declr in init_declrs)
            {
                Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String> r_declr = init_declr.GetInitDeclr(env, base_type);

                env = r_declr.Item1;
                AST.ExprType type = r_declr.Item2;
                Option<AST.Initr> initr = r_declr.Item3;
                String name = r_declr.Item4;

                // TODO : [finished] add the newly declared object into the environment
                AST.Env.EntryKind loc;
                switch (scs)
                {
                    case AST.Decln.SCS.AUTO:
                        if (env.IsGlobal())
                        {
                            loc = AST.Env.EntryKind.GLOBAL;
                        }
                        else
                        {
                            loc = AST.Env.EntryKind.STACK;
                        }
                        break;
                    case AST.Decln.SCS.EXTERN:
                        loc = AST.Env.EntryKind.GLOBAL;
                        break;
                    case AST.Decln.SCS.STATIC:
                        loc = AST.Env.EntryKind.GLOBAL;
                        break;
                    case AST.Decln.SCS.TYPEDEF:
                        loc = AST.Env.EntryKind.TYPEDEF;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                env = env.PushEntry(loc, name, type);

                declns.Add(Tuple.Create(env, new AST.Decln(name, scs, type, initr)));

            }

            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>>(env, declns);
        }

        public override Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>> GetExternDecln(AST.Env env)
        {
            Tuple<AST.Env, List<Tuple<AST.Env, AST.Decln>>> r_declns = GetDeclns(env);
            env = r_declns.Item1;
            List<Tuple<AST.Env, AST.ExternDecln>> declns = new List<Tuple<AST.Env, AST.ExternDecln>>();
            foreach (Tuple<AST.Env, AST.Decln> decln in r_declns.Item2)
            {
                declns.Add(new Tuple<AST.Env, AST.ExternDecln>(decln.Item1, decln.Item2));
            }
            return new Tuple<AST.Env, List<Tuple<AST.Env, AST.ExternDecln>>>(env, declns);
        }

    }


    /// <summary>
    /// Declaration Specifiers
    /// 
    /// storage class specifiers
    /// type specifiers
    /// type qualifiers
    /// </summary>
    public class DeclnSpecs : PTNode
    {
        public DeclnSpecs(
            List<StorageClassSpec> _scs,
            List<TypeSpec> _typespecs,
            List<TypeQual> _typequals
        )
        {
            scss = _scs;
            type_quals = _typequals;
            type_specs = _typespecs;
        }

        public readonly List<StorageClassSpec> scss;
        public readonly List<TypeSpec> type_specs;
        public readonly List<TypeQual> type_quals;

        /// <summary>
        /// Get storage class specifier and type.
        /// </summary>
        public Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> GetSCSType(AST.Env env)
        {
            Tuple<AST.Env, AST.ExprType> r_type = GetExprTypeEnv(env);
            env = r_type.Item1;
            AST.ExprType type = r_type.Item2;
            AST.Decln.SCS scs = GetSCS();
            return Tuple.Create(env, scs, type);
        }

        /// <summary>
        /// Get the type and the modified environment.
        /// </summary>
        public Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env)
        {

            Boolean is_const = type_quals.Contains(TypeQual.CONST);
            Boolean is_volatile = type_quals.Contains(TypeQual.VOLATILE);

            // 1. if no type specifier => long
            if (type_specs.Count == 0)
            {
                return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));
            }

            // 2. now let's analyse type specs
            if (type_specs.All(spec => spec.kind != TypeSpec.Kind.NON_BASIC))
            {

                var basic_specs = type_specs.Select(spec => spec.kind);

                switch (GetBasicType(basic_specs))
                {
                    case AST.ExprType.Kind.CHAR:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TChar(is_const, is_volatile));

                    case AST.ExprType.Kind.UCHAR:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUChar(is_const, is_volatile));

                    case AST.ExprType.Kind.SHORT:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TShort(is_const, is_volatile));

                    case AST.ExprType.Kind.USHORT:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TUShort(is_const, is_volatile));

                    case AST.ExprType.Kind.LONG:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));

                    case AST.ExprType.Kind.ULONG:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TULong(is_const, is_volatile));

                    case AST.ExprType.Kind.FLOAT:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TFloat(is_const, is_volatile));

                    case AST.ExprType.Kind.DOUBLE:
                        return new Tuple<AST.Env, AST.ExprType>(env, new AST.TDouble(is_const, is_volatile));

                    default:
                        throw new Exception("Can't match type specifier.");
                }

            }
            else if (type_specs.Count == 1)
            {
                // now we can only match for struct, union, function...
                return type_specs[0].GetExprTypeEnv(env, is_const, is_volatile);

            }
            else
            {
                throw new InvalidOperationException("Can't match type specifier.");
            }
        }

        /// <summary>
        /// Only used by the parser.
        /// </summary>
        /// <returns></returns>
        public bool IsTypedef() => scss.Contains(StorageClassSpec.TYPEDEF);

        private AST.Decln.SCS GetSCS()
        {
            if (scss.Count == 0)
            {
                return AST.Decln.SCS.AUTO;
            }
            if (scss.Count == 1)
            {
                switch (scss[0])
                {
                    case StorageClassSpec.AUTO:
                    case StorageClassSpec.NULL:
                    case StorageClassSpec.REGISTER:
                        return AST.Decln.SCS.AUTO;
                    case StorageClassSpec.EXTERN:
                        return AST.Decln.SCS.EXTERN;
                    case StorageClassSpec.STATIC:
                        return AST.Decln.SCS.STATIC;
                    case StorageClassSpec.TYPEDEF:
                        return AST.Decln.SCS.TYPEDEF;
                    default:
                        throw new InvalidOperationException();
                }
            }
            throw new InvalidOperationException("Multiple storage class specifiers.");
        }

        /// <summary>
        /// Match basic type specifier: int / char / ...
        /// </summary>
        private static AST.ExprType.Kind GetBasicType(IEnumerable<TypeSpec.Kind> specs)
        {
            foreach (KeyValuePair<TypeSpec.Kind[], AST.ExprType.Kind> pair in type_spec_dict)
            {
                if (!Enumerable.Except(pair.Key, specs).Any())
                {
                    return pair.Value;
                }
            }
            throw new InvalidOperationException("Cannot match basic type specifier.");
        }

        private static IReadOnlyDictionary<TypeSpec.Kind[], AST.ExprType.Kind> type_spec_dict = new Dictionary<TypeSpec.Kind[], AST.ExprType.Kind>
        {

            // void
            [new[] { TypeSpec.Kind.VOID }] = AST.ExprType.Kind.VOID,

            // char
            [new[] { TypeSpec.Kind.CHAR }] = AST.ExprType.Kind.CHAR,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.CHAR }] = AST.ExprType.Kind.CHAR,

            // uchar
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.CHAR }] = AST.ExprType.Kind.UCHAR,

            // short
            [new[] { TypeSpec.Kind.SHORT }] = AST.ExprType.Kind.SHORT,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.SHORT }] = AST.ExprType.Kind.SHORT,
            [new[] { TypeSpec.Kind.SHORT, TypeSpec.Kind.INT }] = AST.ExprType.Kind.SHORT,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.SHORT, TypeSpec.Kind.INT }] = AST.ExprType.Kind.SHORT,

            // ushort
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.SHORT }] = AST.ExprType.Kind.USHORT,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.SHORT, TypeSpec.Kind.INT }] = AST.ExprType.Kind.USHORT,

            // long
            [new[] { TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.LONG }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.LONG, TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,
            [new[] { TypeSpec.Kind.SIGNED, TypeSpec.Kind.LONG, TypeSpec.Kind.INT }] = AST.ExprType.Kind.LONG,

            // ulong
            [new[] { TypeSpec.Kind.UNSIGNED }] = AST.ExprType.Kind.ULONG,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.INT }] = AST.ExprType.Kind.ULONG,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.LONG }] = AST.ExprType.Kind.ULONG,
            [new[] { TypeSpec.Kind.UNSIGNED, TypeSpec.Kind.LONG, TypeSpec.Kind.INT }] = AST.ExprType.Kind.ULONG,

            // float
            [new[] { TypeSpec.Kind.FLOAT }] = AST.ExprType.Kind.FLOAT,

            // double
            [new[] { TypeSpec.Kind.DOUBLE }] = AST.ExprType.Kind.DOUBLE,
            [new[] { TypeSpec.Kind.LONG, TypeSpec.Kind.DOUBLE }] = AST.ExprType.Kind.DOUBLE,

        };

    }


    // InitDeclr
    // =========
    // initialization declarator: a normal declarator + an initialization expression
    // 
    public class InitDeclr : PTNode
    {

        public InitDeclr(Declr declr, Option<Initr> initr)
        {
            this.declr = declr;
            this.initr = initr;
        }

        public readonly Declr declr; // TODO: should this be optional?
        public readonly Option<Initr> initr;

        // TODO : InitDeclr.GetInitDeclr(env, type) -> (env, type, expr) : change the type corresponding to init expression
        public Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String> GetInitDeclr(AST.Env env, AST.ExprType type)
        {
            String name;
            Option<AST.Initr> initr;

            Option<Tuple<AST.Env, AST.Initr>> r_initr = this.initr.Map(_ => _.GetInitr(env));
            if (r_initr.IsSome)
            {
                env = r_initr.value.Item1;
                initr = new Some<AST.Initr>(r_initr.value.Item2);
            }
            else
            {
                initr = new None<AST.Initr>();
            }

            Tuple<String, AST.ExprType> r_declr = declr.GetNameAndType(env, type);
            name = r_declr.Item1;
            type = r_declr.Item2;

            initr = initr.Map(_ => _.ConformType(type));

            return new Tuple<AST.Env, AST.ExprType, Option<AST.Initr>, String>(env, type, initr, name);
        }

    }


    public enum StorageClassSpec
    {
        NULL,
        AUTO,
        REGISTER,
        STATIC,
        EXTERN,
        TYPEDEF
    }

    // TypeSpec
    // ========
    // TypeSpec
    //    |
    //    +--- TypedefName
    //    |
    //    +--- EnumSpec
    //    |
    //    +--- StructOrUnionSpec
    //                 |
    //                 +--- StructSpec
    //                 |
    //                 +--- UnionSpec
    //
    public class TypeSpec : PTNode
    {
        public enum Kind
        {
            NON_BASIC,
            VOID,
            CHAR,
            SHORT,
            INT,
            LONG,
            FLOAT,
            DOUBLE,
            SIGNED,
            UNSIGNED
        }

        public TypeSpec()
        {
            kind = Kind.NON_BASIC;
        }
        public TypeSpec(Kind spec)
        {
            kind = spec;
        }

        // GetExprType
        // ===========
        // input: env
        // output: tuple<ExprType, Environment>
        // 
        public virtual Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile)
        {
            throw new NotImplementedException();
        }

        public readonly Kind kind;
    }


    /// <summary>
    /// Typedef Name
	/// 
	/// Represents a name that has been previously defined as a typedef.
    /// </summary>
    public class TypedefName : TypeSpec
    {
        public TypedefName(String _name)
        {
            name = _name;
        }

        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile)
        {

            AST.Env.Entry entry = env.Find(name);

            if (entry.kind == AST.Env.EntryKind.NOT_FOUND)
            {
                throw new InvalidOperationException($"Cannot find name \"{name}\".");
            }

            if (entry.kind != AST.Env.EntryKind.TYPEDEF)
            {
                throw new InvalidOperationException($"\"{name}\" is not a typedef.");
            }

            return Tuple.Create(env, entry.type.GetQualifiedType(is_const, is_volatile));
        }


        public readonly String name;
    }


    public enum TypeQual
    {
        NULL,
        CONST,
        VOLATILE
    }

    // Type Modifier
    // =============
    // Modify a type into a function, array, or pointer
    // 
    public abstract class TypeModifier : PTNode
    {
        public enum Kind
        {
            FUNCTION,
            ARRAY,
            POINTER
        }

        public TypeModifier(Kind kind)
        {
            this.kind = kind;
        }
        public readonly Kind kind;

        public abstract AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType type);

    }

    public class FunctionModifier : TypeModifier
    {
        public FunctionModifier(List<ParamDecln> param_declns, Boolean has_varargs)
            : base(Kind.FUNCTION)
        {
            this.param_declns = param_declns;
            this.has_varargs = has_varargs;
        }

        public FunctionModifier(ParameterTypeList _param_type_list)
            : base(Kind.FUNCTION)
        {
            param_type_list = _param_type_list;
        }
        public ParameterTypeList param_type_list;

        public readonly List<ParamDecln> param_declns;
        public readonly Boolean has_varargs;

        public override AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType ret_t)
        {
            var args = param_declns.ConvertAll(decln => decln.GetParamDecln(env));
            return AST.TFunction.Create(ret_t, args, has_varargs);
        }

    }

    public class ArrayModifier : TypeModifier
    {
        public ArrayModifier(Expr num_elems)
            : base(Kind.ARRAY)
        {
            this.num_elems = num_elems;
        }

        public override AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType type)
        {
            AST.Expr num_elems = AST.TypeCast.MakeCast(this.num_elems.GetExpr(env), new AST.TLong(true, true));

            if (!num_elems.IsConstExpr())
            {
                throw new InvalidOperationException("Expected constant length.");
            }

            return new AST.TArray(type, ((AST.ConstLong)num_elems).value);
        }


        public readonly Expr num_elems;
    }

    public class PointerModifier : TypeModifier
    {
        public PointerModifier(List<TypeQual> type_quals)
            : base(Kind.POINTER)
        {
            this.type_quals = type_quals;
        }

        public override AST.ExprType GetDecoratedType(AST.Env env, AST.ExprType type)
        {
            Boolean is_const = type_quals.Contains(TypeQual.CONST);
            Boolean is_volatile = type_quals.Contains(TypeQual.VOLATILE);
            return new AST.TPointer(type, is_const, is_volatile);
        }

        public readonly List<TypeQual> type_quals;
    }

    public class Declr : PTNode
    {
        public Declr(String name, List<TypeModifier> modifiers)
        {
            inner_declr_modifiers = modifiers;
            this.name = name;
        }

        public Declr()
            : this("", new List<TypeModifier>()) { }

        public IReadOnlyList<TypeModifier> declr_modifiers
        {
            get { return inner_declr_modifiers; }
        }
        private readonly List<TypeModifier> inner_declr_modifiers;
        public readonly String name;

        /// <summary>
        /// A declarator consists of 1) a name, and 2) a list of decorators.
        /// This method returns the name, and the modified type.
        /// </summary>
        public virtual Tuple<String, AST.ExprType> GetNameAndType(AST.Env env, AST.ExprType base_type) =>
            Tuple.Create(
                name,
                declr_modifiers
                    .Reverse()
                    .Aggregate(base_type, (type, modifier) => modifier.GetDecoratedType(env, type))
            );

    }


    // Parameter Type List
    // ===================
    // 
    public class ParameterTypeList : PTNode
    {
        public ParameterTypeList(List<ParamDecln> _param_list, Boolean _varargs)
        {
            params_varargs = _varargs;
            params_inner_declns = _param_list;
        }

        public ParameterTypeList(List<ParamDecln> _param_list)
            : this(_param_list, false) { }

        public readonly Boolean params_varargs;
        public IReadOnlyList<ParamDecln> params_declns
        {
            get { return params_inner_declns; }
        }
        public readonly List<ParamDecln> params_inner_declns;

        // Get Parameter Types
        // ===================
        // 
        public Tuple<Boolean, List<Tuple<AST.Env, String, AST.ExprType>>> GetParamTypesEnv(AST.Env env)
        {
            return Tuple.Create(
                params_varargs,
                params_inner_declns.ConvertAll(decln => {
                    Tuple<String, AST.ExprType> r_decln = decln.GetParamDecln(env);
                    // Tuple<AST.Env, String, AST.ExprType> r_decln = decln.GetParamDeclnEnv(env);
                    // env = r_decln.Item1;
                    return Tuple.Create(env, r_decln.Item1, r_decln.Item2);
                })
            );
        }

    }


    /// <summary>
    /// Enum Specifier
    /// 
    /// enum enum-name {
    ///     ENUM-0,
    ///     ENUM-1,
    /// 	...
    /// }
    /// </summary>
    public class EnumSpecifier : TypeSpec
    {
        public EnumSpecifier(String _name, List<Enumerator> _enum_list)
        {
            spec_name = _name;
            spec_enums = _enum_list;
        }

        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile)
        {
            if (spec_enums == null)
            {
                // if there is no content in this enum type, we must find it's definition in the environment
                AST.Env.Entry entry = env.Find("enum " + spec_name);
                if (entry == null || entry.kind != AST.Env.EntryKind.TYPEDEF)
                {
                    Log.SemantError("Error: type 'enum " + spec_name + " ' has not been defined.");
                    return null;
                }
            }
            else
            {
                // so there are something in this enum type, we need to put this type into the environment
                Int32 idx = 0;
                foreach (Enumerator elem in spec_enums)
                {
                    Tuple<AST.Env, String, Int32> r_enum = elem.GetEnumerator(env, idx);
                    env = r_enum.Item1;
                    String name = r_enum.Item2;
                    idx = r_enum.Item3;
                    env = env.PushEnum(name, new AST.TLong(), idx);
                    idx++;
                }
                env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, "enum " + spec_name, new AST.TLong());
            }

            return new Tuple<AST.Env, AST.ExprType>(env, new AST.TLong(is_const, is_volatile));
        }

        public readonly String spec_name;
        public readonly List<Enumerator> spec_enums;

    }


    public class Enumerator : PTNode
    {
        public Enumerator(String _name, Expr _init)
        {
            enum_name = _name;
            enum_init = _init;
        }
        public readonly String enum_name;
        public readonly Expr enum_init;

        public Tuple<AST.Env, String, Int32> GetEnumerator(AST.Env env, Int32 idx)
        {
            AST.Expr init;

            if (enum_init == null)
            {
                return new Tuple<AST.Env, String, int>(env, enum_name, idx);
            }

            init = enum_init.GetExpr(env);

            init = AST.TypeCast.MakeCast(init, new AST.TLong());
            if (!init.IsConstExpr())
            {
                throw new InvalidOperationException("Error: expected constant integer");
            }
            Int32 init_idx = ((AST.ConstLong)init).value;

            return new Tuple<AST.Env, String, int>(env, enum_name, init_idx);
        }
    }

    // StructOrUnionSpec
    // =================
    // a base class of StructSpec and UnionSpec
    // not present in the semant phase
    // 
    public abstract class StructOrUnionSpec : TypeSpec
    {
        public StructOrUnionSpec(String name, List<StructDeclaration> declns)
        {
            this.name = name;
            this.declns = declns;
        }
        public readonly String name;
        public readonly List<StructDeclaration> declns;

        public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetAttribs(AST.Env env)
        {
            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (StructDeclaration decln in declns)
            {
                Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_decln = decln.GetDeclns(env);
                env = r_decln.Item1;
                attribs.AddRange(r_decln.Item2);
            }
            return Tuple.Create(env, attribs);
        }

        public Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(Boolean is_struct, AST.Env env, Boolean is_const, Boolean is_volatile)
        {

            if (name == "")
            {
                // If no name is supplied: must be complete.
                // struct { ... } or union { ... }

                if (declns == null)
                {
                    throw new ArgumentNullException("Error: parser should ensure declns != null");
                }

                Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_attribs = GetAttribs(env);
                env = r_attribs.Item1;

                if (is_struct)
                {
                    return new Tuple<AST.Env, AST.ExprType>(env, AST.TStructOrUnion.CreateStruct("<anonymous>", r_attribs.Item2, is_const, is_volatile));
                }
                else
                {
                    return new Tuple<AST.Env, AST.ExprType>(env, AST.TStructOrUnion.CreateUnion("<anonymous>", r_attribs.Item2, is_const, is_volatile));
                }

            }
            else
            {
                // If a name is supplied, split into 2 cases.

                String typename = is_struct ? $"struct {name}" : $"union {name}";

                if (declns == null)
                {
                    // Case 1: If no attribute list supplied, then we are either
                    //       1) mentioning an already-existed struct/union
                    //    or 2) creating an incomplete struct/union

                    AST.Env.Entry r_find = env.Find(typename);

                    if (r_find.kind == AST.Env.EntryKind.NOT_FOUND)
                    {
                        // If the struct/union is not in the current environment,
                        // then add an incomplete struct/union into the environment
                        AST.ExprType type =
                            is_struct
                            ? AST.TStructOrUnion.CreateIncompleteStruct(name, is_const, is_volatile)
                            : AST.TStructOrUnion.CreateIncompleteUnion(name, is_const, is_volatile);

                        env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typename, type);
                        return Tuple.Create(env, type);
                    }

                    if (r_find.kind != AST.Env.EntryKind.TYPEDEF)
                    {
                        throw new InvalidProgramException(typename + " is not a type? This should be my fault.");
                    }

                    // If the struct/union is found, return it.
                    return Tuple.Create(env, r_find.type);

                }
                else
                {
                    // Case 2: If an attribute list is supplied.

                    // 1) Make sure there is no complete struct/union in the current environment.
                    AST.Env.Entry r_find = env.Find(typename + name);
                    if (r_find.type.kind == AST.ExprType.Kind.STRUCT_OR_UNION && ((AST.TStructOrUnion)r_find.type).IsComplete)
                    {
                        throw new InvalidOperationException($"Redefining {typename}");
                    }

                    // 2) Add an incomplete struct/union into the environment.
                    AST.TStructOrUnion type =
                        is_struct
                        ? AST.TStructOrUnion.CreateIncompleteStruct(name, is_const, is_volatile)
                        : AST.TStructOrUnion.CreateIncompleteUnion(name, is_const, is_volatile);
                    env = env.PushEntry(AST.Env.EntryKind.TYPEDEF, typename, type);

                    // 3) Iterate over the attributes.
                    Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> r_attribs = GetAttribs(env);
                    env = r_attribs.Item1;

                    // 4) Make the type complete. This would also change the entry inside env.
                    if (is_struct)
                    {
                        type.DefineStruct(r_attribs.Item2);
                    }
                    else
                    {
                        type.DefineUnion(r_attribs.Item2);
                    }

                    return new Tuple<AST.Env, AST.ExprType>(env, type);
                }
            }
        }
    }

    /// <summary>
    /// Struct Specifier
    /// 
    /// Specifies a struct type.
    /// 
    /// if name == "", then
    ///     the parser ensures that declns != null,
    ///     and this specifier does not change the environment
    /// if name != "", then
    ///     if declns == null
    ///        this means that this specifier is just mentioning a struct, not defining one, so
    ///        if the current environment doesn't have this struct type, then add an **incomplete** struct
    ///     if declns != null
    ///        this means that this specifier is defining a struct, so we need to perform the following steps:
    ///        1. make sure that the current environment doesn't have a **complete** struct of this name
    ///        2. immediately add an **incomplete** struct into the environment
    ///        3. iterate over the declns
    ///        4. finish forming a complete struct and add it into the environment
    /// </summary>
    public class StructSpecifier : StructOrUnionSpec
    {
        public StructSpecifier(String _name, List<StructDeclaration> _declns)
            : base(_name, _declns) { }

        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile) =>
            GetExprTypeEnv(true, env, is_const, is_volatile);
    }

    /// <summary>
    /// Union Specifier
    /// 
    /// Specifies a union type.
    /// 
    /// if name == "", then
    ///     the parser ensures that declns != null,
    ///     and this specifier does not change the environment
    /// if name != "", then
    ///     if declns == null
    ///        this means that this specifier is just mentioning a struct, not defining one, so
    ///        if the current environment doesn't have this union type, then add an **incomplete** struct
    ///     if declns != null
    ///        this means that this specifier is defining a struct, so we need to perform the following steps:
    ///        1. make sure that the current environment doesn't have a **complete** union of this name
    ///        2. immediately add an **incomplete** union into the environment
    ///        3. iterate over the declns
    ///        4. finish forming a complete union and add it into the environment
    /// </summary>
    public class UnionSpec : StructOrUnionSpec
    {
        public UnionSpec(String _name, List<StructDeclaration> _declns)
            : base(_name, _declns) { }

        public override Tuple<AST.Env, AST.ExprType> GetExprTypeEnv(AST.Env env, Boolean is_const, Boolean is_volatile) =>
            GetExprTypeEnv(false, env, is_const, is_volatile);

    }


    // StructOrUnion
    // =============
    // only used in parsing phase
    // 
    public class StructOrUnion : PTNode
    {
        public StructOrUnion(Boolean _is_union)
        {
            is_union = _is_union;
        }
        public Boolean is_union;
    }


    public class StructDeclaration : PTNode
    {
        public StructDeclaration(DeclnSpecs _specs, List<Declr> _declrs)
        {
            specs = _specs;
            declrs = _declrs;
        }
        public readonly DeclnSpecs specs;
        public readonly List<Declr> declrs;

        // Get Declarations : env -> (env, (name, type)[])
        // ===============================================
        // 
        public Tuple<AST.Env, List<Tuple<String, AST.ExprType>>> GetDeclns(AST.Env env)
        {
            Tuple<AST.Env, AST.ExprType> r_specs = specs.GetExprTypeEnv(env);
            env = r_specs.Item1;
            AST.ExprType base_type = r_specs.Item2;

            List<Tuple<String, AST.ExprType>> attribs = new List<Tuple<String, AST.ExprType>>();
            foreach (Declr declr in declrs)
            {
                Tuple<String, AST.ExprType> r_declr = declr.GetNameAndType(env, base_type);
                String name = r_declr.Item1;
                AST.ExprType type = r_declr.Item2;
                attribs.Add(new Tuple<String, AST.ExprType>(name, type));
            }
            return new Tuple<AST.Env, List<Tuple<String, AST.ExprType>>>(env, attribs);
        }

    }

    /// <summary>
    /// Parameter Declaration.
    /// 
    /// int foo(int arg0, int arg1);
    ///         ~~~~~~~~
    /// 
    /// int foo(int, int);
    ///         ~~~
    /// 
    /// The declarator can be completely omitted.
    /// </summary>
    public class ParamDecln : PTNode
    {
        public ParamDecln(DeclnSpecs specs, Option<Declr> declr)
        {
            this.specs = specs;
            this.declr = declr;
        }

        public readonly DeclnSpecs specs;    // base type
        public readonly Option<Declr> declr; // type modifiers and name

        public Tuple<String, AST.ExprType> GetParamDecln(AST.Env env)
        {

            Tuple<AST.Env, AST.Decln.SCS, AST.ExprType> r_specs = specs.GetSCSType(env);
            // TODO: check environment
            AST.Decln.SCS scs = r_specs.Item2;
            AST.ExprType type = r_specs.Item3;

            String name = "";
            if (declr.IsSome)
            {
                Tuple<String, AST.ExprType> r_declr = declr.value.GetNameAndType(env, type);
                name = r_declr.Item1;
                type = r_declr.Item2;
            }
            return Tuple.Create(name, type);
        }

    }

    public abstract class Initr : PTNode
    {
        public enum Kind
        {
            EXPR,
            INIT_LIST,
        }
        public Initr(Kind kind)
        {
            this.kind = kind;
        }
        public abstract Tuple<AST.Env, AST.Initr> GetInitr(AST.Env env);
        public readonly Kind kind;
    }

    // Initializer List
    // ================
    // used to initialize arrays and structs, etc
    // 
    // C language standard:
    // 1. scalar types
    //    
    // 2. aggregate types
    // 3. strings
    public class InitList : Initr
    {
        public InitList(List<Initr> initrs)
            : base(Kind.INIT_LIST)
        {
            this.initrs = initrs;
        }
        public readonly List<Initr> initrs;
        public override Tuple<AST.Env, AST.Initr> GetInitr(AST.Env env)
        {
            List<AST.Initr> initrs = this.initrs.ConvertAll(initr => {
                Tuple<AST.Env, AST.Initr> r_initr = initr.GetInitr(env);
                env = r_initr.Item1;
                return r_initr.Item2;
            });
            return new Tuple<AST.Env, AST.Initr>(env, new AST.InitList(initrs));
        }
    }

    public class InitExpr : Initr
    {
        public InitExpr(Expr expr)
            : base(Kind.EXPR)
        {
            this.expr = expr;
        }
        public readonly Expr expr;
        public override Tuple<AST.Env, AST.Initr> GetInitr(AST.Env env)
        {
            // TODO: expr should change env
            return new Tuple<AST.Env, AST.Initr>(env, new AST.InitExpr(expr.GetExpr(env)));
        }
    }

    // Type Name
    // =========
    // describes a qualified type
    // 
    public class TypeName : PTNode
    {
        public TypeName(DeclnSpecs specs, Declr declr)
        {
            this.specs = specs;
            this.declr = declr;
        }

        public readonly DeclnSpecs specs;
        public readonly Declr declr;

        // TODO: check env
        public AST.ExprType GetExprType(AST.Env env)
        {
            AST.ExprType type = specs.GetExprTypeEnv(env).Item2;
            return declr.GetNameAndType(env, type).Item2;
        }

    }
}