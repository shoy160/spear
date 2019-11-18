using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Spear.ProxyGenerator.Proxy
{
    internal class ProxyAssembly
    {
        private readonly AssemblyBuilder _ab;
        private readonly ModuleBuilder _mb;
        private int _typeId;

        // Maintain a MethodBase-->int, int-->MethodBase mapping to permit generated code
        // to pass methods by token
        private readonly Dictionary<MethodBase, int> _methodToToken = new Dictionary<MethodBase, int>();
        private readonly List<MethodBase> _methodsByToken = new List<MethodBase>();
        private readonly HashSet<string> _ignoresAccessAssemblyNames = new HashSet<string>();
        private ConstructorInfo _ignoresAccessChecksToAttributeConstructor;

        public ProxyAssembly()
        {
            const AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;
            var assemblyName = new AssemblyName("ProxyBuilder2") { Version = new Version(1, 0, 0) };
            _ab = AssemblyBuilder.DefineDynamicAssembly(assemblyName, access);
            _mb = _ab.DefineDynamicModule("testmod");
        }

        // Gets or creates the ConstructorInfo for the IgnoresAccessChecksAttribute.
        // This attribute is both defined and referenced in the dynamic assembly to
        // allow access to internal types in other assemblies.
        internal ConstructorInfo IgnoresAccessChecksAttributeConstructor
        {
            get
            {
                if (_ignoresAccessChecksToAttributeConstructor == null)
                {
                    TypeInfo attributeTypeInfo = GenerateTypeInfoOfIgnoresAccessChecksToAttribute();
                    _ignoresAccessChecksToAttributeConstructor = attributeTypeInfo.DeclaredConstructors.Single();
                }

                return _ignoresAccessChecksToAttributeConstructor;
            }
        }

        public ProxyBuilder CreateProxy(string name, Type proxyBaseType)
        {
            var nextId = Interlocked.Increment(ref _typeId);
            var tb = _mb.DefineType(name + "_" + nextId, TypeAttributes.Public, proxyBaseType);
            return new ProxyBuilder(this, tb, proxyBaseType);
        }

        // Generate the declaration for the IgnoresAccessChecksToAttribute type.
        // This attribute will be both defined and used in the dynamic assembly.
        // Each usage identifies the name of the assembly containing non-public
        // types the dynamic assembly needs to access.  Normally those types
        // would be inaccessible, but this attribute allows them to be visible.
        // It works like a reverse InternalsVisibleToAttribute.
        // This method returns the TypeInfo of the generated attribute.
        private TypeInfo GenerateTypeInfoOfIgnoresAccessChecksToAttribute()
        {
            var attributeTypeBuilder =
                _mb.DefineType("System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute",
                               TypeAttributes.Public | TypeAttributes.Class,
                               typeof(Attribute));

            // Create backing field as:
            // private string assemblyName;
            FieldBuilder assemblyNameField =
                attributeTypeBuilder.DefineField("assemblyName", typeof(String), FieldAttributes.Private);

            // Create ctor as:
            // public IgnoresAccessChecksToAttribute(string)
            ConstructorBuilder constructorBuilder = attributeTypeBuilder.DefineConstructor(MethodAttributes.Public,
                                                         CallingConventions.HasThis,
                                                         new Type[] { assemblyNameField.FieldType });

            ILGenerator il = constructorBuilder.GetILGenerator();

            // Create ctor body as:
            // this.assemblyName = {ctor parameter 0}
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg, 1);
            il.Emit(OpCodes.Stfld, assemblyNameField);

            // return
            il.Emit(OpCodes.Ret);

            // Define property as:
            // public string AssemblyName {get { return this.assemblyName; } }
            PropertyBuilder getterPropertyBuilder = attributeTypeBuilder.DefineProperty(
                                                   "AssemblyName",
                                                   PropertyAttributes.None,
                                                   CallingConventions.HasThis,
                                                   returnType: typeof(String),
                                                   parameterTypes: null);

            MethodBuilder getterMethodBuilder = attributeTypeBuilder.DefineMethod(
                                                   "get_AssemblyName",
                                                   MethodAttributes.Public,
                                                   CallingConventions.HasThis,
                                                   returnType: typeof(String),
                                                   parameterTypes: null);

            // Generate body:
            // return this.assemblyName;
            il = getterMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, assemblyNameField);
            il.Emit(OpCodes.Ret);

            // Generate the AttributeUsage attribute for this attribute type:
            // [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
            TypeInfo attributeUsageTypeInfo = typeof(AttributeUsageAttribute).GetTypeInfo();

            // Find the ctor that takes only AttributeTargets
            ConstructorInfo attributeUsageConstructorInfo =
                attributeUsageTypeInfo.DeclaredConstructors
                    .Single(c => c.GetParameters().Count() == 1 &&
                                 c.GetParameters()[0].ParameterType == typeof(AttributeTargets));

            // Find the property to set AllowMultiple
            PropertyInfo allowMultipleProperty =
                attributeUsageTypeInfo.DeclaredProperties
                    .Single(f => String.Equals(f.Name, "AllowMultiple"));

            // Create a builder to construct the instance via the ctor and property
            CustomAttributeBuilder customAttributeBuilder =
                new CustomAttributeBuilder(attributeUsageConstructorInfo,
                                            new object[] { AttributeTargets.Assembly },
                                            new PropertyInfo[] { allowMultipleProperty },
                                            new object[] { true });

            // Attach this attribute instance to the newly defined attribute type
            attributeTypeBuilder.SetCustomAttribute(customAttributeBuilder);

            // Make the TypeInfo real so the constructor can be used.
            return attributeTypeBuilder.CreateTypeInfo();
        }

        // Generates an instance of the IgnoresAccessChecksToAttribute to
        // identify the given assembly as one which contains internal types
        // the dynamic assembly will need to reference.
        internal void GenerateInstanceOfIgnoresAccessChecksToAttribute(string assemblyName)
        {
            // Add this assembly level attribute:
            // [assembly: System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute(assemblyName)]
            ConstructorInfo attributeConstructor = IgnoresAccessChecksAttributeConstructor;
            CustomAttributeBuilder customAttributeBuilder =
                new CustomAttributeBuilder(attributeConstructor, new object[] { assemblyName });
            _ab.SetCustomAttribute(customAttributeBuilder);
        }

        // Ensures the type we will reference from the dynamic assembly
        // is visible.  Non-public types need to emit an attribute that
        // allows access from the dynamic assembly.
        internal void EnsureTypeIsVisible(Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsVisible)
            {
                string assemblyName = typeInfo.Assembly.GetName().Name;
                if (!_ignoresAccessAssemblyNames.Contains(assemblyName))
                {
                    GenerateInstanceOfIgnoresAccessChecksToAttribute(assemblyName);
                    _ignoresAccessAssemblyNames.Add(assemblyName);
                }
            }
        }

        internal void GetTokenForMethod(MethodBase method, out Type type, out int token)
        {
            type = method.DeclaringType;
            token = 0;
            if (!_methodToToken.TryGetValue(method, out token))
            {
                _methodsByToken.Add(method);
                token = _methodsByToken.Count - 1;
                _methodToToken[method] = token;
            }
        }

        internal MethodBase ResolveMethodToken(Type type, int token)
        {
            Debug.Assert(token >= 0 && token < _methodsByToken.Count);
            return _methodsByToken[token];
        }
    }
}
