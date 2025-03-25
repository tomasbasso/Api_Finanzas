using Api_Finanzas.Models;

namespace Api_Finanzas.Services
{
    public interface IJwtService
    {
        string GenerarToken(Usuario usuario);
    }
}
