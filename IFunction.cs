using SMath.Manager;
using SMath.Math;
using SMath.Math.Numeric;


namespace XYPlotPluginSeq
{
    public interface IFunction 
    {
        Term Info { get; }

        TermInfo GetTermInfo( string lang );
        bool TryEvaluateExpression( Entry value, Store context, ref Entry result );
        bool NumericEvaluation( Term value, TNumber[] args, ref TNumber result );        
    }
}
