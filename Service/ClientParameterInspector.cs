using System;
using System.ServiceModel.Dispatcher;

namespace Artefacts.Service
{
	public class ClientParameterInspector : IParameterInspector
	{
		public ClientParameterInspector ()
		{
		}

		#region IParameterInspector implementation

		public void AfterCall (string operationName, object[] outputs, object returnValue, object correlationState)
		{
			throw new NotImplementedException ();
		}

		public object BeforeCall (string operationName, object[] inputs)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

