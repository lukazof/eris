using Libra.Arvore;

namespace Libra
{
    public class FuncaoEmbutida : Funcao, IChamavel
    {
        private readonly Func<object[], object> _implementacao;

        public FuncaoEmbutida(Func<object[], object> implementacao) : base("", null, null)
        {
            _implementacao = implementacao;
        }

        public object Executar(params object[] argumentos)
        {
            return _implementacao(argumentos);
        }

    }
}