using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Spear.ProxyGenerator.Proxy
{
    internal enum OpCodeType
    {
        Conv,
        Ldind,
        Stind
    }

    internal static class ProxyCodes
    {
        public static void Ldind(ILGenerator il, Type type)
        {
            var opCode = GetOpCode(type, OpCodeType.Ldind);
            if (!opCode.Equals(OpCodes.Nop))
            {
                il.Emit(opCode);
            }
            else
            {
                il.Emit(OpCodes.Ldobj, type);
            }
        }

        public static void Stind(ILGenerator il, Type type)
        {
            var opCode = GetOpCode(type, OpCodeType.Stind);
            if (!opCode.Equals(OpCodes.Nop))
            {
                il.Emit(opCode);
            }
            else
            {
                il.Emit(OpCodes.Stobj, type);
            }
        }

        public static void Convert(ILGenerator il, Type source, Type target, bool isAddress)
        {
            Debug.Assert(!target.IsByRef);
            if (target == source)
                return;

            var sourceTypeInfo = source.GetTypeInfo();
            var targetTypeInfo = target.GetTypeInfo();

            if (source.IsByRef)
            {
                Debug.Assert(!isAddress);
                var argType = source.GetElementType();
                Ldind(il, argType);
                Convert(il, argType, target, false);
                return;
            }

            if (targetTypeInfo.IsValueType)
            {
                if (sourceTypeInfo.IsValueType)
                {
                    var opCode = GetOpCode(target, OpCodeType.Conv);
                    Debug.Assert(!opCode.Equals(OpCodes.Nop));
                    il.Emit(opCode);
                }
                else
                {
                    Debug.Assert(sourceTypeInfo.IsAssignableFrom(targetTypeInfo));
                    il.Emit(OpCodes.Unbox, target);
                    if (!isAddress)
                        Ldind(il, target);
                }
            }
            else if (targetTypeInfo.IsAssignableFrom(sourceTypeInfo))
            {
                if (sourceTypeInfo.IsValueType || source.IsGenericParameter)
                {
                    if (isAddress)
                        Ldind(il, source);
                    il.Emit(OpCodes.Box, source);
                }
            }
            else
            {
                Debug.Assert(sourceTypeInfo.IsAssignableFrom(targetTypeInfo) || targetTypeInfo.IsInterface ||
                             sourceTypeInfo.IsInterface);
                il.Emit(target.IsGenericParameter ? OpCodes.Unbox_Any : OpCodes.Castclass, target);
            }
        }

        private static OpCode GetOpCode(Type type, OpCodeType codeType)
        {
            var code = GetTypeCode(type);
            switch (codeType)
            {
                case OpCodeType.Conv:
                    return SConvOpCodes[code];
                case OpCodeType.Ldind:
                    return SLdindOpCodes[code];
                case OpCodeType.Stind:
                    return SStindOpCodes[code];
            }

            return OpCodes.Nop;
        }

        private static int GetTypeCode(Type type)
        {
            if (type == null)
                return 0;   // TypeCode.Empty;

            if (type == typeof(Boolean))
                return 3;   // TypeCode.Boolean;

            if (type == typeof(Char))
                return 4;   // TypeCode.Char;

            if (type == typeof(SByte))
                return 5;   // TypeCode.SByte;

            if (type == typeof(Byte))
                return 6;   // TypeCode.Byte;

            if (type == typeof(Int16))
                return 7;   // TypeCode.Int16;

            if (type == typeof(UInt16))
                return 8;   // TypeCode.UInt16;

            if (type == typeof(Int32))
                return 9;   // TypeCode.Int32;

            if (type == typeof(UInt32))
                return 10;  // TypeCode.UInt32;

            if (type == typeof(Int64))
                return 11;  // TypeCode.Int64;

            if (type == typeof(UInt64))
                return 12;  // TypeCode.UInt64;

            if (type == typeof(Single))
                return 13;  // TypeCode.Single;

            if (type == typeof(Double))
                return 14;  // TypeCode.Double;

            if (type == typeof(Decimal))
                return 15;  // TypeCode.Decimal;

            if (type == typeof(DateTime))
                return 16;  // TypeCode.DateTime;

            if (type == typeof(String))
                return 18;  // TypeCode.String;

            if (type.GetTypeInfo().IsEnum)
                return GetTypeCode(Enum.GetUnderlyingType(type));
            return 1;   // TypeCode.Object;
        }

        private static readonly OpCode[] SConvOpCodes = {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Conv_I1,//Boolean = 3,
                OpCodes.Conv_I2,//Char = 4,
                OpCodes.Conv_I1,//SByte = 5,
                OpCodes.Conv_U1,//Byte = 6,
                OpCodes.Conv_I2,//Int16 = 7,
                OpCodes.Conv_U2,//UInt16 = 8,
                OpCodes.Conv_I4,//Int32 = 9,
                OpCodes.Conv_U4,//UInt32 = 10,
                OpCodes.Conv_I8,//Int64 = 11,
                OpCodes.Conv_U8,//UInt64 = 12,
                OpCodes.Conv_R4,//Single = 13,
                OpCodes.Conv_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Nop,//String = 18,
            };

        private static readonly OpCode[] SLdindOpCodes = {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Ldind_I1,//Boolean = 3,
                OpCodes.Ldind_I2,//Char = 4,
                OpCodes.Ldind_I1,//SByte = 5,
                OpCodes.Ldind_U1,//Byte = 6,
                OpCodes.Ldind_I2,//Int16 = 7,
                OpCodes.Ldind_U2,//UInt16 = 8,
                OpCodes.Ldind_I4,//Int32 = 9,
                OpCodes.Ldind_U4,//UInt32 = 10,
                OpCodes.Ldind_I8,//Int64 = 11,
                OpCodes.Ldind_I8,//UInt64 = 12,
                OpCodes.Ldind_R4,//Single = 13,
                OpCodes.Ldind_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Ldind_Ref,//String = 18,
            };

        private static readonly OpCode[] SStindOpCodes = {
                OpCodes.Nop,//Empty = 0,
                OpCodes.Nop,//Object = 1,
                OpCodes.Nop,//DBNull = 2,
                OpCodes.Stind_I1,//Boolean = 3,
                OpCodes.Stind_I2,//Char = 4,
                OpCodes.Stind_I1,//SByte = 5,
                OpCodes.Stind_I1,//Byte = 6,
                OpCodes.Stind_I2,//Int16 = 7,
                OpCodes.Stind_I2,//UInt16 = 8,
                OpCodes.Stind_I4,//Int32 = 9,
                OpCodes.Stind_I4,//UInt32 = 10,
                OpCodes.Stind_I8,//Int64 = 11,
                OpCodes.Stind_I8,//UInt64 = 12,
                OpCodes.Stind_R4,//Single = 13,
                OpCodes.Stind_R8,//Double = 14,
                OpCodes.Nop,//Decimal = 15,
                OpCodes.Nop,//DateTime = 16,
                OpCodes.Nop,//17
                OpCodes.Stind_Ref,//String = 18,
            };
    }
}
