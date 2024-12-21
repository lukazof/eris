using Libra.Arvore;

namespace Libra; 

public class PilhaDeEscopos
{
    private Stack<Escopo> escopos = new Stack<Escopo>();

    public PilhaDeEscopos()
    {
        // Adiciona o escopo global ao iniciar
        EmpilharEscopo();
    }

    // Adiciona um novo escopo à pilha (usado para blocos locais, funções, etc.)
    public void EmpilharEscopo()
    {
        if(escopos.Count < 1000)
            escopos.Push(new Escopo());
        else
            throw new ErroTransbordoDePilha();
        
    }

    // Remove o escopo atual da pilha, caso não seja o global
    public void DesempilharEscopo()
    {
        if (escopos.Count > 1)
            escopos.Pop();
    }

    // Define uma variável no escopo atual
    public void DefinirVariavel(string identificador, Variavel variavel)
    {
        escopos.Peek().DefinirVariavel(identificador, variavel);
    }

    // Busca uma variável, começando pelo escopo mais interno até o global
    public Variavel? ObterVariavel(string identificador)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            if (escopos.ElementAt(i).VariavelExiste(identificador))
                return escopos.ElementAt(i)?.ObterVariavel(identificador);
        }

        throw new ErroVariavelNaoDeclarada(identificador);
        
        return null;
    }

    // TODO: Encontrar forma melhor
    public Endereco? ObterIndiceVariavel(string identificador)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            if (escopos.ElementAt(i).VariavelExiste(identificador))
            {
                int? idcVariavel = escopos.ElementAt(i)?.ObterIndiceVariavel(identificador);
                return new Endereco {IndiceEscopo = i, IndiceVariavel = (int)idcVariavel};
            }
                
        }
        throw new ErroVariavelNaoDeclarada(identificador);
        return null; // Variável não encontrada
    }

    // Atualiza o valor de uma variável existente
    public void AtualizarVariavel(string identificador, object novoValor)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            var escopo = escopos.ElementAt(i);

            if (escopo.VariavelExiste(identificador))
            {
                escopo.AtualizarVariavel(identificador, novoValor);
                return;
            }
        }
        throw new ErroVariavelNaoDeclarada(identificador);
    }
}
