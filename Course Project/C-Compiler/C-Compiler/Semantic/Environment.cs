﻿using System;
using System.Collections.Generic;

namespace AST
{
    public class Env
    {

        // enum EntryLoc
        // =============
        // the location of an object
        //   STACK: this is a variable stored in the stack
        //   FRAME: this is a function parameter
        //   GLOBAL: this is a global symbol
        // 
        public enum EntryLoc
        {
            NOT_FOUND,
            ENUM,
            TYPEDEF,
            STACK,
            FRAME,
            GLOBAL,
        }


        // class Entry
        // ===========
        // the return value when searching for a symbol in the environment
        // attributes:
        //   entry_loc: the location of this object
        //   entry_type: the type of the object
        //   entry_offset: this is used to determine the address of the object
        //              STACK: addr = %ebp - offset
        //              GLOBAL: N/A
        // 
        public class Entry
        {
            public Entry(EntryLoc loc, ExprType type, int offset)
            {
                entry_loc = loc;
                entry_type = type;
                entry_offset = offset;
            }
            public readonly EntryLoc entry_loc;
            public readonly ExprType entry_type;
            public readonly int entry_offset;
        }

        private class Scope
        {

            // private constructor
            // ===================
            // 
            private Scope(List<Utils.StoreEntry> stack_entries,
                          int stack_offset,
                          List<Utils.StoreEntry> global_entries,
                          TFunction curr_func,
                          List<Utils.StoreEntry> typedef_entries,
                          List<Utils.StoreEntry> enum_entries)
            {
                scope_stack_entries = stack_entries;
                scope_stack_offset = stack_offset;
                scope_global_entries = global_entries;
                scope_curr_func = curr_func;
                scope_typedef_entries = typedef_entries;
                scope_enum_entries = enum_entries;
            }

            // copy constructor
            // ================
            // 
            private Scope(Scope other)
                : this(new List<Utils.StoreEntry>(other.scope_stack_entries),
                       other.scope_stack_offset,
                       new List<Utils.StoreEntry>(other.scope_global_entries),
                       other.scope_curr_func,
                       new List<Utils.StoreEntry>(other.scope_typedef_entries),
                       new List<Utils.StoreEntry>(other.scope_enum_entries))
            { }

            // empty Scope
            // ===========
            // 
            public Scope()
                : this(new List<Utils.StoreEntry>(),
                       0,
                       new List<Utils.StoreEntry>(),
                       new TEmptyFunction(),
                       new List<Utils.StoreEntry>(),
                       new List<Utils.StoreEntry>())
            { }


            // InScope
            // =======
            // create a new scope with:
            //   the same stack offset
            //   the same current function
            //   other entries are empty
            // 
            public Scope InScope()
            {
                return new Scope(new List<Utils.StoreEntry>(),
                                 scope_stack_offset,
                                 new List<Utils.StoreEntry>(),
                                 scope_curr_func,
                                 new List<Utils.StoreEntry>(),
                                 new List<Utils.StoreEntry>());
            }


            // PushEntry
            // =========
            // input: loc, name, type
            // output: Scope
            // returns a new scope with everything the same as this, excpet for a new entry
            // 
            public Scope PushEntry(EntryLoc loc, String name, ExprType type)
            {
                Scope scope = new Scope(this);
                switch (loc)
                {
                    case EntryLoc.STACK:
                        scope.scope_stack_offset += type.SizeOf;
                        scope.scope_stack_offset = Utils.RoundUp(scope.scope_stack_offset, type.Alignment);
                        scope.scope_stack_entries.Add(new Utils.StoreEntry(name, type, scope.scope_stack_offset));
                        break;
                    case EntryLoc.GLOBAL:
                        scope.scope_global_entries.Add(new Utils.StoreEntry(name, type, 0));
                        break;
                    case EntryLoc.TYPEDEF:
                        scope.scope_typedef_entries.Add(new Utils.StoreEntry(name, type, 0));
                        break;
                    default:
                        return null;
                }
                return scope;
            }


            // PushEnum
            // ========
            // input: name, type
            // output: Environment
            // return a new environment which adds a enum value
            // 
            public Scope PushEnum(String name, ExprType type, int value)
            {
                Scope scope = new Scope(this);
                scope.scope_enum_entries.Add(new Utils.StoreEntry(name, type, value));
                return scope;
            }


            // SetCurrFunc
            // ===========
            // set the current function
            public Scope SetCurrentFunction(TFunction type)
            {
                Scope scope = new Scope(this);
                scope.scope_curr_func = type;
                return scope;
            }


