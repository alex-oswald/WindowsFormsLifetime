using Microsoft.CodeAnalysis;
using System;

namespace WindowsFormsLifetime.Mvp.Generators
{
    [Generator]
    public class MyGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            IMethodSymbol mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }
    }
}
