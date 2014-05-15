using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

namespace Artefacts.Service
{
	public static class Expression_Extensions
	{
		private static readonly BinaryFormatter _bf = new BinaryFormatter();

		public static bool IsEnumerable(this Expression e)
		{
			return typeof(IEnumerable).IsAssignableFrom(e.Type);
		}
		
		public static bool IsMethodCallExpression(this Expression e)
		{
			return e.NodeType == ExpressionType.Call;
		}
		
		public static TExpression As<TExpression>(this Expression e)
			where TExpression : Expression
		{
			return (TExpression)e;
		}
		
		public static object Id(this Expression e)
		{
			return e.ToJson();		//.ToString();//.ToJson();	//ToExpressionNode().GetHashCode();
				//e.ToString();		// TODO: Will have to implement your own Id builder I think, because generic method calls don't include generic arguments in the string
		}

		public static byte[] ToBinary(this Expression e)
		{
			return ToBinary(e, _bf);
		}

		public static byte[] ToBinary(this Expression e, BinaryFormatter bf)
		{
			ExpressionNode en = e.ToExpressionNode();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, en);
			byte[] binaryExpression = ms.GetBuffer();
			return binaryExpression;
		}

		public static Expression FromBinary(this byte[] eBinary)
		{
			return FromBinary(eBinary, _bf);
		}

		public static Expression FromBinary(this byte[] eBinary, BinaryFormatter bf)
		{
			ExpressionNode en = (ExpressionNode)bf.Deserialize(new System.IO.MemoryStream(eBinary));
			Expression expression = en.ToExpression();
			return expression;
		}
	}
}

