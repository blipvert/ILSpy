// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.


namespace ICSharpCode.Decompiler.CSharp.Syntax
{
	/// <summary>
	/// AST visitor.
	/// </summary>
	public abstract class DefaultAstVisitor : IAstVisitor
	{
		public virtual void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression) {}
		public virtual void VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression) {}
		public virtual void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression) {}
		public virtual void VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression) {}
		public virtual void VisitAsExpression(AsExpression asExpression) {}
		public virtual void VisitAssignmentExpression(AssignmentExpression assignmentExpression) {}
		public virtual void VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression) {}
		public virtual void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression) {}
		public virtual void VisitCastExpression(CastExpression castExpression) {}
		public virtual void VisitCheckedExpression(CheckedExpression checkedExpression) {}
		public virtual void VisitConditionalExpression(ConditionalExpression conditionalExpression) {}
		public virtual void VisitDeclarationExpression(DeclarationExpression declarationExpression) {}
		public virtual void VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression) {}
		public virtual void VisitDirectionExpression(DirectionExpression directionExpression) {}
		public virtual void VisitIdentifierExpression(IdentifierExpression identifierExpression) {}
		public virtual void VisitIndexerExpression(IndexerExpression indexerExpression) {}
		public virtual void VisitInterpolatedStringExpression(InterpolatedStringExpression interpolatedStringExpression) {}
		public virtual void VisitInvocationExpression(InvocationExpression invocationExpression) {}
		public virtual void VisitIsExpression(IsExpression isExpression) {}
		public virtual void VisitLambdaExpression(LambdaExpression lambdaExpression) {}
		public virtual void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression) {}
		public virtual void VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression) {}
		public virtual void VisitNamedExpression(NamedExpression namedExpression) {}
		public virtual void VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression) {}
		public virtual void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression) {}
		public virtual void VisitOutVarDeclarationExpression(OutVarDeclarationExpression outVarDeclarationExpression) {}
		public virtual void VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression) {}
		public virtual void VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression) {}
		public virtual void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression) {}
		public virtual void VisitSizeOfExpression(SizeOfExpression sizeOfExpression) {}
		public virtual void VisitStackAllocExpression(StackAllocExpression stackAllocExpression) {}
		public virtual void VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression) {}
		public virtual void VisitThrowExpression(ThrowExpression throwExpression) {}
		public virtual void VisitTupleExpression(TupleExpression tupleExpression) {}
		public virtual void VisitTypeOfExpression(TypeOfExpression typeOfExpression) {}
		public virtual void VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression) {}
		public virtual void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression) {}
		public virtual void VisitUncheckedExpression(UncheckedExpression uncheckedExpression) {}
		public virtual void VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression) {}
		public virtual void VisitWithInitializerExpression(WithInitializerExpression withInitializerExpression) {}

		public virtual void VisitQueryExpression(QueryExpression queryExpression) {}
		public virtual void VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause) {}
		public virtual void VisitQueryFromClause(QueryFromClause queryFromClause) {}
		public virtual void VisitQueryLetClause(QueryLetClause queryLetClause) {}
		public virtual void VisitQueryWhereClause(QueryWhereClause queryWhereClause) {}
		public virtual void VisitQueryJoinClause(QueryJoinClause queryJoinClause) {}
		public virtual void VisitQueryOrderClause(QueryOrderClause queryOrderClause) {}
		public virtual void VisitQueryOrdering(QueryOrdering queryOrdering) {}
		public virtual void VisitQuerySelectClause(QuerySelectClause querySelectClause) {}
		public virtual void VisitQueryGroupClause(QueryGroupClause queryGroupClause) {}

		public virtual void VisitAttribute(Attribute attribute) {}
		public virtual void VisitAttributeSection(AttributeSection attributeSection) {}
		public virtual void VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration) {}
		public virtual void VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration) {}
		public virtual void VisitTypeDeclaration(TypeDeclaration typeDeclaration) {}
		public virtual void VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration) {}
		public virtual void VisitUsingDeclaration(UsingDeclaration usingDeclaration) {}
		public virtual void VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration) {}

		public virtual void VisitBlockStatement(BlockStatement blockStatement) {}
		public virtual void VisitBreakStatement(BreakStatement breakStatement) {}
		public virtual void VisitCheckedStatement(CheckedStatement checkedStatement) {}
		public virtual void VisitContinueStatement(ContinueStatement continueStatement) {}
		public virtual void VisitDoWhileStatement(DoWhileStatement doWhileStatement) {}
		public virtual void VisitEmptyStatement(EmptyStatement emptyStatement) {}
		public virtual void VisitExpressionStatement(ExpressionStatement expressionStatement) {}
		public virtual void VisitFixedStatement(FixedStatement fixedStatement) {}
		public virtual void VisitForeachStatement(ForeachStatement foreachStatement) {}
		public virtual void VisitForStatement(ForStatement forStatement) {}
		public virtual void VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement) {}
		public virtual void VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement) {}
		public virtual void VisitGotoStatement(GotoStatement gotoStatement) {}
		public virtual void VisitIfElseStatement(IfElseStatement ifElseStatement) {}
		public virtual void VisitLabelStatement(LabelStatement labelStatement) {}
		public virtual void VisitLockStatement(LockStatement lockStatement) {}
		public virtual void VisitReturnStatement(ReturnStatement returnStatement) {}
		public virtual void VisitSwitchStatement(SwitchStatement switchStatement) {}
		public virtual void VisitSwitchSection(SwitchSection switchSection) {}
		public virtual void VisitCaseLabel(CaseLabel caseLabel) {}
		public virtual void VisitSwitchExpression(SwitchExpression switchExpression) {}
		public virtual void VisitSwitchExpressionSection(SwitchExpressionSection switchExpressionSection) {}
		public virtual void VisitThrowStatement(ThrowStatement throwStatement) {}
		public virtual void VisitTryCatchStatement(TryCatchStatement tryCatchStatement) {}
		public virtual void VisitCatchClause(CatchClause catchClause) {}
		public virtual void VisitUncheckedStatement(UncheckedStatement uncheckedStatement) {}
		public virtual void VisitUnsafeStatement(UnsafeStatement unsafeStatement) {}
		public virtual void VisitUsingStatement(UsingStatement usingStatement) {}
		public virtual void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement) {}
		public virtual void VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatement localFunctionDeclarationStatement) {}
		public virtual void VisitWhileStatement(WhileStatement whileStatement) {}
		public virtual void VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement) {}
		public virtual void VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement) {}

		public virtual void VisitAccessor(Accessor accessor) {}
		public virtual void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration) {}
		public virtual void VisitConstructorInitializer(ConstructorInitializer constructorInitializer) {}
		public virtual void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration) {}
		public virtual void VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration) {}
		public virtual void VisitEventDeclaration(EventDeclaration eventDeclaration) {}
		public virtual void VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration) {}
		public virtual void VisitFieldDeclaration(FieldDeclaration fieldDeclaration) {}
		public virtual void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration) {}
		public virtual void VisitMethodDeclaration(MethodDeclaration methodDeclaration) {}
		public virtual void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration) {}
		public virtual void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration) {}
		public virtual void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration) {}
		public virtual void VisitVariableInitializer(VariableInitializer variableInitializer) {}
		public virtual void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration) {}
		public virtual void VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer) {}

		public virtual void VisitSyntaxTree(SyntaxTree syntaxTree) {}
		public virtual void VisitSimpleType(SimpleType simpleType) {}
		public virtual void VisitMemberType(MemberType memberType) {}
		public virtual void VisitTupleType(TupleAstType tupleType) {}
		public virtual void VisitTupleTypeElement(TupleTypeElement tupleTypeElement) {}
		public virtual void VisitFunctionPointerType(FunctionPointerAstType functionPointerType) {}
		public virtual void VisitInvocationType(InvocationAstType invocationType) {}
		public virtual void VisitComposedType(ComposedType composedType) {}
		public virtual void VisitArraySpecifier(ArraySpecifier arraySpecifier) {}
		public virtual void VisitPrimitiveType(PrimitiveType primitiveType) {}

		public virtual void VisitComment(Comment comment) {}
		public virtual void VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective) {}
		public virtual void VisitDocumentationReference(DocumentationReference documentationReference) {}

		public virtual void VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration) {}
		public virtual void VisitConstraint(Constraint constraint) {}
		public virtual void VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode) {}
		public virtual void VisitIdentifier(Identifier identifier) {}

		public virtual void VisitInterpolation(Interpolation interpolation) {}
		public virtual void VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText) {}

		public virtual void VisitSingleVariableDesignation(SingleVariableDesignation singleVariableDesignation) {}
		public virtual void VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignation parenthesizedVariableDesignation) {}

		public virtual void VisitNullNode(AstNode nullNode) {}
		public virtual void VisitErrorNode(AstNode errorNode) {}
		public virtual void VisitPatternPlaceholder(AstNode placeholder, PatternMatching.Pattern pattern) {}
	}

	/// <summary>
	/// AST visitor.
	/// </summary>
	public abstract class DefaultAstVisitor<S> : IAstVisitor<S>
	{
		public virtual S VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression) { return default; }
		public virtual S VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression) { return default; }
		public virtual S VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression) { return default; }
		public virtual S VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression) { return default; }
		public virtual S VisitAsExpression(AsExpression asExpression) { return default; }
		public virtual S VisitAssignmentExpression(AssignmentExpression assignmentExpression) { return default; }
		public virtual S VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression) { return default; }
		public virtual S VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression) { return default; }
		public virtual S VisitCastExpression(CastExpression castExpression) { return default; }
		public virtual S VisitCheckedExpression(CheckedExpression checkedExpression) { return default; }
		public virtual S VisitConditionalExpression(ConditionalExpression conditionalExpression) { return default; }
		public virtual S VisitDeclarationExpression(DeclarationExpression declarationExpression) { return default; }
		public virtual S VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression) { return default; }
		public virtual S VisitDirectionExpression(DirectionExpression directionExpression) { return default; }
		public virtual S VisitIdentifierExpression(IdentifierExpression identifierExpression) { return default; }
		public virtual S VisitIndexerExpression(IndexerExpression indexerExpression) { return default; }
		public virtual S VisitInterpolatedStringExpression(InterpolatedStringExpression interpolatedStringExpression) { return default; }
		public virtual S VisitInvocationExpression(InvocationExpression invocationExpression) { return default; }
		public virtual S VisitIsExpression(IsExpression isExpression) { return default; }
		public virtual S VisitLambdaExpression(LambdaExpression lambdaExpression) { return default; }
		public virtual S VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression) { return default; }
		public virtual S VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression) { return default; }
		public virtual S VisitNamedExpression(NamedExpression namedExpression) { return default; }
		public virtual S VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression) { return default; }
		public virtual S VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression) { return default; }
		public virtual S VisitOutVarDeclarationExpression(OutVarDeclarationExpression outVarDeclarationExpression) { return default; }
		public virtual S VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression) { return default; }
		public virtual S VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression) { return default; }
		public virtual S VisitPrimitiveExpression(PrimitiveExpression primitiveExpression) { return default; }
		public virtual S VisitSizeOfExpression(SizeOfExpression sizeOfExpression) { return default; }
		public virtual S VisitStackAllocExpression(StackAllocExpression stackAllocExpression) { return default; }
		public virtual S VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression) { return default; }
		public virtual S VisitThrowExpression(ThrowExpression throwExpression) { return default; }
		public virtual S VisitTupleExpression(TupleExpression tupleExpression) { return default; }
		public virtual S VisitTypeOfExpression(TypeOfExpression typeOfExpression) { return default; }
		public virtual S VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression) { return default; }
		public virtual S VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression) { return default; }
		public virtual S VisitUncheckedExpression(UncheckedExpression uncheckedExpression) { return default; }
		public virtual S VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression) { return default; }
		public virtual S VisitWithInitializerExpression(WithInitializerExpression withInitializerExpression) { return default; }

		public virtual S VisitQueryExpression(QueryExpression queryExpression) { return default; }
		public virtual S VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause) { return default; }
		public virtual S VisitQueryFromClause(QueryFromClause queryFromClause) { return default; }
		public virtual S VisitQueryLetClause(QueryLetClause queryLetClause) { return default; }
		public virtual S VisitQueryWhereClause(QueryWhereClause queryWhereClause) { return default; }
		public virtual S VisitQueryJoinClause(QueryJoinClause queryJoinClause) { return default; }
		public virtual S VisitQueryOrderClause(QueryOrderClause queryOrderClause) { return default; }
		public virtual S VisitQueryOrdering(QueryOrdering queryOrdering) { return default; }
		public virtual S VisitQuerySelectClause(QuerySelectClause querySelectClause) { return default; }
		public virtual S VisitQueryGroupClause(QueryGroupClause queryGroupClause) { return default; }

		public virtual S VisitAttribute(Attribute attribute) { return default; }
		public virtual S VisitAttributeSection(AttributeSection attributeSection) { return default; }
		public virtual S VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration) { return default; }
		public virtual S VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration) { return default; }
		public virtual S VisitTypeDeclaration(TypeDeclaration typeDeclaration) { return default; }
		public virtual S VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration) { return default; }
		public virtual S VisitUsingDeclaration(UsingDeclaration usingDeclaration) { return default; }
		public virtual S VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration) { return default; }

		public virtual S VisitBlockStatement(BlockStatement blockStatement) { return default; }
		public virtual S VisitBreakStatement(BreakStatement breakStatement) { return default; }
		public virtual S VisitCheckedStatement(CheckedStatement checkedStatement) { return default; }
		public virtual S VisitContinueStatement(ContinueStatement continueStatement) { return default; }
		public virtual S VisitDoWhileStatement(DoWhileStatement doWhileStatement) { return default; }
		public virtual S VisitEmptyStatement(EmptyStatement emptyStatement) { return default; }
		public virtual S VisitExpressionStatement(ExpressionStatement expressionStatement) { return default; }
		public virtual S VisitFixedStatement(FixedStatement fixedStatement) { return default; }
		public virtual S VisitForeachStatement(ForeachStatement foreachStatement) { return default; }
		public virtual S VisitForStatement(ForStatement forStatement) { return default; }
		public virtual S VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement) { return default; }
		public virtual S VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement) { return default; }
		public virtual S VisitGotoStatement(GotoStatement gotoStatement) { return default; }
		public virtual S VisitIfElseStatement(IfElseStatement ifElseStatement) { return default; }
		public virtual S VisitLabelStatement(LabelStatement labelStatement) { return default; }
		public virtual S VisitLockStatement(LockStatement lockStatement) { return default; }
		public virtual S VisitReturnStatement(ReturnStatement returnStatement) { return default; }
		public virtual S VisitSwitchStatement(SwitchStatement switchStatement) { return default; }
		public virtual S VisitSwitchSection(SwitchSection switchSection) { return default; }
		public virtual S VisitCaseLabel(CaseLabel caseLabel) { return default; }
		public virtual S VisitSwitchExpression(SwitchExpression switchExpression) { return default; }
		public virtual S VisitSwitchExpressionSection(SwitchExpressionSection switchExpressionSection) { return default; }
		public virtual S VisitThrowStatement(ThrowStatement throwStatement) { return default; }
		public virtual S VisitTryCatchStatement(TryCatchStatement tryCatchStatement) { return default; }
		public virtual S VisitCatchClause(CatchClause catchClause) { return default; }
		public virtual S VisitUncheckedStatement(UncheckedStatement uncheckedStatement) { return default; }
		public virtual S VisitUnsafeStatement(UnsafeStatement unsafeStatement) { return default; }
		public virtual S VisitUsingStatement(UsingStatement usingStatement) { return default; }
		public virtual S VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement) { return default; }
		public virtual S VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatement localFunctionDeclarationStatement) { return default; }
		public virtual S VisitWhileStatement(WhileStatement whileStatement) { return default; }
		public virtual S VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement) { return default; }
		public virtual S VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement) { return default; }

		public virtual S VisitAccessor(Accessor accessor) { return default; }
		public virtual S VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration) { return default; }
		public virtual S VisitConstructorInitializer(ConstructorInitializer constructorInitializer) { return default; }
		public virtual S VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration) { return default; }
		public virtual S VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration) { return default; }
		public virtual S VisitEventDeclaration(EventDeclaration eventDeclaration) { return default; }
		public virtual S VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration) { return default; }
		public virtual S VisitFieldDeclaration(FieldDeclaration fieldDeclaration) { return default; }
		public virtual S VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration) { return default; }
		public virtual S VisitMethodDeclaration(MethodDeclaration methodDeclaration) { return default; }
		public virtual S VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration) { return default; }
		public virtual S VisitParameterDeclaration(ParameterDeclaration parameterDeclaration) { return default; }
		public virtual S VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration) { return default; }
		public virtual S VisitVariableInitializer(VariableInitializer variableInitializer) { return default; }
		public virtual S VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration) { return default; }
		public virtual S VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer) { return default; }

		public virtual S VisitSyntaxTree(SyntaxTree syntaxTree) { return default; }
		public virtual S VisitSimpleType(SimpleType simpleType) { return default; }
		public virtual S VisitMemberType(MemberType memberType) { return default; }
		public virtual S VisitTupleType(TupleAstType tupleType) { return default; }
		public virtual S VisitTupleTypeElement(TupleTypeElement tupleTypeElement) { return default; }
		public virtual S VisitFunctionPointerType(FunctionPointerAstType functionPointerType) { return default; }
		public virtual S VisitInvocationType(InvocationAstType invocationType) { return default; }
		public virtual S VisitComposedType(ComposedType composedType) { return default; }
		public virtual S VisitArraySpecifier(ArraySpecifier arraySpecifier) { return default; }
		public virtual S VisitPrimitiveType(PrimitiveType primitiveType) { return default; }

		public virtual S VisitComment(Comment comment) { return default; }
		public virtual S VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective) { return default; }
		public virtual S VisitDocumentationReference(DocumentationReference documentationReference) { return default; }

		public virtual S VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration) { return default; }
		public virtual S VisitConstraint(Constraint constraint) { return default; }
		public virtual S VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode) { return default; }
		public virtual S VisitIdentifier(Identifier identifier) { return default; }

		public virtual S VisitInterpolation(Interpolation interpolation) { return default; }
		public virtual S VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText) { return default; }

		public virtual S VisitSingleVariableDesignation(SingleVariableDesignation singleVariableDesignation) { return default; }
		public virtual S VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignation parenthesizedVariableDesignation) { return default; }

		public virtual S VisitNullNode(AstNode nullNode) { return default; }
		public virtual S VisitErrorNode(AstNode errorNode) { return default; }
		public virtual S VisitPatternPlaceholder(AstNode placeholder, PatternMatching.Pattern pattern) { return default; }
	}

	/// <summary>
	/// AST visitor.
	/// </summary>
	public abstract class DefaultAstVisitor<T, S> : IAstVisitor<T, S>
	{
		public virtual S VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, T data) { return default; }
		public virtual S VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression, T data) { return default; }
		public virtual S VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression, T data) { return default; }
		public virtual S VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression, T data) { return default; }
		public virtual S VisitAsExpression(AsExpression asExpression, T data) { return default; }
		public virtual S VisitAssignmentExpression(AssignmentExpression assignmentExpression, T data) { return default; }
		public virtual S VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression, T data) { return default; }
		public virtual S VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, T data) { return default; }
		public virtual S VisitCastExpression(CastExpression castExpression, T data) { return default; }
		public virtual S VisitCheckedExpression(CheckedExpression checkedExpression, T data) { return default; }
		public virtual S VisitConditionalExpression(ConditionalExpression conditionalExpression, T data) { return default; }
		public virtual S VisitDeclarationExpression(DeclarationExpression declarationExpression, T data) { return default; }
		public virtual S VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression, T data) { return default; }
		public virtual S VisitDirectionExpression(DirectionExpression directionExpression, T data) { return default; }
		public virtual S VisitIdentifierExpression(IdentifierExpression identifierExpression, T data) { return default; }
		public virtual S VisitIndexerExpression(IndexerExpression indexerExpression, T data) { return default; }
		public virtual S VisitInterpolatedStringExpression(InterpolatedStringExpression interpolatedStringExpression, T data) { return default; }
		public virtual S VisitInvocationExpression(InvocationExpression invocationExpression, T data) { return default; }
		public virtual S VisitIsExpression(IsExpression isExpression, T data) { return default; }
		public virtual S VisitLambdaExpression(LambdaExpression lambdaExpression, T data) { return default; }
		public virtual S VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression, T data) { return default; }
		public virtual S VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression, T data) { return default; }
		public virtual S VisitNamedExpression(NamedExpression namedExpression, T data) { return default; }
		public virtual S VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression, T data) { return default; }
		public virtual S VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, T data) { return default; }
		public virtual S VisitOutVarDeclarationExpression(OutVarDeclarationExpression outVarDeclarationExpression, T data) { return default; }
		public virtual S VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression, T data) { return default; }
		public virtual S VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression, T data) { return default; }
		public virtual S VisitPrimitiveExpression(PrimitiveExpression primitiveExpression, T data) { return default; }
		public virtual S VisitSizeOfExpression(SizeOfExpression sizeOfExpression, T data) { return default; }
		public virtual S VisitStackAllocExpression(StackAllocExpression stackAllocExpression, T data) { return default; }
		public virtual S VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression, T data) { return default; }
		public virtual S VisitThrowExpression(ThrowExpression throwExpression, T data) { return default; }
		public virtual S VisitTupleExpression(TupleExpression tupleExpression, T data) { return default; }
		public virtual S VisitTypeOfExpression(TypeOfExpression typeOfExpression, T data) { return default; }
		public virtual S VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression, T data) { return default; }
		public virtual S VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, T data) { return default; }
		public virtual S VisitUncheckedExpression(UncheckedExpression uncheckedExpression, T data) { return default; }
		public virtual S VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression, T data) { return default; }
		public virtual S VisitWithInitializerExpression(WithInitializerExpression withInitializerExpression, T data) { return default; }

		public virtual S VisitQueryExpression(QueryExpression queryExpression, T data) { return default; }
		public virtual S VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause, T data) { return default; }
		public virtual S VisitQueryFromClause(QueryFromClause queryFromClause, T data) { return default; }
		public virtual S VisitQueryLetClause(QueryLetClause queryLetClause, T data) { return default; }
		public virtual S VisitQueryWhereClause(QueryWhereClause queryWhereClause, T data) { return default; }
		public virtual S VisitQueryJoinClause(QueryJoinClause queryJoinClause, T data) { return default; }
		public virtual S VisitQueryOrderClause(QueryOrderClause queryOrderClause, T data) { return default; }
		public virtual S VisitQueryOrdering(QueryOrdering queryOrdering, T data) { return default; }
		public virtual S VisitQuerySelectClause(QuerySelectClause querySelectClause, T data) { return default; }
		public virtual S VisitQueryGroupClause(QueryGroupClause queryGroupClause, T data) { return default; }

		public virtual S VisitAttribute(Attribute attribute, T data) { return default; }
		public virtual S VisitAttributeSection(AttributeSection attributeSection, T data) { return default; }
		public virtual S VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration, T data) { return default; }
		public virtual S VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, T data) { return default; }
		public virtual S VisitTypeDeclaration(TypeDeclaration typeDeclaration, T data) { return default; }
		public virtual S VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration, T data) { return default; }
		public virtual S VisitUsingDeclaration(UsingDeclaration usingDeclaration, T data) { return default; }
		public virtual S VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration, T data) { return default; }

		public virtual S VisitBlockStatement(BlockStatement blockStatement, T data) { return default; }
		public virtual S VisitBreakStatement(BreakStatement breakStatement, T data) { return default; }
		public virtual S VisitCheckedStatement(CheckedStatement checkedStatement, T data) { return default; }
		public virtual S VisitContinueStatement(ContinueStatement continueStatement, T data) { return default; }
		public virtual S VisitDoWhileStatement(DoWhileStatement doWhileStatement, T data) { return default; }
		public virtual S VisitEmptyStatement(EmptyStatement emptyStatement, T data) { return default; }
		public virtual S VisitExpressionStatement(ExpressionStatement expressionStatement, T data) { return default; }
		public virtual S VisitFixedStatement(FixedStatement fixedStatement, T data) { return default; }
		public virtual S VisitForeachStatement(ForeachStatement foreachStatement, T data) { return default; }
		public virtual S VisitForStatement(ForStatement forStatement, T data) { return default; }
		public virtual S VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement, T data) { return default; }
		public virtual S VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement, T data) { return default; }
		public virtual S VisitGotoStatement(GotoStatement gotoStatement, T data) { return default; }
		public virtual S VisitIfElseStatement(IfElseStatement ifElseStatement, T data) { return default; }
		public virtual S VisitLabelStatement(LabelStatement labelStatement, T data) { return default; }
		public virtual S VisitLockStatement(LockStatement lockStatement, T data) { return default; }
		public virtual S VisitReturnStatement(ReturnStatement returnStatement, T data) { return default; }
		public virtual S VisitSwitchStatement(SwitchStatement switchStatement, T data) { return default; }
		public virtual S VisitSwitchSection(SwitchSection switchSection, T data) { return default; }
		public virtual S VisitCaseLabel(CaseLabel caseLabel, T data) { return default; }
		public virtual S VisitSwitchExpression(SwitchExpression switchExpression, T data) { return default; }
		public virtual S VisitSwitchExpressionSection(SwitchExpressionSection switchExpressionSection, T data) { return default; }
		public virtual S VisitThrowStatement(ThrowStatement throwStatement, T data) { return default; }
		public virtual S VisitTryCatchStatement(TryCatchStatement tryCatchStatement, T data) { return default; }
		public virtual S VisitCatchClause(CatchClause catchClause, T data) { return default; }
		public virtual S VisitUncheckedStatement(UncheckedStatement uncheckedStatement, T data) { return default; }
		public virtual S VisitUnsafeStatement(UnsafeStatement unsafeStatement, T data) { return default; }
		public virtual S VisitUsingStatement(UsingStatement usingStatement, T data) { return default; }
		public virtual S VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement, T data) { return default; }
		public virtual S VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatement localFunctionDeclarationStatement, T data) { return default; }
		public virtual S VisitWhileStatement(WhileStatement whileStatement, T data) { return default; }
		public virtual S VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement, T data) { return default; }
		public virtual S VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement, T data) { return default; }

		public virtual S VisitAccessor(Accessor accessor, T data) { return default; }
		public virtual S VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, T data) { return default; }
		public virtual S VisitConstructorInitializer(ConstructorInitializer constructorInitializer, T data) { return default; }
		public virtual S VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration, T data) { return default; }
		public virtual S VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration, T data) { return default; }
		public virtual S VisitEventDeclaration(EventDeclaration eventDeclaration, T data) { return default; }
		public virtual S VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration, T data) { return default; }
		public virtual S VisitFieldDeclaration(FieldDeclaration fieldDeclaration, T data) { return default; }
		public virtual S VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration, T data) { return default; }
		public virtual S VisitMethodDeclaration(MethodDeclaration methodDeclaration, T data) { return default; }
		public virtual S VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration, T data) { return default; }
		public virtual S VisitParameterDeclaration(ParameterDeclaration parameterDeclaration, T data) { return default; }
		public virtual S VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, T data) { return default; }
		public virtual S VisitVariableInitializer(VariableInitializer variableInitializer, T data) { return default; }
		public virtual S VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration, T data) { return default; }
		public virtual S VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer, T data) { return default; }

		public virtual S VisitSyntaxTree(SyntaxTree syntaxTree, T data) { return default; }
		public virtual S VisitSimpleType(SimpleType simpleType, T data) { return default; }
		public virtual S VisitMemberType(MemberType memberType, T data) { return default; }
		public virtual S VisitTupleType(TupleAstType tupleType, T data) { return default; }
		public virtual S VisitTupleTypeElement(TupleTypeElement tupleTypeElement, T data) { return default; }
		public virtual S VisitFunctionPointerType(FunctionPointerAstType functionPointerType, T data) { return default; }
		public virtual S VisitInvocationType(InvocationAstType invocationType, T data) { return default; }
		public virtual S VisitComposedType(ComposedType composedType, T data) { return default; }
		public virtual S VisitArraySpecifier(ArraySpecifier arraySpecifier, T data) { return default; }
		public virtual S VisitPrimitiveType(PrimitiveType primitiveType, T data) { return default; }

		public virtual S VisitComment(Comment comment, T data) { return default; }
		public virtual S VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective, T data) { return default; }
		public virtual S VisitDocumentationReference(DocumentationReference documentationReference, T data) { return default; }

		public virtual S VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration, T data) { return default; }
		public virtual S VisitConstraint(Constraint constraint, T data) { return default; }
		public virtual S VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode, T data) { return default; }
		public virtual S VisitIdentifier(Identifier identifier, T data) { return default; }

		public virtual S VisitInterpolation(Interpolation interpolation, T data) { return default; }
		public virtual S VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText, T data) { return default; }

		public virtual S VisitSingleVariableDesignation(SingleVariableDesignation singleVariableDesignation, T data) { return default; }
		public virtual S VisitParenthesizedVariableDesignation(ParenthesizedVariableDesignation parenthesizedVariableDesignation, T data) { return default; }

		public virtual S VisitNullNode(AstNode nullNode, T data) { return default; }
		public virtual S VisitErrorNode(AstNode errorNode, T data) { return default; }
		public virtual S VisitPatternPlaceholder(AstNode placeholder, PatternMatching.Pattern pattern, T data) { return default; }
	}
}
