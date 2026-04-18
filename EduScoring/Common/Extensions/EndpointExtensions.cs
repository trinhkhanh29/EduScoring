using System.Reflection;

namespace EduScoring.Common.Extensions;

/// <summary>
/// Convention: mỗi Endpoint class phải có static method Map[FeatureName]Endpoint(this IEndpointRouteBuilder app).
/// Thêm feature mới → tự động được map, không cần sửa Program.cs.
/// </summary>
public static class EndpointExtensions
{
    public static void MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Tìm tất cả static class tên kết thúc bằng "Endpoint"
        var endpointTypes = assembly.GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && // static class = abstract + sealed
                        t.Name.EndsWith("Endpoint"))
            .ToList();

        var mapped = 0;
        foreach (var type in endpointTypes)
        {
            // Tìm method Map*Endpoint(this IEndpointRouteBuilder)
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m =>
                    m.Name.StartsWith("Map") &&
                    m.Name.EndsWith("Endpoint") &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(IEndpointRouteBuilder));

            if (method is null) continue;

            method.Invoke(null, new object[] { app });
            mapped++;
        }

        Console.WriteLine($"[STARTUP][ENDPOINTS] Đã tự động map {mapped} Endpoints.");
    }
}