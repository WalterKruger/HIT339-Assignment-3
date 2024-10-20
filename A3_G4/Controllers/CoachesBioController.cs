using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using A3_G4.Data;
using A3_G4.Models;

namespace A3_G4.Controllers
{
    public class CoachesBioController : Controller
    {
        private readonly Hitdb1Context _context;

        public CoachesBioController(Hitdb1Context context)
        {
            _context = context;
        }

        // GET: CoachesBio
        public async Task<IActionResult> Index()
        {
            return View(await _context.Coaches.ToListAsync());
        }

        // GET: CoachesBio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachBio = await _context.Coaches
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coachBio == null)
            {
                return NotFound();
            }

            return View(coachBio);
        }

        // GET: CoachesBio/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CoachesBio/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CoachId,FirstName,LastName,Biography,Photo")] CoachBio coachBio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(coachBio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coachBio);
        }

        // GET: CoachesBio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachBio = await _context.Coaches.FindAsync(id);
            if (coachBio == null)
            {
                return NotFound();
            }
            return View(coachBio);
        }

        // POST: CoachesBio/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CoachId,FirstName,LastName,Biography,Photo")] CoachBio coachBio)
        {
            if (id != coachBio.CoachId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coachBio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoachExists(coachBio.CoachId))
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
            return View(coachBio);
        }

        // GET: CoachesBio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coachBio = await _context.Coaches
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coachBio == null)
            {
                return NotFound();
            }

            return View(coachBio);
        }

        // POST: CoachesBio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coachBio = await _context.Coaches.FindAsync(id);
            if (coachBio != null)
            {
                _context.Coaches.Remove(coachBio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoachExists(int id)
        {
            return _context.Coaches.Any(e => e.CoachId == id);
        }
    }
}
