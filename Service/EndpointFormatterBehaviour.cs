using System;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Artefacts.Service
{
	public class EndpointFormatterBehaviour : IEndpointBehavior
	{
		public EndpointFormatterBehaviour ()
		{
		}

		#region IEndpointBehavior implementation

		public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection parameters)
		{
			throw new NotImplementedException ();
		}

		public void ApplyDispatchBehavior (ServiceEndpoint serviceEndpoint, System.ServiceModel.Dispatcher.EndpointDispatcher dispatcher)
		{
			throw new NotImplementedException ();
		}

		public void ApplyClientBehavior (ServiceEndpoint serviceEndpoint, System.ServiceModel.Dispatcher.ClientRuntime behavior)
		{
			behavior.ClientMessageInspectors.Add(new ClientMessageInspector());
//			foreach (ClientOperation op in behavior.Operations)
//				op.ClientParameterInspectors.Add(
		}

		public void Validate (ServiceEndpoint serviceEndpoint)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

