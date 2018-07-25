
using IdentityContext.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Extensions;
namespace IdentityContext.Data
{
    public class IdentityDbContextSeed
    {
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public async Task SeedAsync(IdentityAppContext context, IHostingEnvironment env,
            ILogger<IdentityDbContextSeed> logger, IOptions<AppSettings> settings,
            UserManager<ApplicationUser> manager, RoleManager<IdentityRole> roleManager,
            int? retry = 0)
        {
            int retryForAvaiability = retry.Value;
 
            try
            {
                var useCustomizationData = settings.Value.UserCustomizationData;
                var contentRootPath = env.ContentRootPath;
                var webroot = env.WebRootPath;

                var defaultUsers = GetDefaultUser();

                if (!context.Users.Any())
                {
                    context.Users.AddRange(useCustomizationData
                        ? GetUsersFromFile(contentRootPath, logger)
                        : defaultUsers);

                    await context.SaveChangesAsync();
                }

                var roles = new List<IdentityRole>()
                {
                    new IdentityRole(){ Name ="Admin"},
                    new IdentityRole(){ Name ="HR"},
                    new IdentityRole(){ Name ="Candidate"},
                    new IdentityRole(){ Name ="Employee"},
                    new IdentityRole(){ Name ="ExternalUser"},
                };

                if (!context.Roles.Any())
				{ 

                    foreach (var role in roles)
                    {
                        var result = await roleManager.CreateAsync(role);
                    }

                    var defaultAdmin = defaultUsers.FirstOrDefault(u => u.Email.ToLower().Contains("admin"));

                    var asAdmin =await  manager.AddToRoleAsync(defaultAdmin, "Admin");
                }
                if (useCustomizationData)
                {
                    //GetPreconfiguredImages(contentRootPath, webroot, logger);
                }


            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;

                    logger.LogError(ex.Message, $"There is an error migrating data for ApplicationDbContext");

                    await SeedAsync(context, env, logger, settings, manager, roleManager, retryForAvaiability);
                }
            }
        }

        private IEnumerable<ApplicationUser> GetUsersFromFile(string contentRootPath, ILogger logger)
        {
            string csvFileUsers = Path.Combine(contentRootPath, "Setup", "Users.csv");

            if (!File.Exists(csvFileUsers))
            {
                return GetDefaultUser();
            }

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = {
                   
                    "email",  "lastname", "name", "phonenumber",
                    "username" , "securitynumber",
                    "normalizedemail", "normalizedusername", "password"
                };
                csvheaders = GetHeaders(requiredHeaders, csvFileUsers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                return GetDefaultUser();
            }

            List<ApplicationUser> users = File.ReadAllLines(csvFileUsers)
                        .Skip(1) // skip header column
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .SelectTry(column => CreateApplicationUser(column, csvheaders))
                        .OnCaughtException(ex => { logger.LogError(ex.Message); return null; })
                        .Where(x => x != null)
                        .ToList();

            return users;
        }

        private ApplicationUser CreateApplicationUser(string[] column, string[] headers)
        {
            if (column.Count() != headers.Count())
            {
                throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
            }

            string cardtypeString = column[Array.IndexOf(headers, "cardtype")].Trim('"').Trim();
            if (!int.TryParse(cardtypeString, out int cardtype))
            {
                throw new Exception($"cardtype='{cardtypeString}' is not a number");
            }

            var user = new ApplicationUser
            {
                 
                Email = column[Array.IndexOf(headers, "email")].Trim('"').Trim(),
                
                Id = Guid.NewGuid().ToString(),
                LastName = column[Array.IndexOf(headers, "lastname")].Trim('"').Trim(),
                Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
                PhoneNumber = column[Array.IndexOf(headers, "phonenumber")].Trim('"').Trim(),
                UserName = column[Array.IndexOf(headers, "username")].Trim('"').Trim(),
                
                NormalizedEmail = column[Array.IndexOf(headers, "normalizedemail")].Trim('"').Trim(),
                NormalizedUserName = column[Array.IndexOf(headers, "normalizedusername")].Trim('"').Trim(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = column[Array.IndexOf(headers, "password")].Trim('"').Trim(), // Note: This is the password
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);

            return user;
        }

        private IEnumerable<ApplicationUser> GetDefaultUser()
        {
            var user =
            new ApplicationUser()
            {
                
                Email = "admin@focusservices.com",
                
                Id = Guid.NewGuid().ToString(),
                LastName = "DemoLastName",
                Name = "DemoUser",
                PhoneNumber = "1234567890",
                UserName = "admin@focusservices.com",
               
                NormalizedEmail = "ADMIN@FOCUSSERVICES.COM",
                NormalizedUserName = "ADMIN@FOCUSSERVICES.COM",
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, "1234567");

            return new List<ApplicationUser>()
            {
                user
            };
        }

        static string[] GetHeaders(string[] requiredHeaders, string csvfile)
        {
            string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

            if (csvheaders.Count() != requiredHeaders.Count())
            {
                throw new Exception($"requiredHeader count '{ requiredHeaders.Count()}' is different then read header '{csvheaders.Count()}'");
            }

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!csvheaders.Contains(requiredHeader))
                {
                    throw new Exception($"does not contain required header '{requiredHeader}'");
                }
            }

            return csvheaders;
        }

        static void GetPreconfiguredImages(string contentRootPath, string webroot, ILogger logger)
        {
            try
            {
                string imagesZipFile = Path.Combine(contentRootPath, "Setup", "images.zip");
                if (!File.Exists(imagesZipFile))
                {
                    logger.LogError($" zip file '{imagesZipFile}' does not exists.");
                    return;
                }

                string imagePath = Path.Combine(webroot, "images");
                string[] imageFiles = Directory.GetFiles(imagePath).Select(file => Path.GetFileName(file)).ToArray();

                using (ZipArchive zip = ZipFile.Open(imagesZipFile, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (imageFiles.Contains(entry.Name))
                        {
                            string destinationFilename = Path.Combine(imagePath, entry.Name);
                            if (File.Exists(destinationFilename))
                            {
                                File.Delete(destinationFilename);
                            }
                            entry.ExtractToFile(destinationFilename);
                        }
                        else
                        {
                            logger.LogWarning($"Skip file '{entry.Name}' in zipfile '{imagesZipFile}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception in method GetPreconfiguredImages WebMVC. Exception Message={ex.Message}");
            }
        }
    }
}

