namespace SoatTechChallenge.Domain.OrdensServico.Enums;

public enum OrdemServicoStatus
{
    Recebida = 0,
    EmDiagnostico = 1,
    AguardandoAprovacao = 2,
    EmExecucao = 3,
    Finalizada = 4,
    Entregue = 5
}