using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer.Commands
{
    /// <summary>
    /// The Get-Parameter cmdlet get parameters from methods.
    /// </summary>
    [OutputType(typeof(ParameterInfo))]
    [Cmdlet(VerbsCommon.Get, "Parameter")]
    public class GetParameterCommand : Cmdlet
    {
        /// <summary>
        /// Gets or sets the method to get parameters from.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public PSObject Method { get; set; } = null!;

        /// <summary>
        /// The ProcessRecord method.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Method == null)
            {
                return;
            }

            if (Method.BaseObject is MethodBase method)
            {
                WriteObject(method.GetParameters(), enumerateCollection: true);
                return;
            }

            if (Method.BaseObject is PropertyInfo property)
            {
                WriteObject(
                    property.GetGetMethod(true)?.GetParameters(),
                    enumerateCollection: true);
                return;
            }

            if (Method.BaseObject is EventInfo eventInfo)
            {
                WriteObject(
                    eventInfo.GetAddMethod(true)?.GetParameters(),
                    enumerateCollection: true);
            }
        }
    }
}
