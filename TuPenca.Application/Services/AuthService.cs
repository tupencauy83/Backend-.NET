using Microsoft.AspNetCore.Identity;
using TuPenca.Application.DTOs.Auth;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;

public class AuthService : IAuthService
{
    private const string MensajePendienteAprobacion = "Tu cuenta está pendiente de aprobación por un administrador";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IFirebaseService _firebaseService;
    private readonly PasswordHasher<string> _hasher = new();

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IFirebaseService firebaseService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _firebaseService = firebaseService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, Guid? sitioId)
    {

        // ¿Viene con SitioId? → es Usuario del sitio (común o admin de sitio)
        if (sitioId != null)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId.Value);
            if (sitio == null || sitio.Estado != EstadoSitio.Activo)
                throw new Exception("Este sitio no está disponible");

            var usuario = await _unitOfWork.Usuarios
                .GetByEmailAsync(request.Email, sitioId.Value);

            if (usuario == null) return null;

            var resultado = _hasher.VerifyHashedPassword(
                null!, usuario.PasswordHash, request.Password);

            if (resultado == PasswordVerificationResult.Failed)
                return null;

            Console.WriteLine($"Usuario encontrado: {usuario.Email}");
            Console.WriteLine($"Hash DB: {usuario.PasswordHash}");
            Console.WriteLine($"Estado: {usuario.Estado}");
            Console.WriteLine($"Rol: {usuario.Rol}");

            // VERIFICACION DE ESTADO DE USUARIO, SI ES PENDIENTE O RECHAZADO NO SE LOGUEA.
            if (usuario.Estado == EstadoUsuario.Pendiente)
                throw new Exception("Tu cuenta está pendiente de aprobación por un administrador");

            if (usuario.Estado == EstadoUsuario.Rechazado)
                throw new Exception("Tu cuenta fue rechazada");

            // El rol viene del campo Rol de la entidad
            var rolClaim = usuario.Rol == RolUsuario.AdministradorSitio
                ? "AdministradorSitio"
                : "UsuarioComun";

            var token = _jwtService.GenerarToken(
               usuario.Id.ToString(),
               usuario.Email,
               usuario.Nombre,
               rolClaim,
               usuario.SitioId.ToString() // ← agregado
            );

            return new LoginResponseDto
            {
                Token = token,
                Nombre = usuario.Nombre,
                Rol = rolClaim,
                Expira = DateTime.UtcNow.AddHours(8)
            };
        }
        else
        {
            // Sin SitioId → es Administrador de plataforma
            var admins = await _unitOfWork.Administrador.GetAllAsync();
            var admin = admins.FirstOrDefault(a => a.Email == request.Email);

            if (admin == null) return null;

            var resultado = _hasher.VerifyHashedPassword(
                null!, admin.PasswordHash, request.Password);

            if (resultado == PasswordVerificationResult.Failed)
                return null;

            // Login de AdministradorPlataforma — sin SitioId
            var token = _jwtService.GenerarToken(
                admin.Id.ToString(),
                admin.Email,
                admin.Email,
                "AdministradorPlataforma"
            // sin sitioId → queda null
            );

            return new LoginResponseDto
            {
                Token = token,
                Nombre = admin.Email,
                Rol = "AdministradorPlataforma",
                Expira = DateTime.UtcNow.AddHours(8)
            };
        }
    }

    public async Task<LoginResponseDto> LoginFirebaseAsync(string idToken, Guid? sitioId)
    {
        if (sitioId == null)
            throw new Exception("No se pudo determinar el sitio");

        var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId.Value);
        if (sitio == null)
            throw new Exception("Sitio no encontrado");

        if (sitio.Estado != EstadoSitio.Activo)
            throw new Exception("Este sitio no está disponible");

        // Validar token con Firebase
        var decodedToken = await _firebaseService.VerifyTokenAsync(idToken);

        // Extraer datos
        var email = decodedToken.Claims["email"]?.ToString();
        var nombre = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"]?.ToString() : email;

        if (string.IsNullOrEmpty(email))
            throw new Exception("Token inválido: no contiene email");

        // Buscar usuario en DB
        var usuario = await _unitOfWork.Usuarios
            .GetByEmailAsync(email, sitioId.Value);

        if (sitio.TipoRegistro == TipoRegistro.Cerrada)
            throw new Exception("Este sitio no acepta registros");

        // Si no existe, crearlo respetando la política de registro del sitio
        if (usuario == null)
        {
            if (sitio.TipoRegistro == TipoRegistro.Con_Invitacion)
            {
                var invitaciones = await _unitOfWork.Invitaciones.GetAllAsync();
                var invitacion = invitaciones.FirstOrDefault(i =>
                    i.EmailInvitado == email &&
                    !i.Aceptada &&
                    i.SitioId == sitioId.Value);

                if (invitacion == null)
                    throw new Exception("Este sitio requiere una invitación válida");

                invitacion.Aceptada = true;
                await _unitOfWork.Invitaciones.UpdateAsync(invitacion);
            }

            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = nombre ?? email,
                Email = email,
                PasswordHash = "",
                Rol = RolUsuario.UsuarioComun,
                FechaRegistro = DateTime.UtcNow,
                Estado = sitio.TipoRegistro == TipoRegistro.Abierta
                    ? EstadoUsuario.Aprobado
                    : EstadoUsuario.Pendiente,
                ProveedorAuth = ProveedorAuth.Google,
                SitioId = sitioId.Value
            };
            await _unitOfWork.Usuarios.AddAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        // Validar estado
        if (usuario.Estado == EstadoUsuario.Pendiente)
            throw new Exception(MensajePendienteAprobacion);

        if (usuario.Estado == EstadoUsuario.Rechazado)
            throw new Exception("Tu cuenta fue rechazada");

        // Generar JWT
        var rolClaim = usuario.Rol == RolUsuario.AdministradorSitio
            ? "AdministradorSitio"
            : "UsuarioComun";

        var token = _jwtService.GenerarToken(
            usuario.Id.ToString(),
            usuario.Email,
            usuario.Nombre,
            rolClaim,
            usuario.SitioId.ToString()
        );

        return new LoginResponseDto
        {
            Token = token,
            Nombre = usuario.Nombre,
            Rol = rolClaim,
            Expira = DateTime.UtcNow.AddHours(8)
        };
    }

    public async Task<RegistroResponseDto> RegistrarUsuarioAsync(RegistroUsuarioRequestDto request, Guid? sitioId)
    {
        var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId.Value);
        if (sitio == null)
            throw new Exception("Sitio no encontrado");

        if (sitio.TipoRegistro == TipoRegistro.Cerrada)
            throw new Exception("Este sitio no acepta registros");

        var existente = await _unitOfWork.Usuarios
            .GetByEmailAsync(request.Email, sitioId.Value);
        if (existente != null)
            throw new Exception("El email ya está en uso en este sitio");

        if (sitio.TipoRegistro == TipoRegistro.Con_Invitacion)
        {
            if (string.IsNullOrEmpty(request.CodigoInvitacion))
                throw new Exception("Este sitio requiere un código de invitación");

            var invitaciones = await _unitOfWork.Invitaciones.GetAllAsync();
            var invitacion = invitaciones.FirstOrDefault(i =>
                i.Codigo == request.CodigoInvitacion &&
                i.EmailInvitado == request.Email &&
                !i.Aceptada);

            if (invitacion == null)
                throw new Exception("Código de invitación inválido o ya utilizado");

            invitacion.Aceptada = true;
            await _unitOfWork.Invitaciones.UpdateAsync(invitacion);
        }

        var estadoInicial = sitio.TipoRegistro == TipoRegistro.Abierta
            ? EstadoUsuario.Aprobado
            : EstadoUsuario.Pendiente;

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            SitioId = sitioId.Value,
            Estado = estadoInicial,
            Rol = request.Rol, // ← viene del request, puede ser UsuarioComun o AdministradorSitio
            FechaRegistro = DateTime.UtcNow
        };

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        var mensaje = estadoInicial == EstadoUsuario.Aprobado
            ? "Registro exitoso"
            : MensajePendienteAprobacion;

        return new RegistroResponseDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            Mensaje = mensaje
        };
    }

    public async Task<RegistroResponseDto> RegistrarAdminAsync(RegistroAdminRequestDto request)
    {
        // Verificar que no exista ya un admin con ese email
        var admins = await _unitOfWork.Administrador.GetAllAsync();
        var existente = admins.FirstOrDefault(a => a.Email == request.Email);
        if (existente != null)
            throw new Exception("El email ya está en uso");

        var admin = new Administrador
        {
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FechaRegistro = DateTime.UtcNow
            // Sin SitioId
        };

        await _unitOfWork.Administrador.AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        return new RegistroResponseDto
        {
            Id = admin.Id,
            Email = admin.Email,
            Mensaje = "Administrador de plataforma registrado exitosamente"
        };
    }


    /// INVITACIONES - SERVICES

    public async Task<IEnumerable<UsuarioPendienteDto>> ObtenerUsuariosPendientesAsync(Guid sitioId)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        return usuarios
            .Where(u => u.SitioId == sitioId && u.Estado == EstadoUsuario.Pendiente)
            .Select(u => new UsuarioPendienteDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                FechaRegistro = u.FechaRegistro
            });
    }

    public async Task<string> AprobarUsuarioAsync(Guid usuarioId)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
        if (usuario == null)
            throw new Exception("Usuario no encontrado");

        if (usuario.Estado != EstadoUsuario.Pendiente)
            throw new Exception("El usuario no está pendiente de aprobación");

        usuario.Estado = EstadoUsuario.Aprobado;
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return "Usuario aprobado exitosamente";
    }

    public async Task<string> RechazarUsuarioAsync(Guid usuarioId)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
        if (usuario == null)
            throw new Exception("Usuario no encontrado");

        if (usuario.Estado != EstadoUsuario.Pendiente)
            throw new Exception("El usuario no está pendiente de aprobación");

        usuario.Estado = EstadoUsuario.Rechazado;
        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return "Usuario rechazado";
    }

    /// ////////////////////////////////////////////////////

    public string HashPassword(string password)
        => _hasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string hash)
        => _hasher.VerifyHashedPassword(null!, hash, password)
           != PasswordVerificationResult.Failed;
}