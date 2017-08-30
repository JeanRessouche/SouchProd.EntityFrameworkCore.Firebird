namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class FbScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
    {
        public virtual string GenerateUseProvider(string connectionString, string language)
        {
            return $".UseFirebird(\"{connectionString}\")";
        }
    }
}
