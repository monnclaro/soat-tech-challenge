namespace SoatTechChallenge.Host.Common.DTOs;

public class PagedResponse<T>
{
    public List<T> Itens { get; }
    public int Total { get; }
    public int Pagina { get; }
    public int Tamanho { get; }
    public int TotalPaginas => (int)Math.Ceiling((double)Total / Tamanho);

    public PagedResponse(List<T> itens, int total, int pagina, int tamanho)
    {
        Itens = itens;
        Total = total;
        Pagina = pagina;
        Tamanho = tamanho;
    }
}