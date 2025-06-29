using Orders.Attributes;
using Orders.Core.Security;

namespace Orders.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Este middleware de autorização utiliza a implementação de IAuthorizer
            // para verificar se a chave de autorização está presente e é válida.
            // A implementação de IAuthorizer é quem carrega a lógica de autorização.
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                // Verifica se o endpoint possui o atributo RequireAuthorizedKeyAttribute.
                // Apenas os endpoints que possuem esse atributo serão validados.
                var requireAuthKey = endpoint.Metadata.GetMetadata<RequireAuthorizedKeyAttribute>();

                if (requireAuthKey == null)
                {
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var authorizer = context.RequestServices.GetRequiredService<IAuthorizer>();
            var key = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(key) || !await authorizer.Authorize(key))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            await _next(context);
        }
    }
}