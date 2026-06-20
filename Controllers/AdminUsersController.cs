using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemMagazynu.Models;
using SystemMagazynu.ViewModels;

namespace SystemMagazynu.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager,
                                     RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /AdminUsers
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var list = new List<AdminUserListItem>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new AdminUserListItem
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    Role = string.Join(", ", roles)
                });
            }

            return View(list);
        }

        // GET: /AdminUsers/Create
        public IActionResult Create()
        {
            PopulateRoles();
            return View();
        }

        // POST: /AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Role))
                ModelState.AddModelError("Role", "Wybrana rola nie istnieje.");

            if (!ModelState.IsValid)
            {
                PopulateRoles();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error));

                PopulateRoles();
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "Użytkownik został utworzony.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminUsers/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "",
                IsActive = user.IsActive
            };

            PopulateRoles();
            return View(model);
        }

        // POST: /AdminUsers/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateRoles();
                return View(model);
            }

            // Zabezpieczenie: administrator nie może zdezaktywować ani odebrać roli samemu sobie
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId && (!model.IsActive || model.Role != "Administrator"))
            {
                ModelState.AddModelError(string.Empty, "Nie możesz zdezaktywować ani zmienić roli własnego konta administratora.");
                PopulateRoles();
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsActive = model.IsActive;

            await _userManager.UpdateAsync(user);

            // Aktualizacja roli (jedna rola na użytkownika w tym systemie)
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "Dane użytkownika zostały zaktualizowane.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminUsers/ResetPassword/{id}
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new ResetPasswordViewModel
            {
                Id = user.Id,
                Email = user.Email ?? ""
            };

            return View(model);
        }

        // POST: /AdminUsers/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error));

                return View(model);
            }

            TempData["Success"] = "Hasło użytkownika zostało zresetowane.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/ToggleActive/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "Nie możesz zmienić statusu własnego konta.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsActive
                ? "Konto zostało aktywowane."
                : "Konto zostało dezaktywowane.";

            return RedirectToAction(nameof(Index));
        }

        private void PopulateRoles()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
        }

        private static string TranslateIdentityError(IdentityError error) => error.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail" => "Użytkownik z takim adresem email już istnieje.",
            "PasswordTooShort" => "Hasło jest za krótkie (min. 8 znaków).",
            "PasswordRequiresDigit" => "Hasło musi zawierać cyfrę.",
            "PasswordRequiresLower" => "Hasło musi zawierać małą literę.",
            "PasswordRequiresUpper" => "Hasło musi zawierać wielką literę.",
            _ => error.Description
        };
    }
}
