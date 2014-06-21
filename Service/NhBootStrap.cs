using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Artefacts.Service
{
	public static class NhBootStrap
	{
		#region Private fields
		private static Configuration _configuration;
		private static ISessionFactory _sessionFactory;
		private static ISession _nhSession;
		#endregion

		/// <summary>
		/// Gets the <see cref="Configuration"/> instance
		/// </summary>
		public static Configuration Config {
			get
			{
				if (_configuration == null)
				{
					_configuration = new Configuration()
						.Configure("hibernate_server.cfg.xml")
						.DataBaseIntegration((p) =>
					{
//						p.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
						p.SchemaAction = SchemaAutoAction.Update;		//.Create;	//.Recreate; 			//.Update;
					});

					// not needed because hibernate.server.cfg adds mappings to the required assemblies
//					foreach (Assembly artefactTypeAssembly in Artefact.ArtefactTypes
//							.ConvertAll<Assembly>((input) => input.Assembly)
//							.Distinct().Where((assembly) => assembly.GetName().Name != "Artefacts"))
//						_configuration.AddAssembly(artefactTypeAssembly);
	
					
//					SchemaValidator validator = new SchemaValidator(_configuration);
//					validator.Validate();
//					new SchemaExport(_configuration);
				}
				return _configuration;
			}
		}

		/// <summary>
		/// Gets the session factory.
		/// </summary>
		public static ISessionFactory SessionFactory {
			get
			{
				return _sessionFactory != null && !_sessionFactory.IsClosed ?
					_sessionFactory : _sessionFactory = Config.BuildSessionFactory();
			}
		}
		
		/// <summary>
		/// Gets the session.
		/// </summary>
		public static ISession Session {
			get
			{
				return _nhSession != null && _nhSession.IsOpen ?
					_nhSession : _nhSession = SessionFactory.OpenSession();
			}
		}
	}
}

