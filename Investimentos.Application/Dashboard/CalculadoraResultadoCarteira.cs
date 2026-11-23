namespace Investimentos.Application.Dashboard
{
    /// <summary>
    /// Centraliza a regra econômica usada pelos indicadores da carteira.
    ///
    /// Resultado da carteira =
    /// valorização atual dos ativos
    /// + proventos/dividendos líquidos
    /// + resultado líquido de opções
    /// + lucros/prejuízos realizados nas vendas, inclusive de posições zeradas.
    /// </summary>
    public static class CalculadoraResultadoCarteira
    {
        public static decimal CalcularResultado(
            decimal valorizacaoAtivos,
            decimal totalProventos,
            decimal premioLiquidoOpcoes,
            decimal resultadoRealizadoVendas)
        {
            return
                valorizacaoAtivos +
                totalProventos +
                premioLiquidoOpcoes +
                resultadoRealizadoVendas;
        }

        public static decimal CalcularCapitalBase(
            decimal patrimonioAtual,
            decimal resultadoCarteira)
        {
            return
                patrimonioAtual -
                resultadoCarteira;
        }

        public static decimal CalcularRentabilidade(
            decimal patrimonioAtual,
            decimal resultadoCarteira)
        {
            var capitalBase =
                CalcularCapitalBase(
                    patrimonioAtual,
                    resultadoCarteira);

            return capitalBase > 0
                ? resultadoCarteira /
                  capitalBase * 100
                : 0;
        }
    }
}
