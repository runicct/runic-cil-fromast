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
using System.Collections.Generic;
using System.IO;
using static Runic.AST.Node.Expression;
using static Runic.AST.Node.Expression.Constant;

namespace Runic.CIL
{
    public abstract partial class FromAST
    {
        internal class Signature
        {
            List<byte> _localSignature = new List<byte>();
            public void WriteCompressedInteger(List<byte> result, uint integer)
            {
                if (integer <= 127) { result.Add((byte)integer); return; }
                if (integer <= 0x3FFF)
                {
                    result.Add((byte)((integer >> 8) | 0x80));
                    result.Add((byte)((integer & 0xFF)));
                    return;
                }
                if (integer > 0x1FFFFFFF) { throw new System.ArgumentOutOfRangeException(); }

                result.Add((byte)((integer >> 24) | 0xFF));
                result.Add((byte)((integer >> 16) | 0xFF));
                result.Add((byte)((integer >> 8) | 0xFF));
                result.Add((byte)((integer) | 0xFF));
            }
            int _localCount = 0;
            public void EmitLocal(Runic.AST.Type type)
            {
                _localCount++;
                EncodeLocal(type);
            }
            void EncodeLocal(Runic.AST.Type type)
            {
                switch (type)
                {
                    case Runic.AST.Type.Boolean _: _localSignature.Add((byte)0x02); break;
                    case Runic.AST.Type.Integer integer:
                        switch (integer.Bits)
                        {
                            case 8: if (integer.Signed) { _localSignature.Add((byte)0x04); } else { _localSignature.Add((byte)0x05); } break;
                            case 16: if (integer.Signed) { _localSignature.Add((byte)0x06); } else { _localSignature.Add((byte)0x07); } break;
                            case 32: if (integer.Signed) { _localSignature.Add((byte)0x08); } else { _localSignature.Add((byte)0x09); } break;
                            case 64: if (integer.Signed) { _localSignature.Add((byte)0x0A); } else { _localSignature.Add((byte)0x0B); } break;
                        }
                        break;
                    case Runic.AST.Type.FloatingPoint flt:
                        switch (flt.Bits)
                        {
                            case 32: _localSignature.Add((byte)0x0C); break;
                            case 64: _localSignature.Add((byte)0x0D); break;
                        }
                        break;
                    case Runic.AST.Type.Pointer ptr:
                        _localSignature.Add((byte)0x0F);
                        EncodeLocal(ptr.TargetType);
                        break;
                }
            }
            public byte[] ToArray()
            {
                List<byte> result = new List<byte>();
                result.Add(0x07); // Local var signature
                WriteCompressedInteger(result, (uint)_localCount);
                result.AddRange(_localSignature);
                return result.ToArray();
            }
        }
    }
}
