namespace ExceptionAnalyzer;

/// <summary>
///     Adds all exceptions thrown by this method to Exceptions partial class.
///     <br />
///     <br />
///     Use the following partial class to access all analyzed exceptions
///     
///     <code>
///     public partial class Exceptions
///     {
///         public static Exceptions Current => new();
///
///         private MethodExceptionInfo[] _methods;
///
///         public Exceptions()
///         {
///             SetThrownExceptions();
///         }
///
///         partial void SetThrownExceptions();
///     }
///     </code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AddExceptionsAttribute : Attribute
{

}
