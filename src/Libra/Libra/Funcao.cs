using Libra.Arvore;

namespace Libra
{
    public class Funcao
    {
            public string Identificador;
            public Instrucao[] Instrucoes;
            public List<string> Parametros;

            public Funcao(string ident, Instrucao[] instrucoes, List<string> parametros)
            {
                Identificador = ident;
                Instrucoes = instrucoes;
                Parametros = parametros;
            }

    }
}