using Investimentos.Application.Carteira.ConsultarCarteira;

namespace Investimentos.Application.Dashboard;

public static class CalculadoraRendimentosCarteira
{
    // Acumulado de ações e FIIs: inclui vendas de posições já zeradas e
    // proventos líquidos, sem repetir os ganhos de opções do mesmo ativo.
    // Sem cotação, não supõe valorização nem perda da posição atual.
    public static decimal CalcularAcoesFiis(IEnumerable<PosicaoAtivoDto> posicoes) =>
        posicoes
            .Where(x => x.TipoAtivoCodigo is "ACAO" or "FII")
            .Sum(x => x.ResultadoRealizado + x.Proventos +
                (x.Quantidade > 0 && x.PrecoAtual.HasValue ? x.Valorizacao : 0));
}
