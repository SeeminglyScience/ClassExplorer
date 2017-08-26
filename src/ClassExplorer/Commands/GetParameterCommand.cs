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
        public MethodBase Method { get; set; }

        /// <summary>
        /// The ProcessRecord method.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Method == null)
            {
                return;
            }

            WriteObject(Method.GetParameters(), enumerateCollection: true);
        }
    }
}
