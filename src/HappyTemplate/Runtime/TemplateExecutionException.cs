/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using HappyTemplate.Compiler;

namespace HappyTemplate.Runtime
{
    [global::System.Serializable]
    public class TemplateExecutionException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TemplateExecutionException() { }
        public TemplateExecutionException(string message) : base(message) { }
        public TemplateExecutionException(string message, Exception inner) : base(message, inner) { }
        protected TemplateExecutionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    //enum TemplateExecutionErrorCode
    //{
    //    DoesNotImplementIEnumerableInForEach,
    //    IdentifierNotFound_Identifier,
    //    ObjectDoesNotExist_Identifier,
    //    IncorrectNumberOfArguments_ObjectName_Expected_Actual,
    //    IncorrectNumberOfArguments_ObjectName_MinExpected_MaxExpected_Actual,
    //    MultipartIdentifierNotFound_Identifier_PartNotFound,
    //    InputValueDoesNotExistInLookup_LookupName_InputValue,
    //    ExceptionWhileRetrievingPropertyValue_Identifier_PartInError_ExceptionClass_Message_StackTrace,
    //    TemplateNamesCannotBeMutlipart,
    //    Internal_Message,
    //    MaximumStackSizeExceeded,
    //    BuiltInFunctionException_FunctionName,
    //    ArgumentNull_ArgumentNumber_FunctionName
    //}

    //public class TemplateExecutionException : SourceException
    //{
    //    private TemplateExecutionException(SourceLocation loc, Enum key, params object[] args)
    //        : base(loc, key, Resources.ExecutionErrorMessages.ResourceManager.GetString(key.ToString()), args)
    //    {

    //    }

    //    private TemplateExecutionException(Exception inner, SourceLocation loc, Enum key, params object[] args)
    //        : base(inner, loc, key, Resources.ExecutionErrorMessages.ResourceManager.GetString(key.ToString()), args)
    //    {

    //    }

    //    internal static void ThrowDoesNotImplementIEnumerable(SourceLocation loc)
    //    {
    //        throw new TemplateExecutionException(loc, TemplateExecutionErrorCode.DoesNotImplementIEnumerableInForEach);
    //    }

    //    internal static void ThrowIdentifierNotFound(Identifier ident)
    //    {
    //        throw new TemplateExecutionException(ident.Span, TemplateExecutionErrorCode.IdentifierNotFound_Identifier, ident.Text);
    //    }


    //    internal static void ThrowTemplateDoesNotExist(Identifier identifier)
    //    {
    //        throw new TemplateExecutionException(identifier.Span,
    //            TemplateExecutionErrorCode.ObjectDoesNotExist_Identifier, identifier.Text);
    //    }

    //    internal static void ThrowIncorrectNumberOfArguments(Identifier identifier, int expected, int actual)
    //    {
    //        throw new TemplateExecutionException(identifier.Span,
    //            TemplateExecutionErrorCode.IncorrectNumberOfArguments_ObjectName_Expected_Actual,
    //            identifier, expected, actual);
    //    }

    //    internal static void ThrowIncorrectNumberOfArguments(
    //        Identifier identifier, 
    //        int minExpected, 
    //        int maxExpected, 
    //        int actual)
    //    {
    //        if(minExpected == maxExpected)
    //            throw new TemplateExecutionException(identifier.Span,
    //                TemplateExecutionErrorCode.IncorrectNumberOfArguments_ObjectName_Expected_Actual,
    //                identifier, minExpected, actual);

    //        throw new TemplateExecutionException(identifier.Span,
    //            TemplateExecutionErrorCode.IncorrectNumberOfArguments_ObjectName_MinExpected_MaxExpected_Actual,
    //            identifier, minExpected, maxExpected, actual);
    //    }

    //    internal static void ThrowMultipartIdentiferNotFound(Identifier ident, string partNotFound)
    //    {
    //        throw new TemplateExecutionException(ident.Span,
    //            TemplateExecutionErrorCode.MultipartIdentifierNotFound_Identifier_PartNotFound,
    //            ident.Text, partNotFound);
    //    }

    //    internal static void ThrowInputValueDoesNotExistInLookup(Identifier lookupIdentifier, string inputValue)
    //    {
    //        throw new TemplateExecutionException(lookupIdentifier.Span,
    //            TemplateExecutionErrorCode.InputValueDoesNotExistInLookup_LookupName_InputValue, lookupIdentifier.Text, inputValue);
    //    }

    //    internal static void ThrowExceptionWhileRetrievingPropertyValue(Identifier identifier, string partInError, Exception ex)
    //    {
    //        throw new TemplateExecutionException(identifier.Span,
    //            TemplateExecutionErrorCode.ExceptionWhileRetrievingPropertyValue_Identifier_PartInError_ExceptionClass_Message_StackTrace,
    //            identifier.Text, partInError, ex.GetType().Name, ex.Message, ex.StackTrace);
    //    }

    //    internal static void ThrowTemplateNamesCannotBeMutlipart(SourceLocation location)
    //    {
    //        throw new TemplateExecutionException(location, TemplateExecutionErrorCode.TemplateNamesCannotBeMutlipart);
    //    }

    //    internal static void ThrowInternal(SourceLocation at, string message)
    //    {
    //        throw new TemplateExecutionException(at, TemplateExecutionErrorCode.Internal_Message, message);
    //    }

    //    internal static void ThrowMaximumStackSizeExceeded(SourceLocation at)
    //    {
    //        throw new TemplateExecutionException(at, TemplateExecutionErrorCode.MaximumStackSizeExceeded);
    //    }

    //    public static Exception NewBuiltinFuncitonException(SourceLocation at, string functionName, Exception inner)
    //    {
    //        return new TemplateExecutionException(inner, at, 
    //            TemplateExecutionErrorCode.BuiltInFunctionException_FunctionName, functionName);
    //    }

    //    public static void ThrowArgumentNull(SourceLocation at, int argIndex, string functionName)
    //    {
    //        throw new TemplateExecutionException(at, TemplateExecutionErrorCode.ArgumentNull_ArgumentNumber_FunctionName, 
    //            argIndex + 1, functionName);
    //    }
    //}
}

