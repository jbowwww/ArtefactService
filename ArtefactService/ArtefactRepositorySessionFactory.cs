using System;
using System.Collections.Generic;
using System.Reflection;

using NHibernate;
using NHibernate.Cfg;

namespace Artefacts.Services
{
	public static class ArtefactRepositorySessionFactory
	{
		#region Static members
		private static Configuration _configuration;
		private static ISessionFactory _sessionFactory;

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
							.DataBaseIntegration(
								(p) =>
							{
//								p.LogSqlInConsole = true;
								p.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
								p.SchemaAction = SchemaAutoAction.Create; 			//.Update;
							});

//					List<Assembly> artefactAssemblies = new List<Assembly>();
//					foreach (Type artefactType in Artefact.ArtefactTypes)
//						if (!artefactAssemblies.Contains(artefactType.Assembly))
//							artefactAssemblies.Add(artefactType.Assembly);
//
//					foreach (Assembly assembly in artefactAssemblies)
//						_configuration.AddAssembly(assembly);

//					_configuration.AddAssembly("Artefacts");
//					_configuration.AddAssembly("ArtefactFileSystem");

//					_configuration.AddAssembly(Assembly.LoadFile("Artefacts.dll"));
//					_configuration.AddAssembly(Assembly.LoadFile("ArtefactFileSystem.dll"));
//					Assembly.l
				}
				return _configuration;
			}
		}

		/// <summary>
		/// Gets the session factory.
		/// </summary>
		public static ISessionFactory SessionFactory {
			get { return _sessionFactory != null ? _sessionFactory : _sessionFactory = Config.BuildSessionFactory(); }
		}
		#endregion

		#region Private fields
		private static ISession _nhSession;
		#endregion

		/// <summary>
		/// Gets the session.
		/// </summary>
		public static ISession Session {
			get { return _nhSession != null ? _nhSession : _nhSession = SessionFactory.OpenSession(); }
		}
	}
}

