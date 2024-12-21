namespace Libra.Arvore
{
    public class Programa
    {
        public Programa(Instrucao[] instrucoes)
        {
            Instrucoes = instrucoes;
            Variaveis = new Dictionary<string, Variavel>();
            Funcoes = new Dictionary<string, Funcao>();
            PilhaEscopos = new PilhaDeEscopos();
        }

        public string Saida { get; internal set; }
        public int CodigoSaida { get; private set; }
        public PilhaDeEscopos PilhaEscopos { get; private set;}
        public Instrucao[] Instrucoes;
        public Dictionary<string, Variavel> Variaveis {get; private set; }
        public Dictionary<string, Funcao> Funcoes {get; private set; }
        internal bool _falha = false;

        public bool VariavelExiste(string ident)
        {
            return Variaveis.ContainsKey(ident);
        }
        public bool FuncaoExiste(string ident)
        {
            return Funcoes.ContainsKey(ident);
        }
        public void Sair(int codigo)
        {
            _falha = true;
            CodigoSaida = codigo;
        }
    }
}