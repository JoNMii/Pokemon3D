﻿using Pokemon3D.Scripting.Types.Prototypes;
using System.Collections.Generic;
using System.Text;

namespace Pokemon3D.Scripting.Types
{
    /// <summary>
    /// An object that is an instance of a <see cref="Prototypes.Prototype"/>.
    /// </summary>
    public class SProtoObject : SObject
    {
        internal const string PROPERTY_GET_PREFIX = "_get_";
        internal const string PROPERTY_SET_PREFIX = "_set_";
        protected internal const string MEMBER_NAME_PROTOTYPE = "prototype";
        protected internal const string MEMBER_NAME_SUPER = "super";

        /// <summary>
        /// Determines if this object actually was instantiated through a prototype.
        /// </summary>
        internal bool IsProtoInstance { get; set; }

        internal Dictionary<string, SVariable> Members { get; } = new Dictionary<string, SVariable>();

        internal SFunction IndexerGetFunction { get; set; }
        internal SFunction IndexerSetFunction { get; set; }

        // keeps references to objects that should be copied back when translated between script and .net object.
        internal Dictionary<string, object> ReferenceContainer { get; set; }

        internal void AddMember(SVariable member)
        {
            Members.Add(member.Identifier, member);
        }

        internal void AddMember(string idenfifier, SObject data)
        {
            Members.Add(idenfifier, new SVariable(idenfifier, data));
        }

        internal void SetMember(string identifier, SObject data)
        {
            Members[identifier].Data = data;
        }

        internal SProtoObject SuperClass
        {
            get
            {
                if (Members.ContainsKey(MEMBER_NAME_SUPER))
                {
                    return (SProtoObject)Members[MEMBER_NAME_SUPER].Data;
                }
                return null;
            }
        }

        internal Prototype Prototype
        {
            get
            {
                if (Members.ContainsKey(MEMBER_NAME_PROTOTYPE))
                {
                    return (Prototype)Members[MEMBER_NAME_PROTOTYPE].Data;
                }
                return null;
            }
        }

        internal override bool HasMember(ScriptProcessor processor, string memberName)
        {
            if (Members.ContainsKey(memberName))
            {
                return true;
            }
            else if (Prototype != null && Prototype.HasMember(processor, memberName))
            {
                return true;
            }
            else if (SuperClass != null)
            {
                return SuperClass.HasMember(processor, memberName);
            }
            return false;
        }

        internal override SObject GetMember(ScriptProcessor processor, SObject accessor, bool isIndexer)
        {
            if (isIndexer && accessor.TypeOf() == LITERAL_TYPE_NUMBER)
            {
                if (Prototype.GetIndexerGetFunction() != IndexerGetFunction)
                    IndexerGetFunction = Prototype.GetIndexerGetFunction();

                if (IndexerGetFunction != null)
                {
                    return IndexerGetFunction.Call(processor, this, this, new SObject[] { accessor });
                }
                else
                {
                    return processor.Undefined;
                }
            }

            string memberName;
            if (accessor is SString)
                memberName = ((SString)accessor).Value;
            else
                memberName = accessor.ToString(processor).Value;

            if (Members.ContainsKey(PROPERTY_GET_PREFIX + memberName)) // getter property
            {
                return ((SFunction)Members[PROPERTY_GET_PREFIX + memberName].Data).Call(processor, this, this, new SObject[] { });
            }
            else if (Members.ContainsKey(memberName))
            {
                return Members[memberName];
            }
            else if (Prototype != null && Prototype.HasMember(processor, memberName))
            {
                return Prototype.GetMember(processor, accessor, isIndexer);
            }
            else if (SuperClass != null)
            {
                return SuperClass.GetMember(processor, accessor, isIndexer);
            }

            return processor.Undefined;
        }

        internal override void SetMember(ScriptProcessor processor, SObject accessor, bool isIndexer, SObject value)
        {
            if (isIndexer)
            {
                if (Prototype.GetIndexerSetFunction() != IndexerSetFunction)
                    IndexerSetFunction = Prototype.GetIndexerSetFunction();
            }

            if (isIndexer && accessor.TypeOf() == LITERAL_TYPE_NUMBER && IndexerSetFunction != null)
            {
                IndexerSetFunction.Call(processor, this, this, new SObject[] { accessor, value });
            }
            else
            {
                string memberName;
                if (accessor is SString)
                    memberName = ((SString)accessor).Value;
                else
                    memberName = accessor.ToString(processor).Value;

                if (Members.ContainsKey(PROPERTY_SET_PREFIX + memberName)) // setter property
                {
                    ((SFunction)Members[PROPERTY_SET_PREFIX + memberName].Data).Call(processor, this, this, new SObject[] { value });
                }
                if (Members.ContainsKey(memberName))
                {
                    Members[memberName].Data = value;
                }
                else if (Prototype != null && Prototype.HasMember(processor, memberName) && !Prototype.IsStaticMember(memberName))
                {
                    // This is the case when new members got added to the prototype, and we haven't copied them over to the instance yet.
                    // So we do that now, and then set the value of that member:
                    AddMember(memberName, value);
                }
                else
                {
                    SuperClass?.SetMember(processor, accessor, isIndexer, value);
                }
            }
        }

