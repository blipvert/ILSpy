using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;

using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace ICSharpCode.Decompiler.CSharp.OutputVisitor
{

	public class XmlOutputVisitor : AbstracttAstVisitor
	{
		readonly public XmlWriter writer;

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
			//			settings.ConformanceLevel = ConformanceLevel.Fragment;
			return settings;
		}

		protected void VisitNode(AstNode node)
		{
			WriteStartElement(node.GetType().Name);

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
}
