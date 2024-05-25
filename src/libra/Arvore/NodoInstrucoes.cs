namespace Libra.Arvore
{
    public abstract class NodoInstrucao : Nodo {} // TODO: Por que não interface? Não me pergunte

    public class NodoInstrucaoSair : NodoInstrucao
    {
        public NodoInstrucaoSair(NodoExpressao expressao)
        {
            Expressao = expressao;
        }

        public NodoExpressao Expressao { get; private set; }

        public override object Avaliar()
        {
            return Expressao.Avaliar();
        }
    }

    public class NodoInstrucaoVar : NodoInstrucao
    {
        public NodoInstrucaoVar(Variavel var)
        {
            m_var = var;
        }

        private Variavel m_var;

        public override object Avaliar()
        {
            return m_var;
        }
    }

    public class NodoInstrucaoExibir: NodoInstrucao
    {
        public NodoInstrucaoExibir(NodoString str)
        {
            m_string = str;
        }

        private NodoString m_string;

        public override object Avaliar()
        {
            return m_string.Avaliar();
        }
    }
}