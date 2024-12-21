// TODO: Péssima forma de endereçar variáveis!
public struct Endereco
{
    public int IndiceEscopo { get; internal set; }
    public int IndiceVariavel { get; internal set; }

    public override string ToString()
    {
        return IndiceEscopo.ToString().PadLeft(8, '0') + IndiceVariavel.ToString().PadLeft(8, '0');
    }
}