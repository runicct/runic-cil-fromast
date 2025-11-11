/*
 * MIT License
 * 
 * Copyright (c) 2025 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Runic.AST;
using System;
using System.Collections.Generic;
using static Runic.AST.Node.Expression;
using static Runic.AST.Node.Expression.Constant;

namespace Runic.CIL
{
    public abstract partial class FromAST
    {
        public abstract uint PointerSize { get; }
        public abstract uint Packing { get; }
        public abstract uint Padding { get; }
        class Context
        {
            Signature _localSignature = new Signature();
            public Signature LocalSignature { get { return _localSignature; } }
            Runic.CIL.Emitter _emitter;
            public Runic.CIL.Emitter Emitter { get { return _emitter; } }
            public Context(Runic.CIL.Emitter emitter)
            {
                _emitter = emitter;
            }
            int _maxStackSize = 0;
            public int MaxStackSize { get { return _maxStackSize; } }
            public void BumpMaxStackSize(int size) { if (size > _maxStackSize) { _maxStackSize = size; } }
            Dictionary<ulong, Runic.CIL.Emitter.Label> _labels = new Dictionary<ulong, Runic.CIL.Emitter.Label>();
            public Runic.CIL.Emitter.Label GetLabel(Runic.AST.Node.Label label)
            {
                lock (_labels)
                {
                    if (!_labels.TryGetValue(label.ID, out var cilLabel))
                    {
                        cilLabel = _emitter.DeclareLabel();
                        _labels.Add(label.ID, cilLabel);
                    }
                    return cilLabel;
                }
            }
            int _nextVariable = 0;
            Dictionary<int, int> _tempVariables = new Dictionary<int, int>();
            Dictionary<ulong, int> _localVariables = new Dictionary<ulong, int>();
            public int GetLocal(Runic.AST.Variable.LocalVariable localVariable)
            {
                int index = 0;
                if (!_localVariables.TryGetValue(localVariable.Index, out index))
                {
                    index = _nextVariable;
                    _nextVariable += 1;
                    _localVariables.Add(localVariable.Index, index);
                    _localSignature.EmitLocal(localVariable.Type);
                }
                return (int)localVariable.Index;
            }
            public int GetTempLocal(Runic.AST.Type type, int index)
            {
                int tempindex = 0;
                if (!_tempVariables.TryGetValue(index, out tempindex))
                {
                    tempindex = _nextVariable;
                    _nextVariable += 1;
                    _tempVariables.Add(index, tempindex);
                    _localSignature.EmitLocal(type);
                }
                return tempindex;
            }
        }

        protected abstract uint GetTypeToken(Runic.AST.Type type);
        protected abstract uint GetStaticFieldToken(Runic.AST.Variable.GlobalVariable variable);
        protected abstract uint GetConstantStringToken(string str);
        protected abstract uint GetConstantByteArrayToken(byte[] array);
        protected abstract uint GetFunctionToken(Runic.AST.Node.Function function);
        void ConstantToCIL(Context context, Runic.AST.Node.Expression.Constant constant, ref int stackSize)
        {
            stackSize += 1;
            context.BumpMaxStackSize(stackSize);
            switch (constant)
            {
                case Runic.AST.Node.Expression.Constant.I8 i8: context.Emitter.LdcI4(i8.Value); break;
                case Runic.AST.Node.Expression.Constant.I16 i16: context.Emitter.LdcI4(i16.Value); break;
                case Runic.AST.Node.Expression.Constant.I32 i32: context.Emitter.LdcI4(i32.Value); break;
                case Runic.AST.Node.Expression.Constant.I64 i64: context.Emitter.LdcI8(i64.Value); break;
            }
        }
        void AddToCIL(Context context, Runic.AST.Node.Expression.Add add, ref int stackSize)
        {
            NodeToCIL(context, add.Left, ref stackSize);
            NodeToCIL(context, add.Right, ref stackSize);
            stackSize -= 1;
            switch (add.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Add(); context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.Add(); context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.Add(); break;
                        case 64: context.Emitter.Add(); break;
                    }
                    break;
            }
        }
        void SubToCIL(Context context, Runic.AST.Node.Expression.Sub sub, ref int stackSize)
        {
            NodeToCIL(context, sub.Left, ref stackSize);
            NodeToCIL(context, sub.Right, ref stackSize);
            stackSize -= 1;
            switch (sub.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Sub(); context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.Sub(); context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.Sub(); break;
                        case 64: context.Emitter.Sub(); break;
                    }
                    break;
            }
        }
        void MulToCIL(Context context, Runic.AST.Node.Expression.Mul mul, ref int stackSize)
        {
            NodeToCIL(context, mul.Left, ref stackSize);
            NodeToCIL(context, mul.Right, ref stackSize);
            stackSize -= 1;
            switch (mul.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Mul(); context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.Mul(); context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.Mul(); break;
                        case 64: context.Emitter.Mul(); break;
                    }
                    break;
            }
        }
        void DivToCIL(Context context, Runic.AST.Node.Expression.Div div, ref int stackSize)
        {
            NodeToCIL(context, div.Left, ref stackSize);
            NodeToCIL(context, div.Right, ref stackSize);
            stackSize -= 1;
            switch (div.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Div(); context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.Div(); context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.Div(); break;
                        case 64: context.Emitter.Div(); break;
                    }
                    break;
            }
        }
        void RemToCIL(Context context, Runic.AST.Node.Expression.Rem rem, ref int stackSize)
        {
            NodeToCIL(context, rem.Left, ref stackSize);
            NodeToCIL(context, rem.Right, ref stackSize);
            stackSize -= 1;
            switch (rem.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Rem(); context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.Rem(); context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.Rem(); break;
                        case 64: context.Emitter.Rem(); break;
                    }
                    break;
            }
        }
        void ShlToCIL(Context context, Runic.AST.Node.Expression.Shl shl, ref int stackSize)
        {
            NodeToCIL(context, shl.Left, ref stackSize);
            NodeToCIL(context, shl.Right, ref stackSize);
            stackSize -= 1;
            switch (shl.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Shl(); context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.Shl(); context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.Shl(); break;
                        case 64: context.Emitter.Shl(); break;
                    }
                    break;
            }
        }
        void ShrToCIL(Context context, Runic.AST.Node.Expression.Shr shr, ref int stackSize)
        {
            NodeToCIL(context, shr.Left, ref stackSize);
            NodeToCIL(context, shr.Right, ref stackSize);
            stackSize -= 1;
            switch (shr.Type)
            {
                case Runic.AST.Type.Integer integer:
                    if (integer.Signed)
                    {
                        switch (integer.Bits)
                        {
                            case 8: context.Emitter.Shr(); context.Emitter.ConvI1(); break;
                            case 16: context.Emitter.Shr(); context.Emitter.ConvI2(); break;
                            case 32: context.Emitter.Shr(); break;
                            case 64: context.Emitter.Shr(); break;
                        }
                    }
                    else
                    {
                        switch (integer.Bits)
                        {
                            case 8: context.Emitter.ShrUn(); context.Emitter.ConvU1(); break;
                            case 16: context.Emitter.ShrUn(); context.Emitter.ConvU2(); break;
                            case 32: context.Emitter.ShrUn(); break;
                            case 64: context.Emitter.ShrUn(); break;
                        }
                    }
                    break;
            }
        }
        void ReturnToCIL(Context context, Runic.AST.Node.Expression.Return ret, ref int stackSize)
        {
            if (ret.Value != null)
            {
                NodeToCIL(context, ret.Value, ref stackSize);
                stackSize = 0;
            }
            context.Emitter.Ret();
        }
        void CastToCIL(Context context, Runic.AST.Node.Expression.Cast cast, ref int stackSize)
        {
            NodeToCIL(context, cast.Value, ref stackSize);
            switch (cast.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.ConvI1(); break;
                        case 16: context.Emitter.ConvI2(); break;
                        case 32: context.Emitter.ConvI4(); break;
                        case 64: context.Emitter.ConvI8(); break;
                    }
                    break;
                case Runic.AST.Type.FloatingPoint floatingPoint:
                    switch (floatingPoint.Bits)
                    {
                        case 32: context.Emitter.ConvR4(); break;
                        case 64: context.Emitter.ConvR8(); break;
                    }
                    break;
                case Runic.AST.Type.Pointer pointer: context.Emitter.ConvI(); break;
            }
        }
        void SequenceToCIL(Context context, Runic.AST.Node.Expression.Sequence seq, ref int stackSize)
        {
            NodeToCIL(context, seq.First, ref stackSize);
            if (!(seq.First.Type is Runic.AST.Type.Void)) { stackSize -= 1; context.Emitter.Pop(); }
            NodeToCIL(context, seq.Second, ref stackSize);
        }
        void DereferenceToCIL(Context context, Runic.AST.Node.Expression.Dereference deref, ref int stackSize)
        {
            NodeToCIL(context, deref.Address, ref stackSize);
            switch (deref.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.LdIndI1(false); break;
                        case 16: context.Emitter.LdIndI2(false, 0); break;
                        case 32: context.Emitter.LdIndI4(false, 0); break;
                        case 64: context.Emitter.LdIndI8(false, 0); break;
                    }
                    break;
                case Runic.AST.Type.FloatingPoint floatingPoint:
                    switch (floatingPoint.Bits)
                    {
                        case 32: context.Emitter.LdIndR4(false, 0); break;
                        case 64: context.Emitter.LdIndR8(false, 0); break;
                    }
                    break;
                case Runic.AST.Type.Pointer pointer: context.Emitter.LdIndI(false, 0); break;
                default:
                    context.Emitter.LdObj(false, 0, GetTypeToken(deref.Type));
                    break;
            }
        }
        void BranchToCIL(Context context, Runic.AST.Node.Branch branch) { context.Emitter.Br(context.GetLabel(branch.Target)); }
        void VariableUseToCIL(Context context, Runic.AST.Node.Expression.VariableUse varUse, ref int stackSize)
        {
            stackSize += 1;
            context.BumpMaxStackSize(stackSize);
            switch (varUse.Variable)
            {
                case Variable.FunctionParameter funcParam: context.Emitter.LdArg((int)funcParam.Index); break;
                case Variable.LocalVariable localVar: context.Emitter.LdLoc(context.GetLocal(localVar)); break;
                case Variable.GlobalVariable globalVar: context.Emitter.LdSFld(false, GetStaticFieldToken(globalVar)); break;
            }
        }
        void IndexingToCIL(Context context, Runic.AST.Node.Expression.Indexing indexing, ref int stackSize)
        {
            NodeToCIL(context, indexing.Address, ref stackSize);
            NodeToCIL(context, indexing.Index, ref stackSize);
            switch (indexing.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Add(); context.Emitter.LdIndI1(false); break;
                        case 16: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(2); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); context.Emitter.LdIndI2(false, 0); break;
                        case 32: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(4); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); context.Emitter.LdIndI4(false, 0); break;
                        case 64: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(8); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); context.Emitter.LdIndI8(false, 0); break;
                    }
                    break;
                case Runic.AST.Type.FloatingPoint floatingPoint:
                    switch (floatingPoint.Bits)
                    {
                        case 32: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(2); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); context.Emitter.LdIndR4(false, 0); break;
                        case 64: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(2); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); context.Emitter.LdIndR8(false, 0); break;
                    }
                    break;
                case Runic.AST.Type.Pointer pointer: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4((int)PointerSize); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); context.Emitter.LdIndI(false, 0); break;
                default:
                    context.BumpMaxStackSize(stackSize + 1);
                    ulong size = indexing.Type.SizeOf(PointerSize, Packing, Padding);
                    context.Emitter.LdcI4((int)size); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add();
                    context.Emitter.LdObj(false, 0, GetTypeToken(indexing.Type));
                    break;
            }
            stackSize -= 1;
        }
        void IncrementPostfixVariableToCIL(Context context, Runic.AST.Node.Expression.Increment.Postfix.Variable increment, ref int stackSize)
        {
            stackSize += 1;
            context.BumpMaxStackSize(stackSize + 2);
            switch (increment.Target)
            {
                case Variable.FunctionParameter funcParam: context.Emitter.LdArg((int)funcParam.Index); break;
                case Variable.LocalVariable localVar: context.Emitter.LdLoc(context.GetLocal(localVar)); break;
                case Variable.GlobalVariable globalVar: context.Emitter.LdSFld(false, GetStaticFieldToken(globalVar)); break;
            }
            context.Emitter.Dup();
            switch (increment.Target.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.LdcI4(1); context.Emitter.Add(); if (integer.Signed) { context.Emitter.ConvI1(); } else { context.Emitter.ConvU1(); } break;
                        case 16: context.Emitter.LdcI4(1); context.Emitter.Add(); if (integer.Signed) { context.Emitter.ConvI2(); } else { context.Emitter.ConvU2(); } break;
                        case 32: context.Emitter.LdcI4(1); context.Emitter.Add(); break;
                        case 64: context.Emitter.LdcI8(1); context.Emitter.Add(); break;
                    }
                    break;
                case Runic.AST.Type.FloatingPoint floatingPoint:
                    switch (floatingPoint.Bits)
                    {
                        case 32: context.Emitter.LdcR4(1.0f); context.Emitter.Add(); break;
                        case 64: context.Emitter.LdcR8(1.0); context.Emitter.Add(); break;
                    }
                    break;
                case Runic.AST.Type.Pointer pointer:
                    context.Emitter.LdcI4(1); context.Emitter.ConvI(); context.Emitter.Add();
                    break;
            }
            switch (increment.Target)
            {
                case Variable.FunctionParameter funcParam: context.Emitter.StArg((int)funcParam.Index); break;
                case Variable.LocalVariable localVar: context.Emitter.StLoc(context.GetLocal(localVar)); break;
                case Variable.GlobalVariable globalVar: context.Emitter.StSFld(false, GetStaticFieldToken(globalVar)); break;
            }
        }
        void IncrementPostfixDerefToCIL(Context context, Runic.AST.Node.Expression.Increment.Postfix.Dereference increment, ref int stackSize)
        {
            throw new NotImplementedException();
        }
        void LabelToCIL(Context context, Runic.AST.Node.Label label) { context.Emitter.MarkLabel(context.GetLabel(label)); }
        void ComparisonToCIL(Context context, Runic.AST.Node.Expression.Comparison comparison, ref int stackSize)
        {
            NodeToCIL(context, comparison.Left, ref stackSize);
            NodeToCIL(context, comparison.Right, ref stackSize);
            stackSize -= 1;
            switch (comparison.Operation)
            {
                case Runic.AST.Node.Expression.Comparison.ComparisonOperation.Equal: context.Emitter.Ceq(); break;
                case Runic.AST.Node.Expression.Comparison.ComparisonOperation.NotEqual: context.Emitter.Ceq(); context.Emitter.LdcI4(0); context.Emitter.Ceq(); break;
                case Runic.AST.Node.Expression.Comparison.ComparisonOperation.LowerThan: context.Emitter.Clt(); break;
                case Runic.AST.Node.Expression.Comparison.ComparisonOperation.LowerOrEqual: context.Emitter.Cgt(); context.Emitter.LdcI4(0); context.Emitter.Ceq(); break;
                case Runic.AST.Node.Expression.Comparison.ComparisonOperation.GreaterThan: context.Emitter.Cgt(); break;
                case Runic.AST.Node.Expression.Comparison.ComparisonOperation.GreaterOrEqual: context.Emitter.Clt(); context.Emitter.LdcI4(0); context.Emitter.Ceq(); break;
            }
        }
        void VariableReferenceToCIL(Context context, Runic.AST.Node.Expression.VariableReference reference, ref int stackSize)
        {
            stackSize += 1;
            context.BumpMaxStackSize(stackSize);
            switch (reference.Variable)
            {
                case Variable.FunctionParameter funcParam: context.Emitter.LdArgA((int)funcParam.Index); break;
                case Variable.LocalVariable localVar: context.Emitter.LdLocA(context.GetLocal(localVar)); break;
                case Variable.GlobalVariable globalVar: context.Emitter.LdSFldA(GetStaticFieldToken(globalVar)); break;
            }
        }
        void IndexingReferenceToCIL(Context context, Runic.AST.Node.Expression.IndexingReference reference, ref int stackSize)
        {
            NodeToCIL(context, reference.Address, ref stackSize);
            NodeToCIL(context, reference.Index, ref stackSize);
            switch (reference.Type)
            {
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.Add(); break;
                        case 16: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(2); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); break;
                        case 32: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(4); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); break;
                        case 64: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(8); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); break;
                    }
                    break;
                case Runic.AST.Type.FloatingPoint floatingPoint:
                    switch (floatingPoint.Bits)
                    {
                        case 32: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(2); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); break;
                        case 64: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4(2); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); break;
                    }
                    break;
                case Runic.AST.Type.Pointer pointer: context.BumpMaxStackSize(stackSize + 1); context.Emitter.LdcI4((int)PointerSize); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add(); break;
                default:
                    context.BumpMaxStackSize(stackSize + 1);
                    ulong size = reference.Type.SizeOf(PointerSize, Packing, Padding);
                    context.Emitter.LdcI4((int)size); context.Emitter.ConvI(); context.Emitter.Mul(); context.Emitter.Add();
                    break;
            }
            stackSize -= 1;
        }
        void IfToCIL(Context context, Runic.AST.Node.If @if, ref int stackSize)
        {
            NodeToCIL(context, @if.Condition, ref stackSize);
            stackSize -= 1;
            if (@if.ElseBody != null && @if.ElseBody.Length > 0)
            {
                Emitter.Label onFalse = context.Emitter.DeclareLabel();
                Emitter.Label end = context.Emitter.DeclareLabel();
                context.Emitter.BrFalse(onFalse);
                if (@if.Body != null && @if.Body.Length > 0)
                {
                    for (int n = 0; n < @if.Body.Length; n++) { NodeToCIL(context, @if.Body[n], ref stackSize); }
                }
                context.Emitter.Br(end);
                context.Emitter.MarkLabel(onFalse);
                for (int n = 0; n < @if.Body.Length; n++) { NodeToCIL(context, @if.ElseBody[n], ref stackSize); }
                context.Emitter.MarkLabel(end);
                context.Emitter.Nop();
            }
            else
            {
                Emitter.Label end = context.Emitter.DeclareLabel();
                context.Emitter.BrFalse(end);
                for (int n = 0; n < @if.Body.Length; n++) { NodeToCIL(context, @if.Body[n], ref stackSize); }
                context.Emitter.MarkLabel(end);
                context.Emitter.Nop();
            }
        }

        void CallToCIL(Context context, Runic.AST.Node.Expression.Call call, ref int stackSize)
        {
            for (int n = 0; n < call.Parameters.Length; n++)
            {
                NodeToCIL(context, call.Parameters[n], ref stackSize);
            }
            stackSize -= call.Parameters.Length;
            if (!(call.Function.ReturnType is Runic.AST.Type.Void)) { stackSize += 1; }
            context.BumpMaxStackSize(stackSize);
            context.Emitter.Call(false, GetFunctionToken(call.Function));
        }

        bool TryGetLabelValues(Runic.AST.Node.Switch @switch, out Dictionary<int, Runic.AST.Node.Label> values)
        {
            values = new Dictionary<int, Runic.AST.Node.Label>();
            for (int n = 0; n < @switch.Cases.Length; n++)
            {
                Runic.AST.Node.Switch.Case @case = @switch.Cases[n];
                switch (@case.Value)
                {
                    case Runic.AST.Node.Expression.Constant.I8 i8: values.Add((int)i8.Value, @case.Label); break;
                    case Runic.AST.Node.Expression.Constant.I16 i16: values.Add((int)i16.Value, @case.Label); break;
                    case Runic.AST.Node.Expression.Constant.I32 i32: values.Add((int)i32.Value, @case.Label); break;
                    case Runic.AST.Node.Expression.Constant.I64 i64: if (i64.Value <= (long)int.MaxValue && i64.Value >= (long)int.MinValue) { values.Add((int)i64.Value, @case.Label); } else { return false; } break;
                    default: return false;
                }
            }
            return true;
        }

        void SwitchToCIL(Context context, Runic.AST.Node.Switch @switch, ref int stackSize)
        {
            NodeToCIL(context, @switch.Value, ref stackSize);
            Dictionary<int, Runic.AST.Node.Label> values;
            if (TryGetLabelValues(@switch, out values))
            {
                if (values.Count > 0)
                {
                    List<int> valueList = new List<int>(values.Keys);
                    valueList.Sort();
                    int minValue = valueList[0];
                    int maxValue = valueList[0];
                    for (int n = 1; n < valueList.Count; n++)
                    {
                        if (valueList[n] < minValue) { minValue = valueList[n]; }
                        if (valueList[n] > maxValue) { maxValue = valueList[n]; }
                    }
                    long span = (long)maxValue - (long)minValue + 1;
                    if (span < uint.MaxValue)
                    {
                        // Heuristic: use a switch table only if the density of label is at least 25%
                        if (span < ((long)valueList.Count) * 4L)
                        {
                            if (minValue != 0)
                            {
                                stackSize += 2;
                                context.BumpMaxStackSize(stackSize);
                                context.Emitter.LdcI4(minValue);
                                context.Emitter.Sub();
                                stackSize -= 2;
                            }
                            else
                            {
                                stackSize -= 1;
                            }
                            Runic.CIL.Emitter.Label[] labels = new Runic.CIL.Emitter.Label[span];
                            Runic.CIL.Emitter.Label defaultLabel = null;
                            for (int n = 0, valueIndex = 0; n < labels.Length; n++)
                            {
                                int targetValue = valueList[valueIndex] - minValue;
                                if (targetValue == n)
                                {
                                    labels[n] = context.GetLabel(values[valueList[valueIndex]]);
                                    valueIndex++;
                                }
                                else
                                {
                                    if (defaultLabel == null) { defaultLabel = context.Emitter.DeclareLabel(); }
                                    labels[n] = defaultLabel;
                                }
                            }
                            context.Emitter.Switch(labels);
                            if (defaultLabel != null) { context.Emitter.MarkLabel(defaultLabel); context.Emitter.Nop(); }
                            return;
                        }

                    }
                }
            }
            {
                stackSize -= 1;

                // Can't build a compact jump table, use a series of comparisons

                int switchLocal = context.GetTempLocal(@switch.Value.Type, 1);
                context.Emitter.StLoc(switchLocal);
                for (int n = 0; n < @switch.Cases.Length; n++)
                {
                    Runic.AST.Node.Switch.Case @case = @switch.Cases[n];
                    context.Emitter.LdLoc(switchLocal);
                    stackSize += 1;
                    NodeToCIL(context, @case.Value, ref stackSize);
                    context.Emitter.Ceq();
                    context.Emitter.BrTrue(context.GetLabel(@case.Label));
                    stackSize -= 2;
                }
            }
        }
        void VariableAssignmentToCIL(Context context, Runic.AST.Node.Expression.VariableAssignment varassignment, ref int stackSize)
        {
            if (varassignment.Value == null) { return; }

            switch (varassignment.Value)
            {
                case Runic.AST.Node.Expression.Constant.String strConst:

                    // Constant string assignment is a special case because we need to know if we are loading a string
                    // or a char array in case of a char*.
                    switch (varassignment.Variable.Type)
                    {

                        case Runic.AST.Type.Pointer pointer:  context.Emitter.LdSFldA(GetConstantStringToken(strConst.Value)); break;
                        default: context.Emitter.LdStr(GetConstantStringToken(strConst.Value)); break;
                    }
                    break;
                default:
                    NodeToCIL(context, varassignment.Value, ref stackSize);
                    break;
            }
            context.Emitter.Dup();
            stackSize += 1;
            context.BumpMaxStackSize(stackSize);
            switch (varassignment.Variable)
            {
                case Variable.FunctionParameter funcParam: context.Emitter.StArg((int)funcParam.Index); break;
                case Variable.LocalVariable localVar: context.Emitter.StLoc(context.GetLocal(localVar)); break;
                case Variable.GlobalVariable globalVar: context.Emitter.StSFld(false, GetStaticFieldToken(globalVar)); break;
            }
            stackSize -= 1;
        }
        Runic.AST.Type.Pointer _voidPointer = new AST.Type.Pointer(new AST.Type.Void());
        void DereferenceAssignmentToCIL(Context context, Runic.AST.Node.Expression.DereferenceAssignment derefassignment, ref int stackSize)
        {
            throw new NotImplementedException();
        }
        void NodeToCIL(Context context, Runic.AST.Node node, ref int stackSize)
        {
            switch (node)
            {
                case Runic.AST.Node.Expression.VariableAssignment varassignment: VariableAssignmentToCIL(context, varassignment, ref stackSize); break;
                case Runic.AST.Node.Expression.DereferenceAssignment derefassignment: DereferenceAssignmentToCIL(context, derefassignment, ref stackSize); break;
                case Runic.AST.Node.Expression.Return ret: ReturnToCIL(context, ret, ref stackSize); break;
                case Runic.AST.Node.Expression.Constant constant: ConstantToCIL(context, constant, ref stackSize); break;
                case Runic.AST.Node.Expression.Add add: AddToCIL(context, add, ref stackSize); break;
                case Runic.AST.Node.Expression.Sub sub: SubToCIL(context, sub, ref stackSize); break;
                case Runic.AST.Node.Expression.Mul mul: MulToCIL(context, mul, ref stackSize); break;
                case Runic.AST.Node.Expression.Div div: DivToCIL(context, div, ref stackSize); break;
                case Runic.AST.Node.Expression.Rem rem: RemToCIL(context, rem, ref stackSize); break;
                case Runic.AST.Node.Expression.Shl shl: ShlToCIL(context, shl, ref stackSize); break;
                case Runic.AST.Node.Expression.Shr shr: ShrToCIL(context, shr, ref stackSize); break;
                case Runic.AST.Node.Expression.Call call: CallToCIL(context, call, ref stackSize); break;
                case Runic.AST.Node.Expression.Cast cast: CastToCIL(context, cast, ref stackSize); break;
                case Runic.AST.Node.Expression.Sequence seq: SequenceToCIL(context, seq, ref stackSize); break;
                case Runic.AST.Node.Expression.Dereference deref: DereferenceToCIL(context, deref, ref stackSize); break;
                case Runic.AST.Node.Expression.VariableUse varUse: VariableUseToCIL(context, varUse, ref stackSize); break;
                case Runic.AST.Node.Expression.Indexing indexing: IndexingToCIL(context, indexing, ref stackSize); break;
                case Runic.AST.Node.Expression.VariableReference varref: VariableReferenceToCIL(context, varref, ref stackSize); break;
                case Runic.AST.Node.Expression.IndexingReference indexref: IndexingReferenceToCIL(context, indexref, ref stackSize); break;
                case Runic.AST.Node.Expression.Increment.Postfix.Variable varIncr: IncrementPostfixVariableToCIL(context, varIncr, ref stackSize); break;
                case Runic.AST.Node.Expression.Increment.Postfix.Dereference derefIncr: IncrementPostfixDerefToCIL(context, derefIncr, ref stackSize); break;
                case Runic.AST.Node.Expression.Comparison comparison: ComparisonToCIL(context, comparison, ref stackSize); break;
                case Runic.AST.Node.Branch branch: BranchToCIL(context, branch); break;
                case Runic.AST.Node.Switch @switch: SwitchToCIL(context, @switch, ref stackSize); break;
                case Runic.AST.Node.Label label: LabelToCIL(context, label); break;
                case Runic.AST.Node.If @if: IfToCIL(context, @if, ref stackSize); break;
                case Runic.AST.Node.Empty empty: break;
                default: throw new Exception("Unsupported node type: " + node.GetType().ToString());
            }
        }
        void EnsureReturn(Context context, Runic.AST.Node.Function function)
        {
            switch (function.ReturnType)
            {
                case Runic.AST.Type.Void @void: break;
                case Runic.AST.Type.Integer integer:
                    switch (integer.Bits)
                    {
                        case 8: context.Emitter.LdcI4(0); break;
                        case 16: context.Emitter.LdcI4(0); break;
                        case 32: context.Emitter.LdcI4(0); break;
                        case 64: context.Emitter.LdcI8(0); break;
                    }
                    break;
                case Runic.AST.Type.FloatingPoint floatingPoint:
                    switch (floatingPoint.Bits)
                    {
                        case 32: context.Emitter.LdcR4(0.0f); break;
                        case 64: context.Emitter.LdcR8(0.0); break;
                    }
                    break;
                case Runic.AST.Type.Pointer pointer:
                    context.Emitter.LdcI4(0);
                    context.Emitter.ConvI();
                    break;
                case Runic.AST.Type.StructOrUnion structType:
                    {
                        context.Emitter.LdcI4(0);
                        context.Emitter.ConvI();
                        context.Emitter.LdObj(false, 0, GetTypeToken(structType));
                    }
                    break;
            }
            context.Emitter.Ret();
        }
        public void ToCIL(Runic.AST.Node.Function function, Runic.CIL.Emitter emitter, out int maxStackSize, out byte[] localSignature)
        {
#if NET6_0_OR_GREATER
            Runic.AST.Node.Return? lastNodeAsReturn = null;
#else
            Runic.AST.Node.Return lastNodeAsReturn = null;
#endif
            var context = new Context(emitter);
            int stackSize = 0;
            for (int n = 0; n < function.Body.Length; n++)
            {
                Runic.AST.Node node = function.Body[n];
                if (!(node is Runic.AST.Node.Empty))
                {
                    NodeToCIL(context, function.Body[n], ref stackSize);
                    {
#if NET6_0_OR_GREATER
                        Runic.AST.Node.Expression? expression = node as Runic.AST.Node.Expression;
#else
                        Runic.AST.Node.Expression expression = node as Runic.AST.Node.Expression;
#endif
                        if (expression != null && !(expression.Type is Runic.AST.Type.Void))
                        {
                            stackSize -= 1;
                            emitter.Pop();
                        }
                        lastNodeAsReturn = node as Runic.AST.Node.Return;
                    }
                }
            }
            if (lastNodeAsReturn == null)
            {
                EnsureReturn(context, function);
            }
            context.Emitter.Flush();
            maxStackSize = context.MaxStackSize;
            localSignature = context.LocalSignature.ToArray();
        }
    }
}
