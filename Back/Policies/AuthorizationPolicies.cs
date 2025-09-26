using Microsoft.AspNetCore.Authorization;

namespace Back.Policies
{
    public static class AuthorizationPolicies
    {
        public static void ConfigurePolicies(AuthorizationOptions options)
        {
            // Менеджер имеет все права инженера и наблюдателя
            options.AddPolicy("ManagerOnly", policy => 
                policy.RequireRole("Manager"));
                
            options.AddPolicy("EngineerOrAbove", policy => 
                policy.RequireRole("Engineer", "Manager"));
                
            options.AddPolicy("ObserverOrAbove", policy => 
                policy.RequireRole("Observer", "Engineer", "Manager"));

            // Специфичные политики для разных операций
            options.AddPolicy("CanManageUsers", policy =>
                policy.RequireRole("Manager"));
                
            options.AddPolicy("CanManageProjects", policy =>
                policy.RequireRole("Manager"));
                
            options.AddPolicy("CanManageDefects", policy =>
                policy.RequireRole("Engineer", "Manager"));
                
            options.AddPolicy("CanViewAll", policy =>
                policy.RequireRole("Observer", "Engineer", "Manager"));
        }
    }
}