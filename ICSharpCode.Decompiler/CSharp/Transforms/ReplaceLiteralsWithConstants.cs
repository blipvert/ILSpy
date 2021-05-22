//#define DEBUG_ANNOTATE
#define BITVALUE_STUFF

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
		#region BitValue/BitValueExpression/Bitmask/Bitfield
		abstract class BitValue
		{
			public abstract bool Simple { get; }
			public virtual bool Inverted => false;
			public virtual BitValue UninvertedValue => this;
			public readonly int Weight;

			public readonly int Value;
			public int BitCount => Value.BitCount();

			public BitValue(int value, int weight = 1)
			{
				Value = value;
				Weight = weight;
			}

			protected abstract Expression ExpressValue(TransformContext context);

			public virtual Expression Express(TransformContext context)
			{
				return ExpressValue(context).WithCIRR(context, Value);
			}
			public abstract BitValue Simplify();

			public virtual BitValue Invert()
			{
				return new InvertedBitValueExpression(this);
			}
		}

		abstract class SimpleBitValue : BitValue
		{
			public override bool Simple => true;
			protected readonly BitValue bitValue;

			public SimpleBitValue(BitValue bv, int? value = null) : base(value ?? bv.Value, bv.Weight)
			{
				bitValue = bv;
			}

			public override BitValue Simplify()
			{
				return this;
			}
		}

		class CombinedBitValue : BitValue
		{
			public override bool Simple => false;
			protected readonly BitValue left, right;

			public CombinedBitValue(BitValue left, BitValue right) : base(left.Value | right.Value, left.Weight + right.Weight)
			{
				this.left = left;
				this.right = right;
			}
			protected override Expression ExpressValue(TransformContext context)
			{
				var lhs = left.Express(context);
				var rhs = right.Express(context);
				return new BinaryOperatorExpression(lhs, BinaryOperatorType.BitwiseOr, rhs);
			}

			public override BitValue Simplify()
			{
				return new BitValueGroup(this);
			}
		}

		class BitValueGroup : SimpleBitValue
		{
			public BitValueGroup(BitValue bv) : base(bv)
			{
			}

			protected override Expression ExpressValue(TransformContext context)
			{
				return new ParenthesizedExpression(bitValue.Express(context));
			}
		}

		class InvertedBitValueExpression : SimpleBitValue
		{
			public override bool Inverted => true;
			public override BitValue UninvertedValue => bitValue;

			public InvertedBitValueExpression(BitValue bv) : base(bv.UninvertedValue, ~bv.UninvertedValue.Value)
			{
			}

			protected override Expression ExpressValue(TransformContext context)
			{
				return new UnaryOperatorExpression(UnaryOperatorType.BitNot, bitValue.Express(context));
			}

			public override BitValue Invert()
			{
				return bitValue;
			}
		}

		class Bitmask : SimpleBitValue, IComparable<Bitmask>
		{
			public readonly IField Field;
			public BitValue Expansion => bitValue;

			public Bitmask(IField field, BitValue bitValue) : base(bitValue.Simplify())
			{
				Field = field;
			}

			public int CompareTo(Bitmask other)
			{
				int d = other.BitCount.CompareTo(BitCount);
				if (d != 0)
					return d;
				return other.Value.CompareTo(Value);
			}

			protected override Expression ExpressValue(TransformContext context)
			{
				throw new NotImplementedException();
			}

			public override Expression Express(TransformContext context)
			{
				return Field.CreateMemberReference(context);
			}
		}

		class BitPosition : BitValue
		{
			public override bool Simple => true;
			private IField field = null;
			public readonly int position;
			public BitPosition(int position) : base(1 << position)
			{
				this.position = position;
			}
			public void SetField(IField field)
			{
				if (position != field.IntegerConstantValue())
					throw new ArgumentException($"{field} value mismatch with bit position {position}");
				this.field = field;
			}

			private Expression GetPositionExpression(TransformContext context)
			{
				return (field is null) ? position.CreatePrimitive(context) : field.CreateMemberReference(context);
			}

			protected override Expression ExpressValue(TransformContext context)
			{
				var lhs = 1.CreatePrimitive(context);
				var op = BinaryOperatorType.ShiftLeft;
				var rhs = GetPositionExpression(context);
				var expr = new BinaryOperatorExpression(lhs, op, rhs).WithCIRR(context, Value);
				return new ParenthesizedExpression(expr);
			}

			public override BitValue Simplify()
			{
				return this;
			}
		}

		class Bitfield
		{
			private readonly BitPosition[] position = Enumerable.Range(0, 32).Select(x => new BitPosition(x)).ToArray();

			public BitPosition[] Position => position;

			public readonly SortedSet<Bitmask> masks = new();

			public Bitfield()
			{
			}

			public void SetPosition(IField field)
			{
				int bitNumber = field.IntegerConstantValue();
				if (bitNumber < 0 || bitNumber > 31)
				{
					throw new ArgumentException($"field {field} specifies bit {bitNumber} outside of 32-bit range");
				}
				try
				{
					position[field.IntegerConstantValue()].SetField(field);
				}
				catch (Exception e)
				{
					throw e;
				}
			}

			public void AddMask_Old(IField field)
			{
				var value = field.IntegerConstantValue();
				if (value != 0)
				{
					var bv1 = Decompose(value);
					var bv2 = Decompose(~value);
					masks.Add(new Bitmask(field,
						bv2.Weight < bv1.Weight ? bv2.Invert() : bv1));
				}
			}

			public void AddMask(IField field)
			{
				var value = field.IntegerConstantValue();
				if (value != 0)
				{
					var bv = Translate(value);

					masks.Add(new Bitmask(field, bv));
				}
			}

			public BitValue Translate(int value)
			{
				if (value < 0)
					return Decompose(~value).Invert();
				else
					return Decompose(value);
			}

			public BitValue Decompose(int value)
			{
				IEnumerable<BitValue> Iter(int value)
				{
					foreach (var m in masks)
					{
						if (value.AllSet(m.Value))
						{
							yield return m;
							value = value.Clear(m.Value);
						}
					}
					foreach (var b in value.Bits())
					{
						yield return position[b];
					}
				}

				BitValue bitValue = null;

				foreach (var bv in Iter(value))
				{
					bitValue = (bitValue is null) ? bv : new CombinedBitValue(bitValue, bv);
				}
				return bitValue;
			}
		}
		#endregion

		#region Collecting Constant Declarations
		private Bitfield layerMaskBitfield = new();
		private Bitfield hitMaskBitfield = new();

		private void CollectConstantDeclarations(AstNode rootNode)
		{
			foreach (var typeDeclaration in rootNode.Children.OfType<TypeDeclaration>())
			{
				if (typeDeclaration.Role == SyntaxTree.MemberRole)
				{
					var typeSymbol = typeDeclaration.GetSymbol();
					if (typeSymbol.Name == "Constants")
					{
						foreach (var fieldDeclaration in typeDeclaration.Children.OfType<FieldDeclaration>())
						{
							if (fieldDeclaration.Role == Roles.TypeMemberRole)
							{
								var fieldSymbol = fieldDeclaration.GetSymbol() as IField;
								if (fieldSymbol.IsIntegerConstant() && fieldSymbol.Name.StartsWith("cLayer"))
								{
									if (fieldSymbol.Name.StartsWith("cLayerMask"))
										layerMaskBitfield.AddMask(fieldSymbol);
									else
										layerMaskBitfield.SetPosition(fieldSymbol);
								}
							}
						}
					}
					if (typeSymbol.Name == "Voxel")
					{
						foreach (var fieldDeclaration in typeDeclaration.Children.OfType<FieldDeclaration>())
						{
							if (fieldDeclaration.Role == Roles.TypeMemberRole)
							{
								var fieldSymbol = fieldDeclaration.GetSymbol() as IField;
								if (fieldSymbol.IsIntegerConstant() && fieldSymbol.Name.StartsWith("HM_"))
									hitMaskBitfield.AddMask(fieldSymbol);
							}
						}
					}
				}
			}
		}
		#endregion

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
#if DEBUG_ANNOTATE
						parameterDeclaration.AddAnnotation(invocationParameter);
