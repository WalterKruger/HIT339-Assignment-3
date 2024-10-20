using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using A3_G4.Data;
using A3_G4.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace A3_G4.Controllers
{
    public class AccountsController : Controller
    {
        private readonly A3Context _context;
        private readonly Hitdb1Context _hitdb1;
        private readonly PasswordHasher<Account> _passwordHasher;

        public AccountsController(A3Context context, Hitdb1Context hitdb1)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Account>();
            _hitdb1 = hitdb1;
        }

        // GET: Accounts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Account.ToListAsync());
        }

        // GET: Accounts/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Username,PasswordHash,UserType,UserId,CoachId")] Account account, string password)
        {
            if (string.IsNullOrEmpty(password)) { return View(account); }

            account.PasswordHash = CreatePasswordHash(password);

            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,PasswordHash,UserType,UserId,CoachId")] Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                _context.Account.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        public IActionResult Register()
        {
            var memberAccounts = _context.Account
                .Where(i => i.UserType == Account.UserTypes.Member)
                .Select(i => i.UserId)
                .ToHashSet();

            var coachAccounts = _context.Account
                .Where(i => i.UserType == Account.UserTypes.Coach)
                .Select(i => i.CoachId)
                .ToHashSet();

            var memberSelect = _hitdb1.Members
                .Where(i => !memberAccounts.Contains(i.MemberId))
                .Select(i => new { i.MemberId, FullName = $"{i.FirstName} {i.LastName}" }).ToList();
            ViewData["MemberFK"] = new SelectList(memberSelect, "MemberId", "FullName");

            var coachSelect = _hitdb1.Coaches
                .Where(i => !coachAccounts.Contains(i.CoachId))
                .Select(i => new { i.CoachId, FullName = $"{i.FirstName} {i.LastName}" }).ToList();
            ViewData["CoachFK"] = new SelectList(coachSelect, "CoachId", "FullName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            [Bind("Id,Username,PasswordHash,UserType,UserId,CoachId")] Account account, string password
        )
        {
            // TODO: Username + password combo doesn't need to be unqiue...

            // Verify that the coach/member doesn't already have an account
            bool unqiueFKRelation = false;
            switch (account.UserType)
            { 
                case Account.UserTypes.Member:
                    account.CoachId = null;
                    unqiueFKRelation = _context.Account.Where(i => i.UserId != account.UserId).Any();
                    break;
                case Account.UserTypes.Coach:
                    account.UserId = null;
                    unqiueFKRelation = _context.Account.Where(i => i.CoachId != account.CoachId).Any();
                    break;
                case Account.UserTypes.Admin:
                    account.UserId = null;
                    account.CoachId = null;
                    unqiueFKRelation = true; // No relations, so always valid
                    break;
                default:
                    // Should never reach here
                    unqiueFKRelation = false;
                    break;

            }
            bool logicValid = unqiueFKRelation && !string.IsNullOrEmpty(password);

            account.PasswordHash = CreatePasswordHash(password);

            if (ModelState.IsValid && logicValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(account);
        }

        public IActionResult Login()
        {
            ViewData["Error"] = null;
            return View();
        }

        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? ReturnUrl)
        {
            Account? user = await _context.Account.FirstOrDefaultAsync(i => i.Username == username);
            if (user == null) {
                ViewData["Error"] = "Username wasn't found";
                return View();
            }

            if (!ValidatePassword(user, password)) {
                ViewData["Error"] = "Password doesn't match username";
                return View();
            }

            // 
            string accountFK;
            switch (user.UserType) { 
                case Account.UserTypes.Admin:
                    accountFK = ""; break;

                case Account.UserTypes.Coach:
                    accountFK = user.CoachId.ToString(); break;

                case Account.UserTypes.Member:
                    accountFK = user.UserId.ToString(); break;

                default: throw new Exception();
            }

            // Store the user information so they can login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.Id.ToString()),
                new Claim("MemberId", user.UserId.ToString()),
                new Claim("CoachId", user.CoachId.ToString()),
                new Claim(ClaimTypes.Role, user.UserType.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30), // Cookie expiration
            };

            // Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties
            );

            // FIXME: This doesn't redirect properly...
            return (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))?
                Redirect(ReturnUrl) : RedirectToAction("Index", "Home");    
        }

        public IActionResult Logout()
        {
            return View();
        }

        [HttpPost, ActionName("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutConfirm()
        {
            if (!ModelState.IsValid) { 
                return View();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }




        private bool AccountExists(int id)
        {
            return _context.Account.Any(e => e.Id == id);
        }

        public bool ValidatePassword(Account user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public string CreatePasswordHash(string password)
        {
            var junkUser = new Account(); // This var is only used in custom scenarios, ignore it
            return _passwordHasher.HashPassword(junkUser, password);
        }
    }
}
