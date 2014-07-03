using System;
using System.Collections.Generic;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace Artefacts.Service
{
	public class ClientMessageInspector : IClientMessageInspector
	{
		public ClientMessageInspector ()
		{
		}

		#region IClientMessageInspector implementation

		public void AfterReceiveReply (ref System.ServiceModel.Channels.Message message, object correlationState)
		{
			throw new NotImplementedException ();
		}

		public object BeforeSendRequest (ref System.ServiceModel.Channels.Message message, System.ServiceModel.IClientChannel channel)
		{
			Console.WriteLine("{0}: ", channel.LocalAddress.Uri);
			foreach (KeyValuePair<string, object> prop in message.Properties)
				Console.WriteLine("\t{0} = {1}", prop.Key, prop.Value);
		}

		#endregion
	}
}

