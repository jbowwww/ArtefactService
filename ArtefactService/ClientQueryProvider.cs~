using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

using Serialize.Linq;
using Serialize.Linq.Serializers;
using Serialize.Linq.Nodes;
using Serialize.Linq.Extensions;

namespace Artefacts.Services
{
	public class ClientQueryProvider<TArtefact> :
		IQueryProvider
		where TArtefact : Artefact
	{
		private IRepository<TArtefact> _repo = null;
		
		public ClientQueryProvider(IRepository<TArtefact> channel)
		{
			_repo = channel;
		}

		#region IQueryProvider implementation
		public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
		{
			return (this as IQueryProvider).CreateQuery<Artefact>(expression);
		}

		public object Execute(System.Linq.Expressions.Expression expression)
		{
//			StringBuilder sb = new StringBuilder();
//			System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(System.Linq.Expressions.Expression));
//			
//			xs.Serialize(new StringWriter(sb), expression.ToXml());
			
//			 System.Xml.Serialization.XmlSerializer.GenerateSerializer(new Type[] { typeof(ExpressionNode) },
				
//			dcs.WriteObject(XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(sb)), expression, new WCFTypeResolver());

			ExpressionNode en = expression.ToExpressionNode();
//			return _repo.CreateQuery(en);
			
			MemoryStream ms = new MemoryStream();
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			bf.Serialize(ms, en);
			string s = Convert.ToBase64String(ms.GetBuffer(), Base64FormattingOptions.InsertLineBreaks);
//			return null;
			
			return _repo.CreateQuery_EN_Binary(ms.GetBuffer());
		}

		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
		{
			if (!typeof(Artefact).IsAssignableFrom(typeof(TElement)))
				throw new ArgumentOutOfRangeException("TElement", typeof(TElement), "TElement should derive from Artefact");
			return (IQueryable<TElement>)new Queryable<TArtefact>(this, _repo, expression);
		}

		TResult IQueryProvider.Execute<TResult>(System.Linq.Expressions.Expression expression)
		{
			ExpressionNode en = expression.ToExpressionNode();
			_repo.CreateQuery(en);
			return default(TResult);
		}
		#endregion
	}
}