#endif
					}
				}
			}
		}

#if DEBUG_ANNOTATE
		private void AnnotateInvocations(AstNode rootNode)
		{
			foreach (var invocationExpression in rootNode.DescendantsAndSelf.OfType<InvocationExpression>())
			{
				if (invocationExpression.GetSymbol() is IMethod method)
				{
					var invocationMethod = methodMap[method];
					var rr = invocationExpression.Annotation<CSharpInvocationResolveResult>();
					if (rr != null)
					{
						var argMap = rr.GetArgumentToParameterMap();
						foreach (var (index, argument) in invocationExpression.Arguments.WithIndex())
						{
							var invocationParameter = invocationMethod.GetParameter(argMap == null ? index : argMap[index]);
							if (invocationParameter is not null)
							{
								argument.AddAnnotation(invocationParameter);
							}
						}
					}
				}
			}
		}
#endif
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

		public override int VisitPrimitiveExpression(PrimitiveExpression primitiveExpression, SymbolicContext symbolicContext)
		{
			primitiveExpression.SaveContext(symbolicContext);
			return base.VisitPrimitiveExpression(primitiveExpression, symbolicContext);
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
#if DEBUG_ANNOTATE
			if (node is not PrimitiveExpression)
				node.SaveContext(symbolicContext);
#endif
			symbolicContext = node.HasSymbolicContext() ? symbolicContext.Ensure() : null;
		}

		private void ReplacePrimitiveWithSymbolic(PrimitiveExpression primitiveExpression, Bitfield bitfield)
		{
			if (primitiveExpression.Value is int intValue)
			{
				var bitValue = bitfield.Decompose(intValue);
				primitiveExpression.ReplaceWith(bitValue.Express(context).CopyAnnotationsFrom(primitiveExpression));
			}
		}

		private void ReplacePrimitiveExpressions(AstNode node)
		{
			foreach (var primitiveExpression in node.DescendantsAndSelf.OfType<PrimitiveExpression>())
			{
				var symbolicContext = primitiveExpression.Annotation<SymbolicContext>();
				if (symbolicContext is not null)
				{
					primitiveExpression.RemoveAnnotations<SymbolicContext>();
					var representation = symbolicContext.Representation;
					if (representation is not null)
					{
						switch (symbolicContext.Representation.Name)
						{
							case "LayerMask":
								ReplacePrimitiveWithSymbolic(primitiveExpression, layerMaskBitfield);
								break;
							case "HitMask":
								ReplacePrimitiveWithSymbolic(primitiveExpression, hitMaskBitfield);
								break;
						}
					}
				}
			}

		}


		TransformContext context;

		void IAstTransform.Run(AstNode node, TransformContext context)
		{
			this.context = context;
			try
			{
#if BITVALUE_STUFF
				CollectConstantDeclarations(node);
#endif
				BuildMethodMap(node);
#if DEBUG_ANNOTATE
				AnnotateInvocations(node);
#endif
				VisitChildren(node, null);
#if BITVALUE_STUFF
				ReplacePrimitiveExpressions(node);
#endif
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
				if (node.Annotation<SymbolicContext>() is not null)
					throw new ArgumentException($"Node {node} has already been assigned a symbolic context");

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

		public static bool IsIntegerConstant(this IVariable variable)
		{
			return variable is not null && variable.IsConst && variable.Type.IsKnownType(KnownTypeCode.Int32);
		}

		public static int IntegerConstantValue(this IVariable variable)
		{
			if (!variable.IsIntegerConstant())
				throw new ArgumentException($"{variable} is not an Int32 constant");
			return (int)variable.GetConstantValue(true);
		}

		public static Expression CreateTypeReference(this IType type, TransformContext context)
		{
			return new TypeReferenceExpression(context.TypeSystemAstBuilder.ConvertType(type))
				.WithoutILInstruction()
				.WithRR(new TypeResolveResult(type));
		}

		public static Expression CreateMemberReference(this IMember member, TransformContext context)
		{
			var target = member.DeclaringType.CreateTypeReference(context);
			return new MemberReferenceExpression(target, member.Name)
				.WithRR(new MemberResolveResult(target.GetResolveResult(), member));
		}

		public static Expression CreateIdentifierReference(this IMember member, TransformContext context)
		{
			return new IdentifierExpression(member.Name)
				.WithRR(new MemberResolveResult(new TypeResolveResult(member.DeclaringType), member));
		}

		public static Expression CreatePrimitive(this int value, TransformContext context)
		{
			return new PrimitiveExpression(value).WithCIRR(context, value);
		}

		public static Expression WithCIRR(this Expression expression, TransformContext context, int value)
		{
			return expression.WithoutILInstruction()
				.WithRR(new ConstantResolveResult(context.TypeSystem.FindType(KnownTypeCode.Int32), value));
		}
	}
	#endregion
}