            // Find
            // ====
            // input: name
            // output: Entry
            // search for a symbol in the current scope
            // 
            public Entry Find(String name)
            {
                Utils.StoreEntry store_entry;

                // search the enum entries
                if ((store_entry = scope_enum_entries.FindLast(entry => entry.entry_name == name)) != null)
                {
                    return new Entry(EntryLoc.ENUM, store_entry.entry_type, store_entry.entry_offset);
                }

                // search the typedef entries
                if ((store_entry = scope_typedef_entries.FindLast(entry => entry.entry_name == name)) != null)
                {
                    return new Entry(EntryLoc.TYPEDEF, store_entry.entry_type, store_entry.entry_offset);
                }

                // search the stack entries
                if ((store_entry = scope_stack_entries.FindLast(entry => entry.entry_name == name)) != null)
                {
                    return new Entry(EntryLoc.STACK, store_entry.entry_type, store_entry.entry_offset);
                }

                // search the function arguments
                if ((store_entry = scope_curr_func.args.FindLast(entry => entry.entry_name == name)) != null)
                {
                    return new Entry(EntryLoc.FRAME, store_entry.entry_type, store_entry.entry_offset);
                }

                // search the global entries
                if ((store_entry = scope_global_entries.FindLast(entry => entry.entry_name == name)) != null)
                {
                    return new Entry(EntryLoc.GLOBAL, store_entry.entry_type, store_entry.entry_offset);
                }

                return null;
            }


            // Dump
            // ====
            // input: depth, indent
            // output: String
            // dumps the content in this level
            // 
            public String Dump(int depth, String single_indent)
            {
                String indent = "";
                for (; depth > 0; depth--)
                {
                    indent += single_indent;
                }

                String str = "";
                foreach (Utils.StoreEntry entry in scope_curr_func.args)
                {
                    str += indent;
                    str += "[%ebp + " + entry.entry_offset + "] " + entry.entry_name + " : " + entry.entry_type.ToString() + "\n";
                }
                foreach (Utils.StoreEntry entry in scope_global_entries)
                {
                    str += indent;
                    str += "[extern] " + entry.entry_name + " : " + entry.entry_type.ToString() + "\n";
                }
                foreach (Utils.StoreEntry entry in scope_stack_entries)
                {
                    str += indent;
                    str += "[%ebp - " + entry.entry_offset + "] " + entry.entry_name + " : " + entry.entry_type.ToString() + "\n";
                }
                foreach (Utils.StoreEntry entry in scope_typedef_entries)
                {
                    str += indent;
                    str += "typedef: " + entry.entry_name + " <- " + entry.entry_type.ToString() + "\n";
                }
                foreach (Utils.StoreEntry entry in scope_enum_entries)
                {
                    str += indent;
                    str += entry.entry_name + " = " + entry.entry_offset + "\n";
                }
                return str;

            }


            // ================================================================
            //  private members
            // ================================================================
            private List<Utils.StoreEntry> scope_stack_entries;
            private TFunction scope_curr_func;
            private List<Utils.StoreEntry> scope_global_entries;
            private List<Utils.StoreEntry> scope_typedef_entries;
            private List<Utils.StoreEntry> scope_enum_entries;

            private int scope_stack_offset;

        }

        // Environment
        // ===========
        // construct an environment with a single empty scope
        public Env()
        {
            env_scopes = new Stack<Scope>();
            env_scopes.Push(new Scope());
        }

        // Environment
        // ===========
        // construct an environment with the given scopes
        // 
        private Env(Stack<Scope> scopes)
        {
            env_scopes = scopes;
        }

        // InScope
        // =======
        // input: void
        // output: Environment
        // return a new environment which has a new inner scope
        // 
        public Env InScope()
        {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(env_scopes));
            scopes.Push(new Scope());
            return new Env(scopes);
        }

        // OutScope
        // ========
        // input: void
        // output: Environment
        // return a new environment which goes out of the most inner scope of the current environment
        // 
        public Env OutScope()
        {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(env_scopes));
            scopes.Pop();
            return new Env(scopes);
        }

        // PushEntry
        // =========
        // input: loc, name, type
        // ouput: Environment
        // return a new environment which adds a symbol entry
        // 
        public Env PushEntry(EntryLoc loc, String name, ExprType type)
        {
            // note the nested copy constructor. this is because the constructor would reverse the elements.
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(env_scopes));
            Scope top = scopes.Pop().PushEntry(loc, name, type);
            scopes.Push(top);
            return new Env(scopes);
        }

        // PushEnum
        // ========
        // input: name, type
        // output: Environment
        // return a new environment which adds a enum value
        // 
        public Env PushEnum(String name, ExprType type, int value)
        {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(env_scopes));
            Scope top = scopes.Pop().PushEnum(name, type, value);
            scopes.Push(top);
            return new Env(scopes);
        }

        // SetCurrentFunction
        // ==================
        // input: type
        // ouput: Environment
        // return a new environment which sets the current function
        // 
        public Env SetCurrentFunction(TFunction type)
        {
            Stack<Scope> scopes = new Stack<Scope>(new Stack<Scope>(env_scopes));
            Scope top = scopes.Pop().SetCurrentFunction(type);
            scopes.Push(top);
            return new Env(scopes);
        }

        public Entry Find(String name)
        {
            Entry entry = null;
            foreach (Scope scope in env_scopes)
            {
                if ((entry = scope.Find(name)) != null)
                {
                    return entry;
                }
            }
            return entry;
        }

        public Entry FindInCurrentScope(String name)
        {
            return env_scopes.Peek().Find(name);
        }

        public bool IsGlobal()
        {
            return env_scopes.Count == 1;
        }

        public String Dump()
        {
            String str = "";
            int depth = 0;
            foreach (Scope scope in env_scopes)
            {
                str += scope.Dump(depth, "  ");
                depth++;
            }
            return str;
        }

        private readonly Stack<Scope> env_scopes;

    }
}