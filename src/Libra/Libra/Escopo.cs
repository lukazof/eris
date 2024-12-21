namespace Libra;

public class Escopo
{
    private Dictionary<string, Variavel> _variaveis = new Dictionary<string, Variavel>();

    public void DefinirVariavel(string identificador, Variavel variavel)
    {
        if(VariavelExiste(identificador))
            new ErroVariavelJaDeclarada(identificador).LancarErro();

        _variaveis[identificador] = variavel;
    }

    public Variavel? ObterVariavel(string identificador)
    {
        return _variaveis.TryGetValue(identificador, out var variavel) ? variavel : null;
    }

    public int? ObterIndiceVariavel(string identificador)
    {
        if(!VariavelExiste(identificador)) return -1;

        return _variaveis.Keys.ToList().IndexOf(identificador);
    }

    public bool VariavelExiste(string identificador)
    {
        return _variaveis.ContainsKey(identificador);
    }

    public void AtualizarVariavel(string identificador, object novoValor)
    {
        if (_variaveis.TryGetValue(identificador, out var variavel))
        {
            variavel.AtualizarValor(novoValor);
        }
        else
        {
            new ErroVariavelNaoDeclarada(identificador).LancarErro();
        }
    }
}