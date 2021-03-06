using System;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Artefacts.Service
{
	public static class ServiceModel_Extensions
	{
		public static string ToString(this ServiceHostBase shb, bool dummy)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}\nBase Addresses ({1}): ", shb.Description.ToString(true), shb.BaseAddresses.Count);
			foreach (Uri baseUri in shb.BaseAddresses)
				sb.AppendFormat("{0} ", baseUri.ToString());
			sb.AppendFormat("\nEndpoints ({0}):\n", shb.Description.Endpoints.Count);
			foreach (ServiceEndpoint endpoint in shb.Description.Endpoints)
				sb.AppendFormat("\t{0}\n", endpoint.ToString(true));
			sb.AppendFormat("ChannelDispatchers ({0}):\n", shb.ChannelDispatchers.Count);
			foreach (ChannelDispatcherBase cdb in shb.ChannelDispatchers)
				sb.AppendFormat("\t{0} : {1}\n", cdb.Listener.State.ToString(), cdb.Listener.Uri.ToString());
			sb.AppendFormat("Extensions ({0}):\n", shb.Extensions.Count);
			foreach (IExtension<ServiceHostBase> ext in shb.Extensions)
				sb.AppendFormat("\t{0}\n", ext.ToString());
			return sb.AppendLine().ToString();
		}

		public static string ToString (this ServiceDescription d, bool dummy)
		{
			return string.Format("Service: {0}\nName: {1}/{2}\nConfiguration Name: {3}",
				d.ServiceType.FullName, d.Namespace, d.Name, d.ConfigurationName);
		}

		public static string ToString(this ServiceEndpoint se, bool dummy)
		{
			return se.Name + " " + se.ListenUriMode.ToString() + " " + se.ListenUri + " " + se.Binding.ToString();
		}
	}
}
