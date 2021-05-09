
using System;
using System.Linq;

using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;

namespace ICSharpCode.Decompiler.CSharp.Transforms
{
	/// <summary>
    /// Tries to find appropriate constants for numeric literals based on context.
	/// </summary>
	/// <remarks>
    /// This is a crock.
	/// </remarks>

	public class SymbolicRepresentation
	{
		public readonly string Name;
		public SymbolicRepresentation(string name)
		{
			this.Name = name;
		}
	}

	public class SymbolicContext
	{
		private static int contextCount;
		public SymbolicRepresentation Representation;
		public readonly int ContextNumber;

		public SymbolicContext(SymbolicRepresentation representation = null)
		{
			this.Representation = representation;
			ContextNumber = ++contextCount;
		}

	}

	public class ReplaceLiteralsWithConstants : DepthFirstAstVisitor<SymbolicContext, int>, IAstTransform
	{
		private readonly SymbolicRepresentation layerMaskSymbolicRepresentation = new("LayerMask");

		public override int VisitIdentifier(Identifier identifier, SymbolicContext data)
		{
			if (data != null)
			{
				if (identifier.Name.Equals("layerMask", StringComparison.OrdinalIgnoreCase) ||
						identifier.Name.Equals("_layerMask", StringComparison.OrdinalIgnoreCase))
				{
					data.Representation = layerMaskSymbolicRepresentation;
				}
			}

			return base.VisitIdentifier(identifier, data);
		}

		public override int VisitInvocationExpression(InvocationExpression invocationExpression, SymbolicContext data)
		{
			return base.VisitInvocationExpression(invocationExpression, data);
		}

		protected override int VisitChildren(AstNode node, SymbolicContext data)
		{
			node.SaveContext(data);
			return base.VisitChildren(node, node.HasSymbolicContext() ? data ?? new() : null);
		}

		TransformContext context;

		void IAstTransform.Run(AstNode node, TransformContext context)
		{
			this.context = context;
			try
			{
				VisitChildren(node, null);
			}
			finally
			{
				this.context = null;
			}
		}
	}

	public static class SymbolicContextExtensions
	{
		public static void SaveContext(this AstNode node, SymbolicContext symbolicContext)
		{
			if (symbolicContext != null)
			{
				var oldAnnotation = node.Annotation<SymbolicContext>();
				if (oldAnnotation != null)
				{
					node.RemoveAnnotations<SymbolicContext>();
					symbolicContext.Representation ??= oldAnnotation.Representation;
				}
				node.AddAnnotation(symbolicContext);
			}
		}

		public static bool HasSymbolicContext(this AstNode node)
		{
			return
				node is BinaryOperatorExpression binary && (binary.Operator.IsBitwise() || binary.Operator.IsEquality()) ||
				node is UnaryOperatorExpression unary && unary.Operator == UnaryOperatorType.BitNot ||
				node is VariableInitializer ||
				node is MemberReferenceExpression ||
				node is CastExpression ||
				node is IdentifierExpression ||
				node is NamedArgumentExpression ||
				node is ParenthesizedExpression ||
				node is AssignmentExpression;
		}

		public static bool IsEquality(this BinaryOperatorType operatorType)
		{
			return operatorType == BinaryOperatorType.Equality || operatorType == BinaryOperatorType.InEquality;
		}
	}
}
