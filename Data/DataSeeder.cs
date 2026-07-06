using Microsoft.AspNetCore.Identity;
using ObligatorioApiario.Models;
using Microsoft.EntityFrameworkCore;

namespace ObligatorioApiario.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "Dueño", "Peon" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user if it doesn't exist
            var adminUser = await userManager.FindByEmailAsync("admin@apiario.com");
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = "admin@apiario.com",
                    Email = "admin@apiario.com",
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(newAdmin, "Apiario2026!");
                if (createPowerUser.Succeeded)
                {
                    // Assign to Dueño role
                    await userManager.AddToRoleAsync(newAdmin, "Dueño");
                }
            }

            // Seed initial Apicultor
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var matiasExists = await dbContext.Apicultores.AnyAsync(a => a.Nombre == "Matías Verges");
            if (!matiasExists)
            {
                dbContext.Apicultores.Add(new Apicultor
                {
                    CIApicultor = 12345678, // Arbitrary CI
                    Nombre = "Matías Verges",
                    Cargo = "Administrador de Apiario"
                });
                await dbContext.SaveChangesAsync();
            }

            // --- INICIO: Generación de Datos Reales Masivos ---
            
            // Verificación Crítica: Si ya existen apiarios, detenemos el seeder para NO borrar 
            // los datos que el usuario haya creado durante el uso de la aplicación.
            if (await dbContext.Apiarios.AnyAsync())
            {
                return;
            }
            // 1. Eliminar datos existentes (en orden para respetar Foreign Keys)
            await dbContext.Tiene.ExecuteDeleteAsync();
            await dbContext.Revisa.ExecuteDeleteAsync();
            await dbContext.Realiza.ExecuteDeleteAsync();
            await dbContext.Genera.ExecuteDeleteAsync();
            await dbContext.Barriles.ExecuteDeleteAsync();
            await dbContext.Cosechas.ExecuteDeleteAsync();
            await dbContext.Aplica.ExecuteDeleteAsync();
            await dbContext.Asigna.ExecuteDeleteAsync();
            await dbContext.Colmenas.ExecuteDeleteAsync();
            await dbContext.Apiarios.ExecuteDeleteAsync();

            // 2. Generar 16 Apiarios (Max 20, 50 colmenas c/u) en Rivera
            var apiarios = new List<Apiario>();
            double baseLat = -30.8988; // Rivera
            double baseLon = -55.5506;

            for (int i = 1; i <= 16; i++)
            {
                apiarios.Add(new Apiario 
                {
                    Nombre = $"Apiario Rivera {i}",
                    Ubicacion = $"Monte de Eucalipto {i}",
                    Zona = "Rivera",
                    Latitud = (decimal)(baseLat + (i * 0.001)),
                    Longitud = (decimal)(baseLon + (i * 0.001)),
                    Cant_Colmenas = 0 // En DB un trigger puede ajustarlo luego.
                });
            }
            dbContext.Apiarios.AddRange(apiarios);
            await dbContext.SaveChangesAsync(); // Guardamos para obtener los IDs

            // 3. Generar 800 Colmenas
            var colmenas = new List<Colmena>();
            var rand = new Random();
            string[] fortalezas = { "Fuerte", "Media", "Débil" };

            for (int i = 1; i <= 800; i++)
            {
                colmenas.Add(new Colmena
                {
                    Identificador = $"COL-{i:000}",
                    Estado = "Activa",
                    Cant_Bastidores = rand.Next(8, 11),
                    Cant_Abejas_Estimada = rand.Next(20000, 60000),
                    Fortaleza_Abejas = fortalezas[rand.Next(fortalezas.Length)]
                });
            }
            dbContext.Colmenas.AddRange(colmenas);
            await dbContext.SaveChangesAsync(); // Guardamos para obtener los IDs

            // 4. Asignar las Colmenas a los Apiarios (50 por apiario)
            var tieneList = new List<Tiene>();
            int colmenaIndex = 0;

            foreach (var apiario in apiarios)
            {
                // 50 colmenas = 10 filas x 5 columnas
                for (int fila = 1; fila <= 10; fila++)
                {
                    for (int col = 1; col <= 5; col++)
                    {
                        if (colmenaIndex >= colmenas.Count) break;

                        var colmena = colmenas[colmenaIndex];
                        
                        tieneList.Add(new Tiene
                        {
                            ID_Apiario = apiario.ID_Apiario,
                            ID_Colmena = colmena.ID_Colmena,
                            Fecha_Instalacion = DateTime.UtcNow.AddDays(-rand.Next(1, 365)),
                            Sector = "Sector Principal",
                            Fila = $"Fila {fila}",
                            Columna = $"Col {col}"
                        });
                        colmenaIndex++;
                    }
                }
            }
            dbContext.Tiene.AddRange(tieneList);
            await dbContext.SaveChangesAsync();

            // 5. Generar 3 Cosechas Anteriores (~20.000 kg por cosecha)
            string[] nombresCosechas = { "Primavera 2024", "Otoño 2025", "Primavera 2025" };
            DateTime[] fechasCosechas = { new DateTime(2024, 11, 15), new DateTime(2025, 4, 10), new DateTime(2025, 11, 20) };
            
            for (int c = 0; c < 3; c++)
            {
                var cosecha = new Cosecha
                {
                    Identificador = nombresCosechas[c],
                    Observaciones = "Cosecha generada automáticamente",
                    Estado = "Completada"
                };
                dbContext.Cosechas.Add(cosecha);
                await dbContext.SaveChangesAsync(); // Guardamos para obtener el ID

                var realizaList = new List<Realiza>();
                var barrilesList = new List<Barril>();

                foreach (var apiario in apiarios)
                {
                    // Cada apiario (50 colmenas) genera aprox 4 barriles de ~300kg para llegar a ~19.200kg - 24.000kg en total
                    int cantBarriles = rand.Next(4, 6);
                    
                    realizaList.Add(new Realiza
                    {
                        ID_Cosecha = cosecha.ID_Cosecha,
                        ID_Apiario = apiario.ID_Apiario,
                        Fecha_Cosecha = fechasCosechas[c].AddDays(rand.Next(-5, 5)).ToUniversalTime(),
                        Tipo_Miel = "Eucaliptus",
                        Cant_Barriles = cantBarriles
                    });

                    for (int b = 0; b < cantBarriles; b++)
                    {
                        barrilesList.Add(new Barril
                        {
                            Cantidad_Miel = (decimal)(290 + rand.NextDouble() * 20), // ~290-310 kg
                            Precio = 1200.00m,
                            CIApicultor = 12345678 // CI de Matías Verges
                        });
                    }
                }
                
                dbContext.Realiza.AddRange(realizaList);
                dbContext.Barriles.AddRange(barrilesList);
                await dbContext.SaveChangesAsync(); // Guardamos para obtener IDs de barriles

                var generaList = new List<Genera>();
                foreach (var barril in barrilesList)
                {
                    generaList.Add(new Genera
                    {
                        ID_Barril = barril.ID_Barril,
                        ID_Cosecha = cosecha.ID_Cosecha
                    });
                }
                dbContext.Genera.AddRange(generaList);
                await dbContext.SaveChangesAsync();
            }
            
            // --- FIN: Generación de Datos Reales Masivos ---
        }
    }
}
