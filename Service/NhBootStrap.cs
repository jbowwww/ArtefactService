
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Driver;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using Artefacts.Core;
using System.Reflection;
using System.Linq;
using NHibernate.Tool.hbm2ddl;

namespace Artefacts.Service
{
	public static class NhBootStrap
	{
		#region Private fields
		private static IAutomappingConfiguration _mapping = new AutoMapperConfiguration();
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
					_configuration = Fluently.Configure().Database(
						MySQLConfiguration.Standard.ConnectionString("Database=Artefacts3;Data Source=192.168.1.10;User Id=root"))
						.Mappings(m => m.AutoMappings
//							.Add(AutoMap.AssemblyOf<Artefact>(_mapping))
							.Add(AutoMap.Assemblies(_mapping,
								Artefact.ArtefactTypes.ConvertAll<Assembly>(T => T.Assembly).Distinct())))
//						.Database(MySQLConfiguration.Standard.c);
						.BuildConfiguration();
//						IPersistenceConfigurer pc;
					
					
//					SchemaExport exporter = new SchemaExport(_configuration);
//					exporter.
					SchemaValidator validator = new SchemaValidator(_configuration);
					try {
						validator.Validate();
						_configuration.DataBaseIntegration(db => db.SchemaAction = SchemaAutoAction.Update);
					}
					catch (HibernateException hEx) {
						_configuration.DataBaseIntegration(db => db.SchemaAction = SchemaAutoAction.Create);
					}
					
					

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

