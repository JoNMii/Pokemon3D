﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pokemon3D.Scripting.Types.Prototypes
{
    /// <summary>
    /// Represents an object from which other objects can be created.
    /// </summary>
    internal class Prototype : SProtoObject
    {
        internal string Name { get; }
        internal PrototypeMember Constructor { get; set; }
        internal Prototype Extends { get; private set; }
        /// <summary>
        /// The type this prototype instance was created from.
        /// </summary>
        internal Type MappedType { get; set; }

        internal bool IsAbstract
        {
            get
            {
                return MappedType?.IsAbstract ?? _isAbstract;
            }
            private set
            {
                if (MappedType == null)
                    _isAbstract = value;
            }
        }

        private readonly Dictionary<string, PrototypeMember> _prototypeMembers = new Dictionary<string, PrototypeMember>();
        private bool _initializedStatic;
        private SFunction _staticConstructor;
        private ScriptProcessor _staticConstructorProcessor;
        private bool _isAbstract = false;

        internal static bool IsPrototype(Type t)
        {
            return t == typeof(Prototype) || t.IsSubclassOf(typeof(Prototype));
        }

        internal Prototype(string name)
        {
            Name = name;

            var methods = BuiltInMethodManager.GetMethods(GetType());
            foreach (var methodData in methods)
            {
                var function = new SFunction(methodData.Delegate);
                var identifier = methodData.Name;

                function.FunctionUsage = methodData.Attribute.FunctionType;
                if (methodData.Attribute.FunctionType == FunctionUsageType.PropertyGetter)
                    identifier = PROPERTY_GET_PREFIX + identifier;
                else if (methodData.Attribute.FunctionType == FunctionUsageType.PropertySetter)
                    identifier = PROPERTY_SET_PREFIX + identifier;

                _prototypeMembers.Add(identifier, new PrototypeMember(
                        identifier: identifier,
                        data: function,
                        isStatic: methodData.Attribute.IsStatic,
                        isReadOnly: false,
                        isIndexerGet: methodData.Attribute.IsIndexerGet,
                        isIndexerSet: methodData.Attribute.IsIndexerSet
                    ));
            }
        }

        internal override SObject ExecuteMethod(ScriptProcessor processor, string methodName, SObject caller, SObject This, SObject[] parameters)
        {
            InitializeStatic();

            AddObjectPrototypeAsExtends(processor);

            bool isStaticCall = ReferenceEquals(caller, this);

            // Call any static function defined in this prototype:
            if (_prototypeMembers.ContainsKey(methodName) && _prototypeMembers[methodName].IsStatic)
            {
                if (_prototypeMembers[methodName].IsFunction)
                {
                    var cFunction = (SFunction)_prototypeMembers[methodName].Data;
                    return cFunction.Call(processor, caller, this, parameters); // For a static method, the "This" attribute is the prototype.
                }
                else
                {
                    return processor.ErrorHandler.ThrowError(ErrorType.TypeError, ErrorHandler.MESSAGE_TYPE_NOT_A_FUNCTION, methodName);
                }
            }

            // Call the super class prototype, if one exists:
            if (Extends != null)
            {
                return Extends.ExecuteMethod(processor, methodName, caller, This, parameters);
            }

            return processor.ErrorHandler.ThrowError(ErrorType.ReferenceError, ErrorHandler.MESSAGE_REFERENCE_NOT_DEFINED, methodName);
        }

        internal override bool HasMember(ScriptProcessor processor, string memberName)
        {
            AddObjectPrototypeAsExtends(processor);

            return _prototypeMembers.ContainsKey(memberName) || base.HasMember(processor, memberName);
        }

        internal override SObject GetMember(ScriptProcessor processor, SObject accessor, bool isIndexer)
        {
            InitializeStatic();

            AddObjectPrototypeAsExtends(processor);

            string memberName;
            if (accessor is SString)
                memberName = ((SString)accessor).Value;
            else
                memberName = accessor.ToString(processor).Value;

            if (_prototypeMembers.ContainsKey(PROPERTY_GET_PREFIX + memberName))
            {
                return ((SFunction)_prototypeMembers[PROPERTY_GET_PREFIX + memberName].Data).Call(processor, this, this, new SObject[] { });
            }
            else if (_prototypeMembers.ContainsKey(memberName))
            {
                return _prototypeMembers[memberName].Data;
            }
            else
            {
                return processor.Undefined;
            }
        }

        internal override void SetMember(ScriptProcessor processor, SObject accessor, bool isIndexer, SObject value)
        {
            InitializeStatic();

            AddObjectPrototypeAsExtends(processor);

            string memberName;
            if (accessor is SString)
                memberName = ((SString)accessor).Value;
            else
                memberName = accessor.ToString(processor).Value;

            if (_prototypeMembers.ContainsKey(memberName) && _prototypeMembers[memberName].IsStatic)
            {
                if (!_prototypeMembers[memberName].IsReadOnly)
                {
                    _prototypeMembers[memberName].Data = Unbox(value);
                }
            }
        }

        internal override string ToScriptSource()
        {
            return LITERAL_PROTOTYPE;
        }

        internal override string ToScriptObject()
        {
            return Name;
        }

        private void InitializeStatic()
        {
            // Runs the static constructor, if one exists.
            if (!_initializedStatic)
            {
                _initializedStatic = true;

                _staticConstructor?.Call(_staticConstructorProcessor, this, this, new SObject[] { });
            }
        }

        private void AddObjectPrototypeAsExtends(ScriptProcessor processor)
        {
            // If this prototype is not the object prototype, but has no extends prototype, set the object prototype.
            if (Extends == null && GetType() != typeof(ObjectPrototype))
                Extends = processor.Context.GetPrototype("Object");
        }

        /// <summary>
        /// Creates the base object for this <see cref="Prototype"/>'s instantiation method.
        /// </summary>
        protected virtual SProtoObject CreateBaseObject()
        {
            return new SProtoObject();
        }

        /// <summary>
        /// Creates an instance derived from this prototype.
        /// </summary>
        internal SProtoObject CreateInstance(ScriptProcessor processor, SObject[] parameters, bool executeCtor)
        {
            SProtoObject obj = CreateBaseObject();
            obj.IsProtoInstance = true;

            obj.AddMember(MEMBER_NAME_PROTOTYPE, this);

            if (typeof(ObjectPrototype) != GetType())
            {
                // If no extends class is explicitly specified, "Object" is assumed.
                if (Extends == null)
                    Extends = processor.Context.GetPrototype("Object");

                var superInstance = Extends.CreateInstance(processor, null, true);
                obj.AddMember(MEMBER_NAME_SUPER, superInstance);
            }

            foreach (var member in GetInstanceMembers())
            {
                obj.AddMember(member.Identifier, member.Data);
            }

            var indexerGetFunction = GetIndexerGetFunction();
            if (indexerGetFunction != null)
                obj.IndexerGetFunction = indexerGetFunction;

            var indexerSetFunction = GetIndexerSetFunction();
            if (indexerSetFunction != null)
                obj.IndexerSetFunction = indexerSetFunction;

            if (executeCtor)
            {
                Constructor?.ToFunction().Call(processor, obj, obj, parameters);
            }

            // Lock all readonly members after the constructor call, so they can be set in the constructor:
            foreach (PrototypeMember member in GetReadOnlyInstanceMembers())
                obj.Members[member.Identifier].IsReadOnly = true;

            return obj;
        }

        private IEnumerable<PrototypeMember> GetInstanceMembers()
        {
            return _prototypeMembers.Where(x => !x.Value.IsStatic && !x.Value.IsIndexerGet && !x.Value.IsIndexerSet).Select(x => x.Value);
        }

        private IEnumerable<PrototypeMember> GetReadOnlyInstanceMembers()
        {
            return _prototypeMembers.Where(x => !x.Value.IsStatic && x.Value.IsReadOnly && !x.Value.IsIndexerGet && !x.Value.IsIndexerSet).Select(x => x.Value);
        }

        internal SFunction GetIndexerGetFunction()
        {
            foreach (var member in _prototypeMembers.Values)
            {
                if (member.IsIndexerGet && member.IsFunction)
                {
                    return member.ToFunction();
                }
            }

            return IndexerGetFunction;
        }

        internal SFunction GetIndexerSetFunction()
        {
            foreach (var member in _prototypeMembers.Values)
            {
                if (member.IsIndexerSet && member.IsFunction)
                {
                    return member.ToFunction();
                }
            }

            return IndexerSetFunction;
        }

        /// <summary>
        /// Returns if the given member is static.
        /// </summary>
        internal bool IsStaticMember(string memberName)
        {
            return _prototypeMembers.ContainsKey(memberName) && _prototypeMembers[memberName].IsStatic;
        }

        /// <summary>
        /// Returns if the given member is readonly.
        /// </summary>
        internal bool IsReadOnlyMember(string memberName)
        {
            return _prototypeMembers.ContainsKey(memberName) && _prototypeMembers[memberName].IsReadOnly;
        }

        internal void AddMember(ScriptProcessor processor, PrototypeMember member)
        {
            if (_prototypeMembers.ContainsKey(member.Identifier))
                processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_DUPLICATE_DEFINITION, member.Identifier, Name);

            _prototypeMembers.Add(member.Identifier, member);
        }

        private const string REGEX_CLASS_SIGNATURE = @"^class(([ ]+abstract)|([ ]+extends[ ]+[a-zA-Z]\w*)|([ ]+[a-zA-Z]\w*))+[ ]*{.*}$";

        private const string CLASS_SIGNATURE_EXTENDS = "extends";
        private const string CLASS_SIGNATURE_ABSTRACT = "abstract";
        internal const string CLASS_METHOD_CTOR = "constructor";

        private const string FORMAT_VAR_ASSIGNMENT = "{0}={1};\n";

        internal new static SObject Parse(ScriptProcessor processor, string code)
        {
            code = code.Trim();

            if (Regex.IsMatch(code, REGEX_CLASS_SIGNATURE, RegexOptions.Singleline))
            {
                var signature = code.Remove(code.IndexOf("{")).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var extends = "";
                var identifier = "";
                var isAbstract = false;

                // Read extends:
                if (signature.Contains(CLASS_SIGNATURE_EXTENDS))
                {
                    var extendsIndex = signature.IndexOf(CLASS_SIGNATURE_EXTENDS);

                    if (extendsIndex + 1 == signature.Count) // when extends is the last element in the signature, throw error:
                        processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_EXTENDS_MISSING);

                    extends = signature[extendsIndex + 1]; // The extended class name is after the "extends" keyword.
                    signature.RemoveAt(extendsIndex); // Remove at the extends index twice, to remove the "extends" keyword and the identifier of the extended class.
                    signature.RemoveAt(extendsIndex);
                }

                // Read abstract:
                if (signature.Contains(CLASS_SIGNATURE_ABSTRACT))
                {
                    isAbstract = true;
                    signature.Remove(CLASS_SIGNATURE_ABSTRACT);
                }

                if (signature.Count != 2) // The signature must only have "class" and the identifier left.
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_INVALID_CLASS_SIGNATURE);

                // Read class name:
                identifier = signature[1];

                if (!ScriptProcessor.IsValidIdentifier(identifier))
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_IDENTIFIER_MISSING);

                // Create instance:
                var prototype = new Prototype(identifier)
                {
                    IsAbstract = isAbstract
                };

                // Handle extends:
                if (extends.Length > 0)
                {
                    if (isAbstract)
                        processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_TYPE_ABSTRACT_NO_EXTENDS);

                    var extendedPrototype = processor.Context.GetPrototype(extends);

                    if (extendedPrototype == null)
                        processor.ErrorHandler.ThrowError(ErrorType.ReferenceError, ErrorHandler.MESSAGE_REFERENCE_NO_PROTOTYPE, extends);

                    prototype.Extends = extendedPrototype;
                }
                else
                {
                    // Set default prototype:
                    prototype.Extends = processor.Context.GetPrototype("Object");
                }

                var body = code.Remove(0, code.IndexOf("{") + 1);
                body = body.Remove(body.Length - 1, 1).Trim();

                var additionalCtorCode = "";
                var staticCtorCode = "";

                var statements = StatementProcessor.GetStatements(processor, body);

                for (var i = 0; i < statements.Length; i++)
                {
                    var statement = statements[i];

                    if (statement.StatementType == StatementType.Var)
                    {
                        var parsed = ParseVarStatement(processor, statement);

                        if (parsed.Item1.Identifier == CLASS_METHOD_CTOR)
                            processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_MISSING_VAR_NAME);

                        prototype.AddMember(processor, parsed.Item1);

                        if (parsed.Item2.Length > 0)
                        {
                            if (parsed.Item1.IsStatic)
                            {
                                staticCtorCode += string.Format(FORMAT_VAR_ASSIGNMENT, parsed.Item1.Identifier, parsed.Item2);
                            }
                            else
                            {
                                additionalCtorCode += string.Format(FORMAT_VAR_ASSIGNMENT, parsed.Item1.Identifier, parsed.Item2);
                            }
                        }
                    }
                    else if (statement.StatementType == StatementType.Function)
                    {
                        i++;

                        if (statements.Length > i)
                        {
                            var bodyStatement = statements[i];
                            var parsed = ParseFunctionStatement(processor, statement, bodyStatement);

                            if (parsed.Identifier == CLASS_METHOD_CTOR)
                            {
                                if (prototype.Constructor != null)
                                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_DUPLICATE_DEFINITION, parsed.Identifier, identifier);

                                prototype.Constructor = parsed;
                            }
                            else
                            {
                                prototype.AddMember(processor, parsed);
                            }
                        }
                        else
                        {
                            return processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_EXPECTED_EXPRESSION, "end of script");
                        }
                    }
                    else
                    {
                        processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_INVALID_STATEMENT);
                    }
                }

                // Add additional constructor code & static constructor code to prototype:

                if (staticCtorCode.Length > 0)
                {
                    prototype._staticConstructor = new SFunction(staticCtorCode, new string[] { });
                    prototype._staticConstructorProcessor = processor;
                }

                if (additionalCtorCode.Length > 0)
                {
                    if (prototype.Constructor == null)
                    {
                        // Create new ctor if no one has been defined:
                        prototype.Constructor = new PrototypeMember(CLASS_METHOD_CTOR, new SFunction("", new string[] { }), false, true, false, false);
                    }

                    prototype.Constructor.ToFunction().Body = additionalCtorCode + prototype.Constructor.ToFunction().Body;
                }

                return prototype;
            }
            else
            {
                return processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_INVALID_CLASS_SIGNATURE);
            }
        }

        private const string VAR_SIGNATURE_STATIC = "static";
        private const string VAR_SIGNATURE_READONLY = "readonly";

        // TODO: C# 7: put proper Tuple handling in place.

        private static Tuple<PrototypeMember, string> ParseVarStatement(ScriptProcessor processor, ScriptStatement statement)
        {
            var code = statement.Code;
            var signature = code.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            string identifier;
            var assignment = "";
            var isReadOnly = false;
            var isStatic = false;

            // Read static:
            if (signature.Contains(VAR_SIGNATURE_STATIC))
            {
                isStatic = true;
                signature.Remove(VAR_SIGNATURE_STATIC);
            }

            // Read readonly:
            if (signature.Contains(VAR_SIGNATURE_READONLY))
            {
                isReadOnly = true;
                signature.Remove(VAR_SIGNATURE_READONLY);
            }

            if (signature[0] != "var" || signature.Count < 2)
                processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_INVALID_VAR_DECLARATION);

            identifier = signature[1];

            if (!ScriptProcessor.IsValidIdentifier(identifier))
                processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_MISSING_VAR_NAME);

            if (signature.Count > 2)
            {
                if (signature[2].StartsWith("="))
                {
                    assignment = code.Remove(0, code.IndexOf("=") + 1).Trim();
                }
                else
                {
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_INVALID_VAR_DECLARATION);
                }
            }

            var member = new PrototypeMember(identifier, processor.Undefined, isStatic, isReadOnly, false, false);

            return new Tuple<PrototypeMember, string>(member, assignment);
        }

        private const string FUNCTION_SIGNATURE_STATIC = "static";
        private const string FUNCTION_SIGNATURE_INDEXER = "indexer";
        private const string FUNCTION_SIGNATURE_GET = "get";
        private const string FUNCTION_SIGNATURE_SET = "set";
        private const string FUNCTION_SIGNATURE_PROPERTY = "property";

        private static PrototypeMember ParseFunctionStatement(ScriptProcessor processor, ScriptStatement headerStatement, ScriptStatement bodyStatement)
        {
            var header = headerStatement.Code;
            var signature = header.Remove(header.IndexOf("(")).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            string identifier;
            var isStatic = false;
            var isIndexerGet = false;
            var isIndexerSet = false;
            var functionType = FunctionUsageType.Default;

            var significantCount = 0;

            // Read static:
            if (signature.Contains(FUNCTION_SIGNATURE_STATIC))
            {
                isStatic = true;
                signature.Remove(FUNCTION_SIGNATURE_STATIC);
            }

            // Read indexer:
            if (signature.Contains(FUNCTION_SIGNATURE_INDEXER))
            {
                significantCount++;

                var indexerIndex = signature.IndexOf(FUNCTION_SIGNATURE_INDEXER);

                if (indexerIndex + 1 == signature.Count)
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_FUNCTION_INDEXER_EXPECTED_TYPE);

                var indexerType = signature[indexerIndex + 1];
                if (indexerType == FUNCTION_SIGNATURE_GET)
                {
                    isIndexerGet = true;
                    signature.RemoveAt(indexerIndex + 1);
                }
                else if (indexerType == FUNCTION_SIGNATURE_SET)
                {
                    isIndexerSet = true;
                    signature.RemoveAt(indexerIndex + 1);
                }
                else
                {
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_FUNCTION_INDEXER_INVALID_TYPE, indexerType);
                }

                signature.Remove(FUNCTION_SIGNATURE_INDEXER);
            }
            // Read property:
            if (signature.Contains(FUNCTION_SIGNATURE_PROPERTY))
            {
                significantCount++;

                var propertyIndex = signature.IndexOf(FUNCTION_SIGNATURE_PROPERTY);

                if (propertyIndex + 1 == signature.Count)
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_FUNCTION_PROPERTY_EXPECTED_TYPE);

                var propertyType = signature[propertyIndex + 1];
                if (propertyType == FUNCTION_SIGNATURE_GET)
                {
                    functionType = FunctionUsageType.PropertyGetter;
                    signature.RemoveAt(propertyIndex + 1);
                }
                else if (propertyType == FUNCTION_SIGNATURE_SET)
                {
                    functionType = FunctionUsageType.PropertySetter;
                    signature.RemoveAt(propertyIndex + 1);
                }
                else
                {
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_FUNCTION_PROPERTY_INVALID_TYPE, propertyType);
                }

                signature.Remove(FUNCTION_SIGNATURE_PROPERTY);
            }

            // Only one (or none) significant signature types can be added to a signature.
            if (significantCount > 1)
                processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_INCOMPATIBLE_SIGNATURE);

            if (signature.Count != 2)
                processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_CLASS_INVALID_FUNCTION_SIGNATURE);

            identifier = signature[1];

            if (!ScriptProcessor.IsValidIdentifier(identifier))
                processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_MISSING_VAR_NAME);

            // After the valid identifier check is done, we add the getter/setter prefix. It wouldn't pass the test.
            if (functionType == FunctionUsageType.PropertyGetter)
                identifier = PROPERTY_GET_PREFIX + identifier;
            if (functionType == FunctionUsageType.PropertySetter)
                identifier = PROPERTY_SET_PREFIX + identifier;

            var function = new SFunction(processor, signature[0] + " " + header.Remove(0, header.IndexOf("(")) + bodyStatement.Code)
            {
                FunctionUsage = functionType
            };

            return new PrototypeMember(identifier, function, isStatic, false, isIndexerGet, isIndexerSet);
        }
    }
}