        internal override SObject ExecuteMethod(ScriptProcessor processor, string methodName, SObject caller, SObject This, SObject[] parameters)
        {
            if (Members.ContainsKey(methodName))
            {
                if (Members[methodName].Data.TypeOf() == LITERAL_TYPE_FUNCTION)
                {
                    return ((SFunction)Members[methodName].Data).Call(processor, caller, this, parameters);
                }
                else
                {
                    return processor.ErrorHandler.ThrowError(ErrorType.TypeError, ErrorHandler.MESSAGE_TYPE_NOT_A_FUNCTION, methodName);
                }
            }
            else if (Prototype != null && Prototype.HasMember(processor, methodName))
            {
                return Prototype.ExecuteMethod(processor, methodName, caller, This, parameters);
            }
            else if (SuperClass != null)
            {
                return SuperClass.ExecuteMethod(processor, methodName, caller, This, parameters);
            }
            else
            {
                return processor.ErrorHandler.ThrowError(ErrorType.TypeError, ErrorHandler.MESSAGE_TYPE_NOT_A_FUNCTION, methodName);
            }
        }

        internal override string ToScriptObject()
        {
            return ObjectBuffer.OBJ_PREFIX + ObjectBuffer.GetObjectId(this).ToString();
        }

        private const string FORMAT_SOURCE_MEMBER = "{0}:{1}";

        internal override string ToScriptSource()
        {
            var source = new StringBuilder();

            foreach (var member in Members)
            {
                if (member.Key != MEMBER_NAME_PROTOTYPE && member.Key != MEMBER_NAME_SUPER) // Do not include super class or prototype in the string representation
                {
                    if (source.Length > 0)
                        source.Append(",");

                    var data = Unbox(member.Value);

                    if (ReferenceEquals(data, this))
                    {
                        // Avoid infinite recursion:
                        source.Append(string.Format(FORMAT_SOURCE_MEMBER, member.Key, "{}"));
                    }
                    else
                    {
                        source.Append(string.Format(FORMAT_SOURCE_MEMBER, member.Key, data.ToScriptSource()));
                    }
                }
            }

            source.Insert(0, "{");
            source.Append("}");

            return source.ToString();
        }

        internal override double SizeOf()
        {
            return Members.Count;
        }

        internal override SString ToString(ScriptProcessor processor)
        {
            return processor.CreateString(LITERAL_OBJECT_STR);
        }

        /// <summary>
        /// Parses an anonymous object.
        /// </summary>
        internal static SProtoObject Parse(ScriptProcessor processor, string source)
        {
            var prototype = new Prototype(LITERAL_OBJECT);

            source = source.Trim();

            // Format: { member1 : content1, member2 : content2 }

            // Remove "{" and "}":
            source = source.Remove(source.Length - 1, 1).Remove(0, 1).Trim();

            if (source.Length == 0)
                return prototype.CreateInstance(processor, null, false);

            var index = 0;
            var identifier = "";
            var content = "";
            SObject contentObj = null;

            while (index < source.Length)
            {
                var nextSeperatorIndex = source.IndexOf(",", index);
                if (nextSeperatorIndex == -1)
                {
                    nextSeperatorIndex = source.Length;
                }
                else
                {
                    //Let's find the correct ",":

                    nextSeperatorIndex = index;

                    var depth = 0;
                    StringEscapeHelper escaper = new LeftToRightStringEscapeHelper(source, nextSeperatorIndex, true);
                    var foundSeperator = false;

                    while (!foundSeperator && nextSeperatorIndex < source.Length)
                    {
                        var t = source[nextSeperatorIndex];

                        escaper.CheckStartAt(nextSeperatorIndex);

                        if (!escaper.IsString)
                        {
                            if (t == '{' || t == '[' || t == '(')
                            {
                                depth++;
                            }
                            else if (t == '}' || t == ']' || t == ')')
                            {
                                depth--;
                            }
                            else if (t == ',' && depth == 0)
                            {
                                foundSeperator = true;
                                nextSeperatorIndex--; // it adds one to the index at the end of the while loop, but we need the index where we stopped searching, so we subtract 1 here to accommodate for that.
                            }
                        }

                        nextSeperatorIndex++;
                    }
                }

                var member = source.Substring(index, nextSeperatorIndex - index);

                if (member.Contains(":"))
                {
                    identifier = member.Remove(member.IndexOf(":")).Trim();
                    content = member.Remove(0, member.IndexOf(":") + 1);

                    contentObj = processor.ExecuteStatement(new ScriptStatement(content));
                }
                else
                {
                    identifier = member.Trim();
                    contentObj = processor.Undefined;
                }

                if (!ScriptProcessor.IsValidIdentifier(identifier))
                    processor.ErrorHandler.ThrowError(ErrorType.SyntaxError, ErrorHandler.MESSAGE_SYNTAX_MISSING_VAR_NAME);

                prototype.AddMember(processor, new PrototypeMember(identifier, contentObj));

                index = nextSeperatorIndex + 1;
            }

            return prototype.CreateInstance(processor, null, false);
        }
    }
}
