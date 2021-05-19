using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.CSharp.Resolver;

using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace ICSharpCode.Decompiler.CSharp.OutputVisitor
{

	public class XmlOutputVisitor : AbstracttAstVisitor
	{
		readonly public XmlWriter writer;

		class UidMapper : Dictionary<object, int>
		{
			private int count = 0;

			public string Tag(object o)
			{
				if (!TryGetValue(o, out int uid))
				{
					uid = ++count;
					Add(o, uid);
				}
				return o.ToString() + "@" + uid.ToString();
			}
		}

		private UidMapper uidMapper = new();

		public XmlOutputVisitor(TextWriter textWriter)
		{
			if (textWriter == null)
			{
				throw new ArgumentNullException(nameof(textWriter));
			}
			writer = XmlWriter.Create(textWriter, GetSettings());
		}

		protected XmlWriterSettings GetSettings()
		{
			var settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.CheckCharacters = false;
			return settings;
		}

		protected void VisitNode(AstNode node)
		{
			if (node is UsingDeclaration || node is AttributeSection)
				return;

			WriteStartElement(node.GetType().Name);

			var symbol = node.GetSymbol();
			if (symbol != null)
			{
				WriteAttribute("Symbol", symbol.Name, true);
				WriteAttribute("SymbolType", symbol.GetType().Name);
			}
			var variable = node.GetILVariable();
			if (variable != null)
			{
				WriteAttribute("Variable", variable.Name, true);
			}
			var parameter = node.Annotation<InvocationParameter>();
			if (parameter != null)
			{
				WriteAttribute("ParameterId", parameter.uniqueId);
				WriteAttribute("ParameterSymbol", parameter.parameter.Name);
				WriteAttribute("ParameterSymbolType", parameter.parameter.GetType().Name);
				if (parameter.Variable is not null)
					WriteAttribute("ParameterVariable", parameter.Variable.Name);
			}
#if false
			var resolveResult = node.Annotation<ResolveResult>();
			if (resolveResult != null)
			{
				WriteAttribute("ResolveResult", resolveResult.GetType().Name);
				if (resolveResult is ILVariableResolveResult ilvrr)
				{
					WriteAttribute("Variable", ilvrr.Variable.Name);
				}
			}
#endif
			WriteContextAttributes(node.Annotation<SymbolicContext>());
			node.AcceptVisitor(this);
			if (node is Expression && !(node.Parent is Expression))
			{
				WriteComment(node);
			}

			if (!node.IsNull)
			{
				foreach (AstNode child in node.Children)
				{
					VisitNode(child);
				}
			}

			WriteEndElement();
		}

		public void WriteSyntaxTree(SyntaxTree syntaxTree)
		{
			VisitNode(syntaxTree);
		}

		public override void VisitInvocationExpression(InvocationExpression invocationExpression)
		{
			bool hasArgMap = false;
			var resolveResult = invocationExpression.Annotation<CSharpInvocationResolveResult>();
			if (resolveResult != null)
			{
				var argmap = resolveResult.GetArgumentToParameterMap();
				if (argmap != null)
					hasArgMap = true;
			}
			if (hasArgMap)
			{
				WriteAttribute("Mapped", true);
			}
			base.VisitInvocationExpression(invocationExpression);
		}
		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			WriteComment(methodDeclaration.GetSignature());
			base.VisitMethodDeclaration(methodDeclaration);
		}

		public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
		{
			WriteComment(propertyDeclaration.GetSignature());
			base.VisitPropertyDeclaration(propertyDeclaration);
		}

		public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
		{
			WriteComment(fieldDeclaration.GetSignature());
			base.VisitFieldDeclaration(fieldDeclaration);
		}

		public override void VisitPrimitiveType(PrimitiveType primitiveType)
		{
			WriteString(primitiveType.ToString());
		}

		public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
		{
			WriteAttribute("Type", primitiveExpression.Value.GetType().FullName);
			WriteAttribute("Value", primitiveExpression.Value);
		}

		public override void VisitIdentifier(Identifier identifier)
		{
			WriteAttribute("Name", identifier.Name);
		}

		public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
		{
			WriteAttribute("Operator", assignmentExpression.Operator);
		}

		public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
		{
			WriteAttribute("Operator", unaryOperatorExpression.Operator);
		}

		public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
		{
			WriteAttribute("Operator", binaryOperatorExpression.Operator);
		}

		public virtual void VisitCSharpModifierToken(CSharpModifierToken cSharpModifierToken)
		{
			WriteString(CSharpModifierToken.GetModifierName(cSharpModifierToken.Modifier));
		}

		public override void VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode)
		{
			if (cSharpTokenNode is CSharpModifierToken cSharpModifierToken)
			{
				VisitCSharpModifierToken(cSharpModifierToken);
			}
			else
			{
				base.VisitCSharpTokenNode(cSharpTokenNode);
			}
		}

		private void WriteContextAttributes(SymbolicContext symbolicContext)
		{
			if (symbolicContext != null)
			{
				WriteAttribute("Context", symbolicContext.InferenceNumber);
				var representation = symbolicContext.Representation;
				if (representation != null)
				{
					WriteAttribute("Representation", representation.Name);
				}
			}
		}

		private void WriteStartElement(string tagName)
		{
			writer.WriteStartElement(tagName);
		}

		private void WriteEndElement()
		{
			writer.WriteEndElement();
		}

		private void WriteString(string s)
		{
			writer.WriteString(s);
		}

		private void WriteAttribute(string attributeName, object value, bool uid)
		{
			WriteAttribute(attributeName, uid ? uidMapper.Tag(value) : value);
		}

		private void WriteAttribute(string attributeName, object value)
		{
			try
			{
				writer.WriteAttributeString(attributeName, value.ToString());
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void WriteComment(object comment)
		{
			writer.WriteComment(" " + comment.ToString().Replace(Environment.NewLine, " ") + " ");
		}

	}

	public static class XmlOutputVisitorExtensions
    {
		public static string GetSignature(this FieldDeclaration fieldDeclaration)
		{
			return GetEntitySignature(fieldDeclaration) + fieldDeclaration.GetSymbol().Name;
		}

		public static string GetSignature(this PropertyDeclaration propertyDeclaration)
		{
			return GetEntitySignature(propertyDeclaration);
		}

		public static string GetSignature(this MethodDeclaration methodDeclaration)
		{
			return GetMethodSignature(methodDeclaration);
		}

		private static string GetMethodSignature(MethodDeclaration methodDeclaration)
		{
			string methDecl = GetEntitySignature(methodDeclaration);
			bool sep = false;

			foreach (var tp in methodDeclaration.TypeParameters)
			{
				if (sep)
					methDecl += ",";
				else
				{
					methDecl += "<";
					sep = true;
				}
				methDecl += tp.ToString();
			}
			if (sep)
				methDecl += ">";
			methDecl += "(";
			sep = false;
			foreach (var parm in methodDeclaration.Parameters)
			{
				if (sep)
					methDecl += ",";
				else
					sep = true;
				methDecl += parm.ToString();
			}
			methDecl += ")";
			return methDecl;
		}

		private static string GetEntitySignature(EntityDeclaration entityDeclaration)
		{
			string methDecl = "";
			bool sep = false;
			foreach (var attribute in entityDeclaration.Attributes)
			{
				if (sep)
					methDecl += " ";
				else
					sep = true;
				methDecl += attribute.ToString();
			}
			foreach (var modifier in entityDeclaration.ModifierTokens)
			{
				if (sep)
					methDecl += " ";
				else
					sep = true;
				methDecl += modifier.ToString();
			}
			if (sep)
				methDecl += " ";
			return methDecl + entityDeclaration.ReturnType.ToString() + " " + entityDeclaration.NameToken.ToString();
		}

	}
}
