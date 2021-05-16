using System;
using System.Linq;
using System.Collections.Generic;

using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.Semantics;

namespace ICSharpCode.Decompiler.CSharp.Transforms
{
	/// <summary>
	/// Annotates InvocationExpression instances.
	/// </summary>

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
		public InvocationParameter(IParameter parameter)
		{
			this.parameter = parameter;
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
	}

	public class MethodAutoMap : AutoInsertDictionary<IMethod, InvocationMethod>
	{
		public override InvocationMethod NewValue(IMethod method)
		{
			return new(method);
		}
	}

	public class AnnotateInvocationExpressions : IAstTransform
	{
		class MethodMapBuilder : DepthFirstAstVisitor
		{
			public readonly MethodAutoMap methodMap = new();
			public readonly Dictionary<IParameter, InvocationParameter> parameterMap = new();

			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
			{
				if (methodDeclaration.GetSymbol() is IMethod method)
				{
					var invocationMethod = methodMap[method];
					foreach (var (index, parameterDeclaration) in methodDeclaration.Parameters.WithIndex())
					{
						var invocationParameter = invocationMethod.parameters[index];
						invocationParameter.Variable = parameterDeclaration.GetILVariable();
						parameterDeclaration.AddAnnotation(invocationParameter);
					}
					foreach (var invocationParameter in invocationMethod.parameters)
					{
						parameterMap.Add(invocationParameter.parameter, invocationParameter);
					}
				}
				base.VisitMethodDeclaration(methodDeclaration);
			}

			public static MethodAutoMap Process(AstNode node)
			{
				MethodMapBuilder builder = new();
				builder.VisitChildren(node);
				return builder.methodMap;
			}
		}

		class InvocationAnnotator : DepthFirstAstVisitor
		{
			protected readonly MethodAutoMap methodMap;

			public InvocationAnnotator(MethodAutoMap methodMap)
			{
				this.methodMap = methodMap;
			}
			public override void VisitInvocationExpression(InvocationExpression invocationExpression)
			{
				if (invocationExpression.GetSymbol() is IMethod method)
				{
					var invocationMethod = methodMap[method];
					var rr = invocationExpression.Annotation<CSharpInvocationResolveResult>();
					if (rr != null)
					{
						var argMap = rr.GetArgumentToParameterMap();
						int argIdx = 0;
						foreach (var argu in invocationExpression.Arguments)
						{
							var parm = invocationMethod.GetParameter(argMap == null ? argIdx : argMap[argIdx]);
							if (parm is not null)
							{
								argu.AddAnnotation(parm);
							}
							++argIdx;
						}
					}
				}
				base.VisitInvocationExpression(invocationExpression);
			}

			public static void Process(AstNode node)
			{
				var methodMap = MethodMapBuilder.Process(node);
				InvocationAnnotator annotator = new(methodMap);
				annotator.VisitChildren(node);
			}
		}

		void IAstTransform.Run(AstNode node, TransformContext context)
		{
			InvocationAnnotator.Process(node);
		}
	}
}
