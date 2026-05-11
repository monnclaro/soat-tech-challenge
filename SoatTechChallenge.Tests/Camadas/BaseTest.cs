using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SoatTechChallenge.Application;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Tests.Layers;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Usuario).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(SoatTechChallengeDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
