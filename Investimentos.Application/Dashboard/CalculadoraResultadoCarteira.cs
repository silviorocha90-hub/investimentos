namespace Investimentos.Application.Dashboard
{
    /// <summary>
    /// Centraliza a regra econômica usada pelos indicadores da carteira.
    ///
    /// Resultado da carteira =
    /// valorização atual dos ativos
    /// + proventos/dividendos líquidos
    /// + resultado líquido de opções.
    ///
    /// Resultado realizado em vendas de ações permanece separado e
    /// não compõe este indicador.
    /// </summary>
    public static class CalculadoraResultadoCarteira
    {
        public static decimal CalcularResultado(
            decimal valorizacaoAtivos,
            decimal totalProventos,
            decimal premioLiquidoOpcoes)
        {
            return
                valorizacaoAtivos +
                totalProventos +
                premioLiquidoOpcoes;
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
