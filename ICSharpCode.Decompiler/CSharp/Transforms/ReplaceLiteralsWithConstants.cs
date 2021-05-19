
using System;
using System.Linq;
using System.Collections.Generic;

using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.CSharp.Resolver;

namespace ICSharpCode.Decompiler.CSharp.Transforms
{
	/// <summary>
	/// Tries to find appropriate constants for numeric literals based on context.
	/// </summary>
	/// <remarks>
	/// This is a crock.
	/// </remarks>

	#region SymbolicRepresentation
	public class SymbolicRepresentationIncompatibleMerge : Exception
	{
		public SymbolicRepresentationIncompatibleMerge() { }
		public SymbolicRepresentationIncompatibleMerge(string message) : base(message) { }
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
	#endregion

	#region SymbolicContext
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

		public SymbolicContext(SymbolicRepresentation representation = null)
		{
			inference = new(representation, ++contextCount);
			ContextNumber = contextCount;
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
	#endregion

	#region InvocationMethod/InvocationParameter
	public class InvocationParameter
	{
		private static int counter = 0;
		public readonly int uniqueId;
		public readonly IParameter parameter;
		private ILVariable variable = null;
		public ILVariable Variable {
			get { return variable; }
			internal set {
				if (variable is null)
					variable = value;
				else if (variable != value)
					throw new InvalidOperationException("Parameter variable changed");
			}
		}
		public InvocationParameter(IParameter parameter, ILVariable variable = null)
		{
			this.parameter = parameter;
			this.variable = variable;
			uniqueId = ++counter;
		}
	}

	public class InvocationMethod
	{
		public readonly IMethod method;
		public readonly InvocationParameter[] parameters;

		public InvocationMethod(IMethod method)
		{
			if (method is null)
				throw new ArgumentNullException(nameof(method));
			this.method = method;
			parameters = method.Parameters.Select(p => new InvocationParameter(p)).ToArray();
		}

		public InvocationParameter GetParameter(int index)
		{
			return (index >= 0 && index < parameters.Length) ? parameters[index] : null;
		}

		public InvocationParameter GetParameter(int index, IReadOnlyList<int> mapper)
		{
			if (mapper is not null)
				index = mapper[index];
			return GetParameter(index);
		}
	}

	public class MethodAutoMap : AutoInsertDictionary<IMethod, InvocationMethod>
	{
		public override InvocationMethod NewValue(IMethod method)
		{
			return new(method);
		}
	}
	#endregion

	public class ReplaceLiteralsWithConstants : DepthFirstAstVisitor<SymbolicContext, int>, IAstTransform
	{

		#region Identifying invocation variables
		public readonly MethodAutoMap methodMap = new();

		private void BuildMethodMap(AstNode rootNode)
		{
			foreach (var methodDeclaration in rootNode.DescendantsAndSelf.OfType<MethodDeclaration>())
			{
				if (methodDeclaration.GetSymbol() is IMethod method)
				{
					var invocationMethod = methodMap[method];
					foreach (var (index, parameterDeclaration) in methodDeclaration.Parameters.WithIndex())
					{
						var invocationParameter = invocationMethod.parameters[index];
						invocationParameter.Variable = parameterDeclaration.GetILVariable();
					}
				}
			}
		}
		#endregion

		private Dictionary<ILVariable, SymbolicContext> variableContextMap = new();

		private SymbolicContext GetVariableContext(ILVariable variable, SymbolicContext mergeContext = null)
		{
			if (variable is null)
				return mergeContext;

			if (variableContextMap.TryGetValue(variable, out var variableContext))
			{
				variableContext.Merge(mergeContext);
			}
			else
			{
				variableContextMap.Add(variable, variableContext = mergeContext.Ensure());
			}
			return variableContext;
		}

		private readonly SymbolicRepresentation layerMaskSymbolicRepresentation = new("LayerMask");
		private readonly SymbolicRepresentation hitMaskSymbolicRepresentation = new("HitMask");

		private SymbolicRepresentation GetRepresentation(string name)
		{
			if (name is not null)
			{
				if (name.Equals("layerMask", StringComparison.OrdinalIgnoreCase) || name.Equals("_layerMask", StringComparison.OrdinalIgnoreCase))
				{
					return layerMaskSymbolicRepresentation;
				}
				if (name.Equals("hitMask", StringComparison.OrdinalIgnoreCase) || name.Equals("_hitMask", StringComparison.OrdinalIgnoreCase))
				{
					return hitMaskSymbolicRepresentation;
				}
			}
			return null;
		}

		private void SetRepresentation(ref SymbolicContext symbolicContext, string name)
		{
			var symbolicRepresentation = GetRepresentation(name);

			if (symbolicRepresentation is not null)
			{
				if (symbolicContext is null)
					symbolicContext = new SymbolicContext(symbolicRepresentation);
				else
					symbolicContext.SetRepresentation(symbolicRepresentation);
			}
		}

		public override int VisitIdentifier(Identifier identifier, SymbolicContext symbolicContext)
		{
			SetRepresentation(ref symbolicContext, identifier.Name);
			return base.VisitIdentifier(identifier, symbolicContext);
		}

		public override int VisitInvocationExpression(InvocationExpression invocationExpression, SymbolicContext symbolicContext)
		{
			UpdateSymbolicContextForNode(invocationExpression, ref symbolicContext);
			if (invocationExpression.GetSymbol() is IMethod method)
			{
				var invocationMethod = methodMap[method];
				var rr = invocationExpression.Annotation<CSharpInvocationResolveResult>();
				if (rr != null)
				{
					var argMap = rr.GetArgumentToParameterMap();
					int argumentIndex = 0;
					foreach (var child in invocationExpression.Children)
					{
						symbolicContext = null;
						if (child.Role == Roles.Argument)
						{
							var invocationParameter = invocationMethod.GetParameter(argumentIndex++, argMap);
							if (invocationParameter is not null)
							{
								var variable = invocationParameter.Variable;
								if (variable is not null)
									symbolicContext = GetVariableContext(variable, symbolicContext);
								SetRepresentation(ref symbolicContext, invocationParameter.parameter.Name);
							}
						}
						child.AcceptVisitor(this, symbolicContext);
					}
				}
			}
			return default;
		}

		protected override int VisitChildren(AstNode node, SymbolicContext symbolicContext)
		{
			UpdateSymbolicContextForNode(node, ref symbolicContext);
			return base.VisitChildren(node, symbolicContext);
		}

		protected void UpdateSymbolicContextForNode(AstNode node, ref SymbolicContext symbolicContext)
		{
			symbolicContext = GetVariableContext(node.GetILVariable(), symbolicContext);
			node.SaveContext(symbolicContext);
			symbolicContext = node.HasSymbolicContext() ? symbolicContext.Ensure() : null;
		}

		TransformContext context;

		void IAstTransform.Run(AstNode node, TransformContext context)
		{
			this.context = context;
			try
			{
				BuildMethodMap(node);
				VisitChildren(node, null);
			}
			finally
			{
				this.context = null;
			}
		}
	}

	#region SymbolicContextExtensions
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
			return context ?? new();
		}

		public static ILVariable GetILVariable(this AstNode node)
		{
			if (node.Annotation<ResolveResult>() is ILVariableResolveResult rr)
				return rr.Variable;
			else
				return null;
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
	#endregion
}
