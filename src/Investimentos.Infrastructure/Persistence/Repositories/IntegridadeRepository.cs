using Investimentos.Application.Administracao.Integridade;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Infrastructure.Persistence.Repositories
{
    public class IntegridadeRepository : IIntegridadeRepository
    {
        private readonly InvestimentosDbContext _context;

        public IntegridadeRepository(
            InvestimentosDbContext context)
        {
            _context = context;
        }

        public async Task<IntegridadeDto> VerificarAsync(
            CancellationToken cancellationToken = default)
        {
            var problemas =
                new List<ProblemaIntegridadeDto>();

            var operacoes =
                await _context.Operacoes
                    .AsNoTracking()
                    .Include(x => x.Ativo)
                    .Include(x => x.Investidor)
                    .Include(x => x.TipoOperacao)
                    .OrderBy(x => x.InvestidorId)
                    .ThenBy(x => x.AtivoId)
                    .ThenBy(x => x.Data)
                    .ThenBy(x => x.Sequencia)
                    .ToListAsync(cancellationToken);

            foreach (var grupo in operacoes.GroupBy(
                         x => new
                         {
                             x.InvestidorId,
                             x.AtivoId
                         }))
            {
                decimal quantidade = 0;

                foreach (var operacao in grupo)
                {
                    var tipo =
                        operacao.TipoOperacao.Codigo
                            .Trim()
                            .ToUpperInvariant();

                    if (tipo == "COMPRA")
                    {
                        quantidade += operacao.Quantidade;
                    }
                    else if (tipo == "VENDA")
                    {
                        quantidade -= operacao.Quantidade;

                        if (quantidade < 0)
                        {
                            problemas.Add(
                                new ProblemaIntegridadeDto(
                                    "VENDA_SEM_POSICAO",
                                    "ERRO",
                                    "Operacao",
                                    $"{operacao.Investidor.Nome} / {operacao.Ativo.Ticker.Codigo}",
                                    $"A venda de {operacao.Quantidade:N0} em {operacao.Data:dd/MM/yyyy} excede a posição disponível."));
                        }
                    }
                    else
                    {
                        problemas.Add(
                            new ProblemaIntegridadeDto(
                                "TIPO_OPERACAO_INVALIDO",
                                "ERRO",
                                "Operacao",
                                operacao.Id.ToString(),
                                $"Tipo de operação não reconhecido: {operacao.TipoOperacao.Codigo}."));
                    }
                }
            }

            var opcoes =
                await _context.OperacoesOpcoes
                    .AsNoTracking()
                    .Include(x => x.Ativo)
                    .Include(x => x.Investidor)
                    .ToListAsync(cancellationToken);

            foreach (var opcao in opcoes)
            {
                if (opcao.Quantidade <= 0 ||
                    opcao.Strike <= 0 ||
                    opcao.Vencimento.Date < opcao.DataOperacao.Date)
                {
                    problemas.Add(
                        new ProblemaIntegridadeDto(
                            "OPCAO_DADOS_INVALIDOS",
                            "ERRO",
                            "Opcao",
                            opcao.TickerOpcao,
                            "A opção possui quantidade, strike ou datas incompatíveis."));
                }

                if (opcao.Situacao == "ENCERRADA" &&
                    (!opcao.DataFinalizacao.HasValue ||
                     !opcao.PrecoRecompraUnitario.HasValue))
                {
                    problemas.Add(
                        new ProblemaIntegridadeDto(
                            "OPCAO_ENCERRADA_INCOMPLETA",
                            "ERRO",
                            "Opcao",
                            opcao.TickerOpcao,
                            "Opção encerrada sem data de finalização ou preço de recompra."));
                }

                if (opcao.DataFinalizacao.HasValue &&
                    opcao.DataFinalizacao.Value.Date <
                    opcao.DataOperacao.Date)
                {
                    problemas.Add(
                        new ProblemaIntegridadeDto(
                            "OPCAO_FINALIZACAO_INVALIDA",
                            "ERRO",
                            "Opcao",
                            opcao.TickerOpcao,
                            "Data de finalização anterior à data da operação."));
                }
            }

            var proventos =
                await _context.Proventos
                    .AsNoTracking()
                    .Include(x => x.Ativo)
                    .Include(x => x.Investidor)
                    .ToListAsync(cancellationToken);

            foreach (var provento in proventos)
            {
                if (provento.QuantidadeBase <= 0 ||
                    provento.ValorPorUnidade < 0 ||
                    provento.ValorRecebido < 0 ||
                    (provento.DataCom.HasValue &&
                     provento.DataPagamento.Date <
                     provento.DataCom.Value.Date))
                {
                    problemas.Add(
                        new ProblemaIntegridadeDto(
                            "PROVENTO_DADOS_INVALIDOS",
                            "ERRO",
                            "Provento",
                            $"{provento.Investidor.Nome} / {provento.Ativo.Ticker.Codigo}",
                            "O provento possui quantidade, valor ou datas incompatíveis."));
                }
            }

            var posicoesAtuais =
                operacoes
                    .GroupBy(x => x.AtivoId)
                    .Select(grupo => new
                    {
                        AtivoId = grupo.Key,
                        Quantidade = grupo.Sum(x =>
                            x.TipoOperacao.Codigo == "COMPRA"
                                ? x.Quantidade
                                : -x.Quantidade)
                    })
                    .Where(x => x.Quantidade > 0)
                    .ToList();

            var ativosComCotacao =
                (await _context.CotacoesAtivos
                    .AsNoTracking()
                    .Select(x => x.AtivoId)
                    .Distinct()
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            var ativos =
                await _context.Ativos
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        x => x.Id,
                        cancellationToken);

            foreach (var posicao in posicoesAtuais)
            {
                if (!ativosComCotacao.Contains(posicao.AtivoId) &&
                    ativos.TryGetValue(posicao.AtivoId, out var ativo) &&
                    ativo.TipoAtivo.Codigo != "PREVIDENCIA")
                {
                    problemas.Add(
                        new ProblemaIntegridadeDto(
                            "POSICAO_SEM_COTACAO",
                            "AVISO",
                            "Ativo",
                            ativo.Ticker.Codigo,
                            "Existe posição atual sem cotação cadastrada."));
                }
            }

            var duplicidadesCotacao =
                await _context.CotacoesAtivos
                    .AsNoTracking()
                    .GroupBy(x => new
                    {
                        x.AtivoId,
                        x.DataReferencia
                    })
                    .Where(x => x.Count() > 1)
                    .Select(x => new
                    {
                        x.Key.AtivoId,
                        x.Key.DataReferencia,
                        Quantidade = x.Count()
                    })
                    .ToListAsync(cancellationToken);

            foreach (var duplicidade in duplicidadesCotacao)
            {
                problemas.Add(
                    new ProblemaIntegridadeDto(
                        "COTACAO_DUPLICADA",
                        "ERRO",
                        "Cotacao",
                        duplicidade.AtivoId.ToString(),
                        $"{duplicidade.Quantidade} cotações para {duplicidade.DataReferencia:dd/MM/yyyy}."));
            }

            var duplicidadesSaldo =
                await _context.SaldosDisponiveis
                    .AsNoTracking()
                    .GroupBy(x => new
                    {
                        x.InvestidorId,
                        x.DataReferencia
                    })
                    .Where(x => x.Count() > 1)
                    .Select(x => new
                    {
                        x.Key.InvestidorId,
                        x.Key.DataReferencia,
                        Quantidade = x.Count()
                    })
                    .ToListAsync(cancellationToken);

            foreach (var duplicidade in duplicidadesSaldo)
            {
                problemas.Add(
                    new ProblemaIntegridadeDto(
                        "SALDO_DUPLICADO",
                        "ERRO",
                        "SaldoDisponivel",
                        duplicidade.InvestidorId.ToString(),
                        $"{duplicidade.Quantidade} saldos para {duplicidade.DataReferencia:dd/MM/yyyy}."));
            }

            return new IntegridadeDto(
                problemas.All(x => x.Severidade != "ERRO"),
                problemas.Count,
                problemas);
        }
    }
}
