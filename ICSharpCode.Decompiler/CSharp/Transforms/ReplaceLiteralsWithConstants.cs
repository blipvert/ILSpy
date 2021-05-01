
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

	class ReplaceLiteralsWithConstants : DepthFirstAstVisitor, IAstTransform
	{
        TransformContext context;

        void IAstTransform.Run(AstNode node, TransformContext context)
		{
#if false
			this.context = context;
			try
			{
			}
			finally
			{
				this.context = null;
			}
#endif
		}
	}
}
