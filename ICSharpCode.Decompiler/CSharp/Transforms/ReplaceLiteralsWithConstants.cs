
using System;
using System.Linq;
using System.Collections.Generic;

using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;
using ICSharpCode.Decompiler.IL;

namespace ICSharpCode.Decompiler.CSharp.Transforms
{
	/// <summary>
	/// Tries to find appropriate constants for numeric literals based on context.
	/// </summary>
	/// <remarks>
	/// This is a crock.
	/// </remarks>

	public class SymbolicRepresentationIncompatibleMerge : Exception
	{
		public SymbolicRepresentationIncompatibleMerge() { }

		public SymbolicRepresentationIncompatibleMerge(string message)
			: base(message) { }

		public SymbolicRepresentationIncompatibleMerge(string message, Exception inner)
			: base(message, inner) { }
	}

	public class SymbolicRepresentation
	{
		public readonly string Name;
		public SymbolicRepresentation(string name)
		{
			this.Name = name;
		}

		public static SymbolicRepresentation Merge(SymbolicRepresentation rep1, SymbolicRepresentation rep2)
		{
			if (rep1 == rep2)
				return rep1;
			if (rep1 == null)
				return rep2;
			if (rep2 == null)
				return rep1;
			throw new SymbolicRepresentationIncompatibleMerge($"Cannot merge representations {rep1.Name} and {rep2.Name}");
		}
	}

	public class SymbolicContext
	{
		public class Inference
		{
			public SymbolicRepresentation Representation;
			public readonly int OriginalContextNumber;

			public Inference(SymbolicRepresentation representation, int contextNumber)
			{
				Representation = representation;
				OriginalContextNumber = contextNumber;
			}

			public void Merge(Inference other)
			{
				Representation = SymbolicRepresentation.Merge(Representation, other.Representation);
			}
		}

		private static int contextCount = 0;
		private Inference inference;
		public readonly int ContextNumber;
		public int InferenceNumber => inference.OriginalContextNumber;

		private SymbolicContext(SymbolicRepresentation representation, int contextNumber)
		{
			inference = new(representation, contextNumber);
			ContextNumber = contextNumber;
		}

		public static SymbolicContext Create(SymbolicRepresentation representation = null)
		{
			return new(representation, ++contextCount);
		}

		public void Merge(SymbolicContext other)
		{
			if (other != null)
			{
				other.inference.Merge(inference);
				inference = other.inference;
			}
		}

		public SymbolicRepresentation Representation => inference.Representation;
		public void SetRepresentation(SymbolicRepresentation representation, bool force = false)
		{
			if (inference.Representation == null || force)
			{
				inference.Representation = representation;
			}
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
					data.SetRepresentation(layerMaskSymbolicRepresentation);
				}
			}

			return base.VisitIdentifier(identifier, data);
		}

		public override int VisitInvocationExpression(InvocationExpression invocationExpression, SymbolicContext data)
		{
			return base.VisitInvocationExpression(invocationExpression, data);
		}

		protected override int VisitChildren(AstNode node, SymbolicContext symbolicContext)
		{
			node.SaveContext(symbolicContext);
			return base.VisitChildren(node, node.HasSymbolicContext() ? symbolicContext.Ensure() : null);
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
#if false
		public static SymbolicContext Merge(this SymbolicContext context1, SymbolicContext context2)
		{
			if (context1 == null)
				context1 = context2;
			else
				context1.Merge(context2);
			return context1;
		}
#endif

		public static SymbolicContext Ensure(this SymbolicContext context)
		{
			return context ?? SymbolicContext.Create();
		}

		public static void SaveContext(this AstNode node, SymbolicContext symbolicContext)
		{
			if (symbolicContext != null)
			{
				var oldAnnotation = node.Annotation<SymbolicContext>();
				if (oldAnnotation == null)
				{
					node.AddAnnotation(symbolicContext);
				}
				else
				{
					oldAnnotation.Merge(symbolicContext);
				}
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
