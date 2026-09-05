namespace Investimentos.Importador.Importacao
{
    public static class MapeamentoAtivosHistoricos
    {
        private static readonly Dictionary<string, string> Tipos =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // BDR
                ["AURA33"] = "BDR",

                // Ações
                ["B3SA3"] = "ACAO",
                ["BBDC4"] = "ACAO",
                ["CPLE6"] = "ACAO",
                ["DIRR3"] = "ACAO",
                ["LEVE3"] = "ACAO",
                ["PETR3"] = "ACAO",
                ["PETR4"] = "ACAO",
                ["POMO4"] = "ACAO",
                ["SAPR3"] = "ACAO",
                ["VALE3"] = "ACAO",
                ["WEGE3"] = "ACAO",

                // FIIs
                ["BRCO11"] = "FII",
                ["CPTS11"] = "FII",
                ["HGLG11"] = "FII",
                ["KNHF11"] = "FII",
                ["KNRI11"] = "FII",
                ["KNSC11"] = "FII",
                ["KORE11"] = "FII",
                ["MXRF11"] = "FII",
                ["RECR11"] = "FII",
                ["VCJR11"] = "FII",
                ["VGIR11"] = "FII",
                ["VISC11"] = "FII",

                // ETFs
                ["B5P211"] = "ETF",
                ["BICL39"] = "ETF",
                ["DTCR39"] = "ETF",
                ["LFTS11"] = "ETF",
                ["LLFT11"] = "ETF",
                ["SPXR11"] = "ETF",
                ["SVAL11"] = "ETF",

                // Renda fixa
                ["LCA"] = "LCA",
                ["NEON"] = "RENDA_FIXA",

                // Previdência
                ["PREV"] = "PREVIDENCIA"
            };

        public static string? ObterTipoAtivoCodigo(
            string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                return null;
            }

            return Tipos.TryGetValue(
                ticker.Trim(),
                out var tipo)
                    ? tipo
                    : null;
        }
    }
}