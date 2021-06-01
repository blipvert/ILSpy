//#define DEBUG_ANNOTATE_SYMBOLIC_CONTEXTS
//#define DEBUG_ANNOTATE_INVOCATIONS
//#define DEBUG_VERBOSE
#define BITVALUE_STUFF

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

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

	public class SymbolicRepresentationIncompatibleMerge : Exception
	{
		public SymbolicRepresentationIncompatibleMerge() { }
		public SymbolicRepresentationIncompatibleMerge(string message) : base(message) { }
	}

	public interface ISymbolicContext
	{
		abstract int ContextNumber { get; }
		abstract int? InferenceNumber { get; }
		abstract bool HasInference { get; }
		abstract string ContextNumberString { get; }
		abstract string RepresentationString { get; }
	}

	public interface IInvocationParameter
	{
		abstract int UniqueId { get; }
		abstract IParameter Parameter { get; }
		abstract ILVariable Variable { get; }
	}

	public class ReplaceLiteralsWithConstants : DepthFirstAstVisitor<ReplaceLiteralsWithConstants.SymbolicContext, int>, IAstTransform
	{
		private static int instanceCounter = 0;
		public readonly int instanceNumber;

		public ReplaceLiteralsWithConstants()
		{
			instanceNumber = ++instanceCounter;
#if DEBUG_VERBOSE
			Console.WriteLine($"ReplaceLiteralsWithConstants: Instance {instanceNumber} constructed");
#endif
		}

		#region SymbolicRepresentation
		public class SymbolicRepresentation
		{
			public readonly string Name;
			public SymbolicRepresentation(string name)
			{
				Name = name;
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

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		public class SymbolicContext : ISymbolicContext
		{
			public class Inference
			{
				private static int inferenceCount = 0;
				public SymbolicRepresentation Representation;
				public readonly int InferenceNumber;

				public Inference(SymbolicRepresentation representation = null)
				{
					Representation = representation;
					InferenceNumber = ++inferenceCount;
				}

				public void Merge(Inference other)
				{
					Representation = other.Representation = SymbolicRepresentation.Merge(Representation, other.Representation);
				}
			}

			public readonly TransformContext Context;
			public readonly MethodScope DeclaredMethod;

			private static int contextCount = 0;
			private Inference inference;
			private readonly int contextNumber;
			public int ContextNumber => contextNumber;
			public int? InferenceNumber => inference?.InferenceNumber;
			public bool HasInference => inference is not null;
			public string ContextNumberString => HasInference ? $"{contextNumber}/{inference.InferenceNumber}" : contextNumber.ToString();
			public string RepresentationString => Representation?.Name;

			private string DebuggerDisplay {
				get {
					string representationName = RepresentationString;
					if (representationName is null)
						return $"[Context {ContextNumberString}]";
					else
						return $"[Context {ContextNumberString} = {representationName}]";
				}
			}

			internal SymbolicContext(TransformContext context, MethodScope declaredMethod)
			{
				Context = context;
				DeclaredMethod = declaredMethod;
				contextNumber = ++contextCount;
			}

			public SymbolicContext Merge(Inference inference)
			{
				if (inference is not null)
				{
					if (this.inference is not null)
						this.inference.Merge(inference);
					this.inference = inference;
				}
				return this;
			}

			public SymbolicRepresentation Representation => inference?.Representation;
			public void SetRepresentation(SymbolicRepresentation representation, bool force = false)
			{
				if (inference is null)
					inference = new(representation);
				else if (inference.Representation == null || force)
				{
					inference.Representation = representation;
				}
			}
		}
		#endregion

		#region BitValue/BitValueExpression/Bitmask/Bitfield
		class BitValue
		{
			public virtual bool Inverted => false;
			public virtual BitValue UninvertedValue => this;
			public readonly int Complexity;

			public readonly int Value;
			public int BitCount => Value.BitCount();
			public bool Empty => Value == 0;
			public virtual IField Field => null;

			public BitValue(int value = 0, int complexity = 1)
			{
				Value = value;
				Complexity = complexity;
			}

			public virtual Expression Express(TransformContext context, IType currentType)
			{
				return new PrimitiveExpression(Value);
			}

			public virtual Expression Translate(TransformContext context, IType currentType)
			{
				return Express(context, currentType).WithCIRR(context, Value);
			}

			public virtual BitValue Group()
			{
				return this;
			}

			public virtual BitValue Invert()
			{
				return new InvertedBitmask(this);
			}

			public virtual BitValue Combine(BitValue other)
			{
				return Empty ? other : (other.Empty ? this : new CombinedBitmask(this, other));
			}
		}

		abstract class Bitmask : BitValue
		{
			protected readonly BitValue bitValue;

			public Bitmask(BitValue bv, int? value = null, int? complexity = null)
				: base(value ?? bv.Value, complexity ?? bv.Complexity)
			{
				bitValue = bv;
			}
		}

		class CombinedBitmask : Bitmask
		{
			protected readonly BitValue bitValue2;

			public CombinedBitmask(BitValue bv, BitValue bv2) : base(bv, bv.Value | bv2.Value, bv.Complexity + bv2.Complexity)
			{
				bitValue2 = bv2;
			}
			public override Expression Express(TransformContext context, IType currentType)
			{
				return
					new BinaryOperatorExpression(
						bitValue.Translate(context, currentType),
						BinaryOperatorType.BitwiseOr,
						bitValue2.Translate(context, currentType));
			}

			public override BitValue Group()
			{
				return new GroupedBitmask(this);
			}
		}

		class GroupedBitmask : Bitmask
		{
			public GroupedBitmask(BitValue bv) : base(bv)
			{
			}

			public override Expression Express(TransformContext context, IType currentType)
			{
				return new ParenthesizedExpression(bitValue.Translate(context, currentType));
			}
		}

		class InvertedBitmask : Bitmask
		{
			public override bool Inverted => true;
			public override BitValue UninvertedValue => bitValue;

			public InvertedBitmask(BitValue bv) : base(bv, ~bv.Value, bv.Complexity + 1)
			{
			}

			public override Expression Express(TransformContext context, IType currentType)
			{
				return new UnaryOperatorExpression(UnaryOperatorType.BitNot, bitValue.Translate(context, currentType));
			}

			public override BitValue Invert()
			{
				return bitValue;
			}
		}

		class NamedBitmask : Bitmask, IComparable<NamedBitmask>
		{
			public override IField Field => field;
			public BitValue Expansion => bitValue;

			private readonly IField field;

			public NamedBitmask(IField field, BitValue bitValue) : base(bitValue.Group(), complexity: 1)
			{
				this.field = field;
			}

			public int CompareTo(NamedBitmask other)
			{
				int d = other.BitCount.CompareTo(BitCount);
				if (d != 0)
					return d;
				return other.Value.CompareTo(Value);
			}

			public override Expression Express(TransformContext context, IType currentType)
			{
				return Field.CreateMemberReference(context, currentType);
			}

			public override Expression Translate(TransformContext context, IType currentType)
			{
				return Express(context, currentType).WithMemberRR(context, Field);
			}
		}

		class BitPosition : BitValue
		{
			public override IField Field => field;
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

			private Expression GetPositionExpression(TransformContext context, IType currentType)
			{
				return (Field is null) ? position.CreatePrimitive(context) : Field.CreateMemberReference(context, currentType);
			}

			public override Expression Express(TransformContext context, IType currentType)
			{
				return
					new ParenthesizedExpression(
						new BinaryOperatorExpression(
							1.CreatePrimitive(context),
							BinaryOperatorType.ShiftLeft,
							GetPositionExpression(context, currentType)).WithCIRR(context, Value));
			}
		}

		class Bitfield
		{
			private readonly BitPosition[] position = Enumerable.Range(0, 32).Select(x => new BitPosition(x)).ToArray();

			public BitPosition[] Position => position;

			public readonly SortedSet<NamedBitmask> masks = new();
			public readonly Dictionary<int, NamedBitmask> values = new();
			private readonly int? maxValue;
			public int FieldCount { get; private set; } = 0;
			public bool Empty => FieldCount == 0;

			public Bitfield(int? bitLength = null)
			{
				maxValue = (bitLength is null) ? null : ((1 << (int)bitLength) - 1);
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
					FieldCount++;
				}
				catch (Exception e)
				{
					throw e;
				}
			}

			private NamedBitmask AddMask(NamedBitmask bitmask, int value)
			{
				if (value != 0)
					masks.Add(bitmask);
				values.Add(value, bitmask);
				return bitmask;
			}

			public NamedBitmask AddMask(IField field)
			{
				FieldCount++;
				int value = field.IntegerConstantValue();
				return AddMask(new NamedBitmask(field, Translate(value)), value);
			}

			public BitValue Translate(int value)
			{
				if (values.TryGetValue(value, out var bitmask))
					return bitmask;
				if ((maxValue is not null) && value >= maxValue)
					return new BitValue(value);
				var bitValue = Decompose(value);
				var bitValueInv = Decompose(~value).Invert();
				return bitValueInv.Complexity < bitValue.Complexity ? bitValueInv : bitValue;
			}

			public IEnumerable<BitValue> DecomposeIter(int value)
			{
				foreach (var m in masks)
				{
					if (value == 0)
						yield break;
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

			public BitValue Decompose(int value)
			{
				return DecomposeIter(value).Aggregate(new BitValue(), (total, bv) => total.Combine(bv));
			}
		}
		#endregion

		#region Collecting Constant Declarations
		private Bitfield layerMaskBitfield;
		private Bitfield hitMaskBitfield;
		private Dictionary<IField, NamedBitmask> masterBitfieldDirectory = new();

		private Bitfield CreateSymbolicBitField(string definingType, string maskPrefix, string bitPositionPrefix = null, int? bitLength = null)
		{
			var type = transformContext.TypeSystem.MainModule.TopLevelTypeDefinitions.Where(t => t.Name == definingType).SingleOrDefault();
			if (type is not null)
			{
				Bitfield bitfield = new(bitLength);
				foreach (var field in type.Fields)
				{
					if (field.IsIntegerConstant())
					{
						if (field.Name.StartsWith(maskPrefix))
							masterBitfieldDirectory.Add(field, bitfield.AddMask(field));
						else if ((bitPositionPrefix is not null) && field.Name.StartsWith(bitPositionPrefix))
							bitfield.SetPosition(field);
					}
				}
				if (!bitfield.Empty)
					return bitfield;
			}
			return null;
		}


		private void PopulateSymbolicBitfields()
		{
			layerMaskBitfield = CreateSymbolicBitField("Constants", "cLayerMask", "cLayer");
			hitMaskBitfield = CreateSymbolicBitField("Voxel", "HM_", bitLength: 12);
		}

		#endregion

		#region InvocationMethod/InvocationParameter
		public class InvocationParameter
		{
			private static int counter = 0;
			public int UniqueId => uniqueId;
			private readonly int uniqueId;
			public IParameter Parameter => parameter;
			private ILVariable variable = null;
			private readonly IParameter parameter;
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
#if DEBUG_ANNOTATE_INVOCATIONS
						parameterDeclaration.AddAnnotation(invocationParameter);
#endif
					}
				}
			}
		}

#if DEBUG_ANNOTATE_INVOCATIONS
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

		private Dictionary<ILVariable, SymbolicContext.Inference> variableContextMap = new();

		private SymbolicContext GetVariableContext(ILVariable variable, SymbolicContext mergeContext = null)
		{
			if (variable is null)
				return mergeContext;

			if (!variableContextMap.TryGetValue(variable, out var inference))
			{
				variableContextMap.Add(variable, inference = new());
			}
			return Ensure(mergeContext).Merge(inference);
		}

		private readonly SymbolicRepresentation layerMaskSymbolicRepresentation = new("LayerMask");
		private readonly SymbolicRepresentation hitMaskSymbolicRepresentation = new("HitMask");

		private SymbolicRepresentation InferRepresentation(string name)
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
			var symbolicRepresentation = InferRepresentation(name);

			if (symbolicRepresentation is not null)
			{
				(symbolicContext = Ensure(symbolicContext)).SetRepresentation(symbolicRepresentation);
			}
		}

		public class MethodScope
		{
			private HashSet<string> localNames = new();

			public bool AddLocalName(string name)
			{
				return localNames.Add(name);
			}

			public string AddPrefixedName(string prefix)
			{
				int unique = 1;
				string name;

				while (!AddLocalName(name = prefix + unique.ToString()))
				{
					++unique;
				}
				return name;
			}
		}

		private MethodScope currentScope = null;

		private SymbolicContext CreateSymbolicContext()
		{
			return new(transformContext, currentScope);
		}

		private SymbolicContext Ensure(SymbolicContext symbolicContext)
		{
			return symbolicContext ?? CreateSymbolicContext();
		}

		public override int VisitTypeDeclaration(TypeDeclaration typeDeclaration, SymbolicContext symbolicContext)
		{
			var previousContext = transformContext.Retarget(typeDeclaration);
			try
			{
				return base.VisitTypeDeclaration(typeDeclaration, symbolicContext);
			}
			finally
			{
				transformContext = previousContext;
			}
		}

		public override int VisitMethodDeclaration(MethodDeclaration methodDeclaration, SymbolicContext symbolicContext)
		{
			var previousContext = transformContext.Retarget(methodDeclaration);
			var previousScope = currentScope;
			try
			{
				currentScope = new();
				return base.VisitMethodDeclaration(methodDeclaration, symbolicContext);
			}
			finally
			{
				currentScope = previousScope;
				transformContext = previousContext;
			}
		}

		public override int VisitParameterDeclaration(ParameterDeclaration parameterDeclaration, SymbolicContext symbolicContext)
		{
			currentScope?.AddLocalName(parameterDeclaration.Name);
			return base.VisitParameterDeclaration(parameterDeclaration, symbolicContext);
		}

		public override int VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement, SymbolicContext symbolicContext)
		{
			foreach (var variableInitializer in variableDeclarationStatement.Variables)
			{
				currentScope?.AddLocalName(variableInitializer.Name);
			}
			return base.VisitVariableDeclarationStatement(variableDeclarationStatement, symbolicContext);
		}

		public override int VisitFieldDeclaration(FieldDeclaration fieldDeclaration, SymbolicContext symbolicContext)
		{
			var field = fieldDeclaration.GetSymbol() as IField;
			if (masterBitfieldDirectory.TryGetValue(field, out var bitmask))
			{
				ModifyFieldDeclaration(fieldDeclaration, bitmask.Expansion);
			}

			return base.VisitFieldDeclaration(fieldDeclaration, symbolicContext);
		}

		public override int VisitIdentifier(Identifier identifier, SymbolicContext symbolicContext)
		{
			SetRepresentation(ref symbolicContext, identifier.Name);
			return base.VisitIdentifier(identifier, symbolicContext);
		}

		public override int VisitConditionalExpression(ConditionalExpression conditionalExpression, SymbolicContext symbolicContext)
		{
			UpdateSymbolicContextForNode(conditionalExpression, ref symbolicContext);
			conditionalExpression.Condition.AcceptVisitor(this, null);
			conditionalExpression.TrueExpression.AcceptVisitor(this, symbolicContext);
			conditionalExpression.FalseExpression.AcceptVisitor(this, symbolicContext);
			return default;
		}

		public override int VisitPrimitiveExpression(PrimitiveExpression primitiveExpression, SymbolicContext symbolicContext)
		{
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
								symbolicContext = GetVariableContext(invocationParameter.Variable, symbolicContext);
								SetRepresentation(ref symbolicContext, invocationParameter.Parameter.Name);
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
			var inheritedContext = symbolicContext = GetVariableContext(node.GetILVariable(), symbolicContext);
			symbolicContext = NeedsSymbolicContext(node) ? Ensure(symbolicContext) : null;
			node.SaveContext(inheritedContext ?? symbolicContext);
		}

		private static bool NeedsSymbolicContext(AstNode node)
		{
			return
				node is BinaryOperatorExpression binary && (binary.Operator.IsBitwise() || binary.Operator.IsEquality()) ||
				node is UnaryOperatorExpression unary && unary.Operator == UnaryOperatorType.BitNot ||
				node is AssignmentExpression assign && (assign.Operator == AssignmentOperatorType.Assign || assign.Operator.IsBitwise()) ||
				node is ConditionalExpression ||
				node is VariableInitializer ||
				node is MemberReferenceExpression ||
				node is CastExpression ||
				node is IdentifierExpression ||
				node is NamedArgumentExpression ||
				node is ParenthesizedExpression;
		}

		private void ReplacePrimitiveWithSymbolic(PrimitiveExpression primitiveExpression, BitValue bitValue)
		{
			primitiveExpression.ReplaceWith(bitValue.Express(transformContext, primitiveExpression.GetEnclosingType()).CopyAnnotationsFrom(primitiveExpression));
		}

		private void ReplacePrimitiveWithSymbolic(PrimitiveExpression primitiveExpression, Bitfield bitfield)
		{
			if ((primitiveExpression.Value is int intValue) && intValue != 0)
			{
				ReplacePrimitiveWithSymbolic(primitiveExpression, bitfield.Translate(intValue));
			}
		}

		private void ModifyFieldDeclaration(FieldDeclaration fieldDeclaration, BitValue bitValue)
		{
			foreach (var variable in fieldDeclaration.Variables)
			{
				if (variable.Initializer is PrimitiveExpression primitiveExpression)
					ReplacePrimitiveWithSymbolic(primitiveExpression, bitValue);
			}
		}

		private Bitfield GetSymbolicBitfield(SymbolicRepresentation symbolicRepresentation)
		{
			switch (symbolicRepresentation?.Name)
			{
				case "LayerMask":
					return layerMaskBitfield;
				case "HitMask":
					return hitMaskBitfield;
			}
			return null;
		}

		private void ReplacePrimitiveExpressions(AstNode node)
		{
			foreach (var primitiveExpression in node.DescendantsAndSelf.OfType<PrimitiveExpression>())
			{
				var symbolicContext = primitiveExpression.Annotation<SymbolicContext>();
				if (symbolicContext is not null)
				{
					var symbolicBitfield = GetSymbolicBitfield(symbolicContext.Representation);
					if (symbolicBitfield is not null)
						ReplacePrimitiveWithSymbolic(primitiveExpression, symbolicBitfield);
				}
			}

		}

		private void RenameSymbolicVariables(AstNode root)
		{
			void RenameIdentifier(AstNode node)
			{
				var variable = node.GetILVariable();
				if (variable is not null)
				{
					var symbolicContext = node.GetSymbolicContext() as SymbolicContext;
					var declaredMethod = symbolicContext.DeclaredMethod;
					if (declaredMethod is not null)
					{
						var prefix = symbolicContext?.RepresentationString?.ToLowerInvariant();
						if (prefix is not null && variable.Name.StartsWith("num"))
						{
							variable.Name = declaredMethod.AddPrefixedName(prefix);
						}

						var identifier = node.GetChildByRole(Roles.Identifier);

						if (identifier.Name != variable.Name)
							identifier.Name = variable.Name;
					}
				}
			}

			foreach (var variableDeclarationStatement in root.DescendantNodesAndSelf().OfType<VariableDeclarationStatement>())
			{
				foreach (var variableInitializer in variableDeclarationStatement.Variables)
				{
					RenameIdentifier(variableInitializer);
				}
			}

			foreach (var identifierExpression in root.DescendantNodesAndSelf().OfType<IdentifierExpression>())
			{
				RenameIdentifier(identifierExpression);
			}
		}

		private void CleanupSymbolicAnnotations(AstNode root)
		{
			foreach (var node in root.DescendantNodesAndSelf())
			{
				node.RemoveAnnotations<SymbolicContext>();
			}
		}

		TransformContext transformContext;

		void IAstTransform.Run(AstNode node, TransformContext transformContext)
		{
			this.transformContext = transformContext;
			try
			{
#if BITVALUE_STUFF
				PopulateSymbolicBitfields();
#endif
				BuildMethodMap(node);
#if DEBUG_ANNOTATE_INVOCATIONS
				AnnotateInvocations(node);
#endif
				VisitChildren(node, null);
#if BITVALUE_STUFF
				ReplacePrimitiveExpressions(node);
#endif
				RenameSymbolicVariables(node);
#if !DEBUG_ANNOTATE_SYMBOLIC_CONTEXTS
				CleanupSymbolicAnnotations(node);
#endif
			}
			finally
			{
				this.transformContext = null;
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

		public static ILVariable GetILVariable(this AstNode node)
		{
			if (node.Annotation<ResolveResult>() is ILVariableResolveResult rr)
				return rr.Variable;
			else
				return null;
		}

		public static void SaveContext(this AstNode node, ISymbolicContext symbolicContext)
		{
			if (symbolicContext != null)
			{
				if (node.Annotation<ISymbolicContext>() is not null)
					throw new ArgumentException($"Node {node} has already been assigned a symbolic context");

				node.AddAnnotation(symbolicContext);
			}
		}

		public static ISymbolicContext GetSymbolicContext(this AstNode node)
		{
			return node.Annotation<ISymbolicContext>();
		}

		public static bool IsEquality(this BinaryOperatorType operatorType)
		{
			return operatorType == BinaryOperatorType.Equality || operatorType == BinaryOperatorType.InEquality;
		}

		public static bool IsBitwise(this AssignmentOperatorType operatorType)
		{
			return operatorType == AssignmentOperatorType.BitwiseAnd
				|| operatorType == AssignmentOperatorType.BitwiseOr
				|| operatorType == AssignmentOperatorType.ExclusiveOr;
		}

		public static TransformContext Retarget(this TransformContext transformContext, IEntity entity)
		{
			return entity is null ? transformContext :
				new(transformContext.TypeSystem, transformContext.DecompileRun, new SimpleTypeResolveContext(entity), transformContext.TypeSystemAstBuilder);
		}

		public static TransformContext Retarget(this TransformContext transformContext, AstNode node)
		{
			return transformContext.Retarget(node.GetSymbol() as IEntity);
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

		public static T GetAncestor<T>(this AstNode node)
			where T : AstNode
		{
			while ((node = node.Parent) is not null)
			{
				if (node is T parent)
					return parent;
			}
			return null;
		}

		public static ISymbol GetEnclosing<T>(this AstNode node)
			where T : AstNode
		{
			return node.GetAncestor<T>()?.GetSymbol();
		}

		public static TypeDeclaration GetDeclaringType(this AstNode node)
		{
			return node.GetAncestor<TypeDeclaration>();
		}

		public static IType GetEnclosingType(this AstNode node)
		{
			return node.GetEnclosing<TypeDeclaration>() as IType;
		}

		public static Expression CreateMemberReference(this IMember member, TransformContext context, IType currentType)
		{
			var declaringType = member.DeclaringType;
			if (declaringType == currentType)
				return new IdentifierExpression(member.Name);

			return new MemberReferenceExpression(
				new TypeReferenceExpression(context.TypeSystemAstBuilder.ConvertType(declaringType))
							.WithRR(new TypeResolveResult(declaringType)), member.Name);
		}

		public static Expression WithMemberRR(this Expression expr, TransformContext context, IMember m)
		{
			return expr
				.WithRR(new MemberResolveResult(
					(expr is MemberReferenceExpression mre) ? mre.Target.GetResolveResult() : new TypeResolveResult(m.DeclaringType), m));
		}

		public static Expression CreatePrimitive(this int value, TransformContext context)
		{
			return new PrimitiveExpression(value).WithCIRR(context, value);
		}

		public static Expression WithCIRR(this Expression expression, TransformContext context, int value)
		{
			return expression
				.WithRR(new ConstantResolveResult(context.TypeSystem.FindType(KnownTypeCode.Int32), value));
		}
	}
	#endregion
}
